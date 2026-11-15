using Microsoft.AspNetCore.Identity;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace TracNghiemWeb.Services.Security;

public static class AccountBootstrap
{
    public static async Task EnsureRolesAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AppRoles.User, AppRoles.Admin })
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }
    }

    public static async Task<bool> HandleCommandAsync(WebApplication app, string[] args)
    {
        if (!args.Contains("--migrate") && !args.Contains("--provision-user")) return false;
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<AppDbContext>();
        if (args.Contains("--migrate")) await db.Database.MigrateAsync();
        if (args.Contains("--seed-demo")) await DbSeeder.SeedAsync(db);
        await EnsureRolesAsync(services);
        var index = Array.IndexOf(args, "--provision-user");
        if (index >= 0)
        {
            if (index + 1 >= args.Length) throw new InvalidOperationException("Thiếu email tài khoản.");
            var email = args[index + 1].Trim();
            var roleIndex = Array.IndexOf(args, "--role");
            var role = roleIndex >= 0 && roleIndex + 1 < args.Length ? args[roleIndex + 1] : AppRoles.User;
            if (role is not (AppRoles.User or AppRoles.Admin)) throw new InvalidOperationException("Vai trò chỉ có User hoặc Admin.");
            var users = services.GetRequiredService<UserManager<ApplicationUser>>();
            if (await users.FindByEmailAsync(email) is not null)
                throw new InvalidOperationException("Tài khoản đã tồn tại; hãy quản lý bằng giao diện thay vì ghi đè qua CLI.");
            var password = Environment.GetEnvironmentVariable("TRACNGHIEM_PROVISION_PASSWORD");
            if (string.IsNullOrEmpty(password)) throw new InvalidOperationException("Cần biến môi trường riêng TRACNGHIEM_PROVISION_PASSWORD để tạo tài khoản.");
            var nameIndex = Array.IndexOf(args, "--name");
            var name = nameIndex >= 0 && nameIndex + 1 < args.Length ? args[nameIndex + 1].Trim() : role == AppRoles.Admin ? "Quản trị viên" : "Người học";
            if (name.Length is < 1 or > 80) throw new InvalidOperationException("Tên cần từ 1 đến 80 ký tự.");
            await using var transaction = await db.Database.BeginTransactionAsync();
            var user = new ApplicationUser { UserName = email, Email = email, DisplayName = name };
            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
            result = await users.AddToRoleAsync(user, role);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
            await transaction.CommitAsync();
            Console.WriteLine($"Đã tạo tài khoản {email}, vai trò {role}. Mật khẩu không được ghi ra log.");
        }
        return true;
    }
}
