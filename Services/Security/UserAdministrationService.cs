using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Services.Security;

public record AccountEdit(string DisplayName, string Email, string Role, bool IsActive, string? Password, string? Version);
public record AccountResult(bool Success, string? Error = null)
{
    public static AccountResult From(IdentityResult result) => new(result.Succeeded, result.Succeeded ? null : string.Join(" ", result.Errors.Select(e => e.Description)));
}

public sealed class UserAdministrationService(AppDbContext db, UserManager<ApplicationUser> users, ICurrentUser current)
{
    private static string? Validate(AccountEdit input, bool creating)
    {
        if (string.IsNullOrWhiteSpace(input.DisplayName) || input.DisplayName.Trim().Length > 80) return "Tên cần từ 1 đến 80 ký tự.";
        if (input.Email.Length > 254 || !new EmailAddressAttribute().IsValid(input.Email.Trim())) return "Email không hợp lệ.";
        if (input.Role is not (AppRoles.Admin or AppRoles.User)) return "Vai trò không hợp lệ.";
        if (creating && string.IsNullOrEmpty(input.Password)) return "Vui lòng nhập mật khẩu.";
        if (input.Password?.Length > 128) return "Mật khẩu tối đa 128 ký tự.";
        return null;
    }

    private Task LockAsync() => db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r = sp_getapplock @Resource=N'TracNghiem.UserAdministration', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @r < 0 THROW 51000, N'User administration is busy', 1;");

    public async Task<AccountResult> CreateAsync(AccountEdit input)
    {
        if (!current.IsAdmin) return new(false, "Bạn không có quyền quản lý tài khoản.");
        if (Validate(input, true) is string error) return new(false, error);
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAsync();
        var user = new ApplicationUser { DisplayName = input.DisplayName.Trim(), UserName = input.Email.Trim(), Email = input.Email.Trim(), IsActive = input.IsActive };
        var result = await users.CreateAsync(user, input.Password!);
        if (!result.Succeeded) return AccountResult.From(result);
        result = await users.AddToRoleAsync(user, input.Role);
        if (!result.Succeeded) return AccountResult.From(result);
        await transaction.CommitAsync();
        return new(true);
    }

    private async Task<string?> CheckChangeAsync(ApplicationUser user, string role, bool active)
    {
        if (user.Id == current.Id && (role != AppRoles.Admin || !active)) return "Bạn không thể tự hạ quyền hoặc ngừng tài khoản đang sử dụng.";
        if (user.IsActive && await users.IsInRoleAsync(user, AppRoles.Admin) && (role != AppRoles.Admin || !active))
        {
            var count = await (from membership in db.UserRoles join account in db.Users on membership.UserId equals account.Id
                               join assigned in db.Roles on membership.RoleId equals assigned.Id
                               where assigned.Name == AppRoles.Admin && account.IsActive && !account.IsDeleted select account.Id).CountAsync();
            if (count <= 1) return "Cần giữ ít nhất một admin đang hoạt động.";
        }
        return null;
    }

    public async Task<AccountResult> UpdateAsync(string id, AccountEdit input)
    {
        if (!current.IsAdmin) return new(false, "Bạn không có quyền quản lý tài khoản.");
        if (Validate(input, false) is string invalid) return new(false, invalid);
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAsync();
        var user = await users.FindByIdAsync(id);
        if (user is null || user.IsDeleted) return new(false, "Tài khoản không tồn tại.");
        if (input.Version != user.ConcurrencyStamp) return new(false, "Tài khoản vừa được cập nhật. Vui lòng tải lại trang.");
        if (await CheckChangeAsync(user, input.Role, input.IsActive) is string error) return new(false, error);
        user.DisplayName = input.DisplayName.Trim();
        user.UserName = user.Email = input.Email.Trim();
        user.IsActive = input.IsActive;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded) return AccountResult.From(result);
        var roles = await users.GetRolesAsync(user);
        if (!roles.Contains(input.Role) || roles.Count != 1)
        {
            result = await users.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded) return AccountResult.From(result);
            result = await users.AddToRoleAsync(user, input.Role);
            if (!result.Succeeded) return AccountResult.From(result);
        }
        if (!string.IsNullOrEmpty(input.Password))
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            result = await users.ResetPasswordAsync(user, token, input.Password);
            if (!result.Succeeded) return AccountResult.From(result);
            result = await users.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded) return AccountResult.From(result);
            result = await users.ResetAccessFailedCountAsync(user);
            if (!result.Succeeded) return AccountResult.From(result);
        }
        result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded) return AccountResult.From(result);
        await transaction.CommitAsync();
        return new(true);
    }

    public async Task<AccountResult> DeleteAsync(string id, string? version)
    {
        if (!current.IsAdmin) return new(false, "Bạn không có quyền quản lý tài khoản.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAsync();
        var user = await users.FindByIdAsync(id);
        if (user is null || user.IsDeleted) return new(false, "Tài khoản không tồn tại.");
        if (version != user.ConcurrencyStamp) return new(false, "Tài khoản vừa được cập nhật. Vui lòng tải lại trang.");
        if (await CheckChangeAsync(user, AppRoles.User, false) is string error) return new(false, error);
        // Retain ownership and learning records. Deleted users cannot sign in or appear in the active directory.
        user.IsActive = false;
        user.IsDeleted = true;
        var result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded) return AccountResult.From(result);
        await transaction.CommitAsync();
        return new(true);
    }
}
