using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AuthorizeFolder("/Admin", Security.AdminPolicy);
}).AddSessionStateTempDataProvider();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<AccountAdminService>();

// Sign-in and registration posts are throttled per client IP. GET requests are not limited.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(Security.AuthFormPolicy, httpContext =>
    {
        if (!HttpMethods.IsPost(httpContext.Request.Method))
        {
            return RateLimitPartition.GetNoLimiter("auth-get");
        }

        var client = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            $"{client}:{httpContext.Request.Path}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
            });
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsFactory>();
builder.Services.AddAuthorization(options =>
    options.AddPolicy(Security.AdminPolicy, policy => policy.RequireRole(CurrentUser.AdminRole)));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var isFreshDatabase = false;

    if (app.Environment.IsDevelopment())
    {
        isFreshDatabase = (await db.Database.GetPendingMigrationsAsync()).Any();
        await db.Database.MigrateAsync();
    }

    await AdminBootstrap.EnsureAdminAsync(
        scope.ServiceProvider, builder.Configuration, app.Environment, app.Logger);

    if (isFreshDatabase)
    {
        await DbSeeder.SeedAsync(db);
    }
}

app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

public static class Security
{
    public const string AdminPolicy = "AdminOnly";
    public const string AuthFormPolicy = "AuthForms";
}

/// <summary>
/// Adds the display name to the authentication cookie so the shared layout can render
/// the signed-in account without a database round-trip.
/// </summary>
public class ApplicationUserClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public ApplicationUserClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("DisplayName", user.NameOrEmail));
        return identity;
    }
}

/// <summary>
/// Bootstraps the first administrator from explicit configuration only:
/// user secrets in Development, environment variables (BootstrapAdmin__Email /
/// BootstrapAdmin__Password) elsewhere. Never promotes arbitrary accounts and never
/// falls back to a default password. An existing account is only granted the role;
/// a missing account is created when a password is configured. The legacy
/// "DevelopmentAdmin:*" keys are still read so existing local setups keep working.
/// </summary>
public static class AdminBootstrap
{
    public static async Task EnsureAdminAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger logger)
    {
        var email = FirstValue(configuration, "BootstrapAdmin:Email", "DevelopmentAdmin:Email");
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning(environment.IsDevelopment()
                ? "Chưa cấu hình quản trị viên đầu tiên. Đặt BootstrapAdmin:Email và " +
                  "BootstrapAdmin:Password bằng 'dotnet user-secrets set' trong project " +
                  "TracNghiemWeb rồi chạy lại. Ứng dụng không có mật khẩu quản trị mặc định."
                : "Chưa cấu hình BootstrapAdmin:Email/BootstrapAdmin:Password. Ứng dụng " +
                  "không tạo tài khoản quản trị nào. Nếu database chưa có quản trị viên, hãy " +
                  "đặt biến môi trường BootstrapAdmin__Email và BootstrapAdmin__Password rồi " +
                  "khởi động lại.");
            return;
        }

        email = email.Trim().ToLowerInvariant();
        var password = FirstValue(
            configuration,
            "BootstrapAdmin:Password",
            "DevelopmentAdmin:Password",
            "DevelopmentAdmin:LoginPassword");

        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync(CurrentUser.AdminRole))
        {
            var created = await roles.CreateAsync(new IdentityRole(CurrentUser.AdminRole));
            if (!created.Succeeded)
            {
                throw new InvalidOperationException("Không thể tạo vai trò Admin.");
            }
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(email);
        if (admin is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning(
                    "Đã cấu hình email quản trị {Email} nhưng thiếu mật khẩu ban đầu. Đặt " +
                    "BootstrapAdmin:Password (hoặc biến môi trường BootstrapAdmin__Password) " +
                    "để tạo tài khoản quản trị đầu tiên.",
                    email);
                return;
            }

            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Quản trị viên",
            };

            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Không thể tạo tài khoản quản trị từ cấu hình BootstrapAdmin. " +
                    string.Join(" ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await users.IsInRoleAsync(admin, CurrentUser.AdminRole))
        {
            var granted = await users.AddToRoleAsync(admin, CurrentUser.AdminRole);
            if (!granted.Succeeded)
            {
                throw new InvalidOperationException("Không thể gán vai trò Admin.");
            }
        }
    }

    private static string? FirstValue(IConfiguration configuration, params string[] keys) =>
        keys.Select(configuration.GetSection)
            .Select(section => section.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}

