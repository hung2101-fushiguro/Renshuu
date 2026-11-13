using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options => {
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
}).AddSessionStateTempDataProvider();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient<QuestionAnalysisService>(client => client.Timeout = TimeSpan.FromMinutes(3));
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<ImportService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.AddAuthorization(o => o.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));
builder.Services.ConfigureApplicationCookie(o => {
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Denied";
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var isFreshDatabase = (await db.Database.GetPendingMigrationsAsync()).Any();
    await db.Database.MigrateAsync();

    // Explicit local setup: grant Admin only to an existing account named in user secrets.
    var adminEmail = builder.Configuration["DevelopmentAdmin:Email"];
    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var admin = await users.FindByEmailAsync(adminEmail);
        if (admin is not null)
        {
            if (!await roles.RoleExistsAsync("Admin"))
            {
                var created = await roles.CreateAsync(new IdentityRole("Admin"));
                if (!created.Succeeded) throw new InvalidOperationException("Cannot create Admin role.");
            }
            var granted = await users.AddToRoleAsync(admin, "Admin");
            if (!granted.Succeeded && !await users.IsInRoleAsync(admin, "Admin"))
                throw new InvalidOperationException("Cannot grant Admin role.");
        }
    }

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
