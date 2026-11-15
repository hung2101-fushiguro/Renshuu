using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
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
    options.Conventions.AuthorizePage("/Admin/Accounts", Security.AdminPolicy);
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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var isFreshDatabase = (await db.Database.GetPendingMigrationsAsync()).Any();
    await db.Database.MigrateAsync();

    await DevelopmentAccounts.EnsureAdminAsync(scope.ServiceProvider, builder.Configuration);

    if (isFreshDatabase)
    {
        await DbSeeder.SeedAsync(db);
    }
}

app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

public static class Security
{
    public const string AdminPolicy = "AdminOnly";
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
/// Bootstraps the first administrator only through explicit development configuration
/// (user secrets or environment variables). Never promotes arbitrary accounts.
/// An existing account is only granted the role; a missing account is created when a
/// password is configured. Both "DevelopmentAdmin:Password" and the older
/// "DevelopmentAdmin:LoginPassword" key are accepted.
/// </summary>
public static class DevelopmentAccounts
{
    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["DevelopmentAdmin:Email"];
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var password = configuration["DevelopmentAdmin:Password"]
            ?? configuration["DevelopmentAdmin:LoginPassword"];

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
                    "Không thể tạo tài khoản quản trị từ cấu hình DevelopmentAdmin. " +
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
}
