using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services.Documents;

namespace TracNghiemWeb.Services;

/// <summary>
/// Account administration used by the admin-only account pages.
/// Everything here is server-side: the pages themselves require the Admin role.
/// The Admin role has no access to other users' private documents; deleting an
/// account removes its document files as part of the account cleanup.
/// </summary>
public class AccountAdminService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly DocumentStorage _storage;

    public AccountAdminService(AppDbContext db, UserManager<ApplicationUser> users, DocumentStorage storage)
    {
        _db = db;
        _users = users;
        _storage = storage;
    }

    public async Task<List<AccountRow>> GetAccountsAsync(string? search = null)
    {
        var admins = await _db.UserRoles
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .Where(x => x.Name == CurrentUser.AdminRole)
            .Select(x => x.UserId)
            .ToListAsync();
        var adminIds = admins.ToHashSet();

        var folderCounts = await _db.SubjectFolders
            .AsNoTracking()
            .Where(f => f.OwnerId != null)
            .GroupBy(f => f.OwnerId!)
            .Select(g => new { OwnerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OwnerId, x => x.Count);

        var subjectCounts = await _db.Subjects
            .AsNoTracking()
            .Where(s => s.OwnerId != null)
            .GroupBy(s => s.OwnerId!)
            .Select(g => new { OwnerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OwnerId, x => x.Count);

        var attemptCounts = await _db.Attempts
            .AsNoTracking()
            .Where(a => a.OwnerId != null)
            .GroupBy(a => a.OwnerId!)
            .Select(g => new { OwnerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OwnerId, x => x.Count);

        var documentCounts = await _db.Documents
            .AsNoTracking()
            .GroupBy(d => d.OwnerId)
            .Select(g => new { OwnerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OwnerId, x => x.Count);

        var query = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.Email!.Contains(term) || u.UserName!.Contains(term)
                || (u.DisplayName != null && u.DisplayName.Contains(term)));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .ThenBy(u => u.Email)
            .ToListAsync();

        return users
            .Select(u => new AccountRow(
                u.Id,
                u.NameOrEmail,
                u.Email ?? u.UserName ?? string.Empty,
                u.CreatedAt,
                adminIds.Contains(u.Id),
                u.LockoutEnd is DateTimeOffset end && end > DateTimeOffset.UtcNow,
                folderCounts.GetValueOrDefault(u.Id),
                subjectCounts.GetValueOrDefault(u.Id),
                attemptCounts.GetValueOrDefault(u.Id),
                documentCounts.GetValueOrDefault(u.Id)))
            .ToList();
    }

    public async Task<AccountActionResult> CreateAsync(string email, string? displayName, string password, bool isAdmin)
    {
        email = email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
        };

        var created = await _users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return AccountActionResult.Fail(created.Errors.Select(e => e.Description));
        }

        if (!isAdmin)
        {
            return AccountActionResult.Ok($"Đã tạo tài khoản {email}.");
        }

        var grant = await _users.AddToRoleAsync(user, CurrentUser.AdminRole);
        if (!grant.Succeeded)
        {
            return AccountActionResult.Fail(grant.Errors.Select(e => e.Description));
        }

        return AccountActionResult.Ok($"Đã tạo tài khoản quản trị {email}.");
    }

    public async Task<AccountActionResult> ResetPasswordAsync(string userId, string password)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
        {
            return AccountActionResult.Fail("Tài khoản không tồn tại.");
        }

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, password);
        return result.Succeeded
            ? AccountActionResult.Ok("Đã đặt lại mật khẩu.")
            : AccountActionResult.Fail(result.Errors.Select(e => e.Description));
    }

    public async Task<AccountActionResult> SetAdminAsync(string userId, bool isAdmin)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
        {
            return AccountActionResult.Fail("Tài khoản không tồn tại.");
        }

        if (isAdmin)
        {
            var grant = await _users.AddToRoleAsync(user, CurrentUser.AdminRole);
            return grant.Succeeded || await _users.IsInRoleAsync(user, CurrentUser.AdminRole)
                ? AccountActionResult.Ok("Đã cấp quyền quản trị.")
                : AccountActionResult.Fail(grant.Errors.Select(e => e.Description));
        }

        if (await CountAdminsAsync() <= 1 && await _users.IsInRoleAsync(user, CurrentUser.AdminRole))
        {
            return AccountActionResult.Fail("Không thể thu hồi quyền của quản trị viên duy nhất.");
        }

        var remove = await _users.RemoveFromRoleAsync(user, CurrentUser.AdminRole);
        return remove.Succeeded
            ? AccountActionResult.Ok("Đã thu hồi quyền quản trị.")
            : AccountActionResult.Fail(remove.Errors.Select(e => e.Description));
    }

    public async Task<AccountActionResult> SetLockedAsync(string userId, bool locked, string currentUserId)
    {
        if (!locked)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null)
            {
                return AccountActionResult.Fail("Tài khoản không tồn tại.");
            }

            var unlock = await _users.SetLockoutEndDateAsync(user, null);
            await _users.ResetAccessFailedCountAsync(user);
            return unlock.Succeeded
                ? AccountActionResult.Ok("Đã mở khóa tài khoản.")
                : AccountActionResult.Fail(unlock.Errors.Select(e => e.Description));
        }

        if (userId == currentUserId)
        {
            return AccountActionResult.Fail("Bạn không thể tự khóa tài khoản đang đăng nhập.");
        }

        var target = await _users.FindByIdAsync(userId);
        if (target is null)
        {
            return AccountActionResult.Fail("Tài khoản không tồn tại.");
        }

        if (await _users.IsInRoleAsync(target, CurrentUser.AdminRole) && await CountAdminsAsync() <= 1)
        {
            return AccountActionResult.Fail("Không thể khóa quản trị viên duy nhất.");
        }

        await _users.SetLockoutEnabledAsync(target, true);
        var lockResult = await _users.SetLockoutEndDateAsync(target, DateTimeOffset.UtcNow.AddYears(100));
        return lockResult.Succeeded
            ? AccountActionResult.Ok("Đã khóa tài khoản.")
            : AccountActionResult.Fail(lockResult.Errors.Select(e => e.Description));
    }

    public async Task<AccountActionResult> DeleteAsync(string userId, string currentUserId)
    {
        if (userId == currentUserId)
        {
            return AccountActionResult.Fail("Bạn không thể xóa tài khoản đang đăng nhập.");
        }

        var user = await _users.FindByIdAsync(userId);
        if (user is null)
        {
            return AccountActionResult.Fail("Tài khoản không tồn tại.");
        }

        if (await _users.IsInRoleAsync(user, CurrentUser.AdminRole) && await CountAdminsAsync() <= 1)
        {
            return AccountActionResult.Fail("Không thể xóa quản trị viên duy nhất.");
        }

        // Detach content so classroom data survives the account; attempts and private
        // documents cascade with the user row. Document files are removed after the
        // account row is actually deleted.
        var documentKeys = await _db.Documents
            .AsNoTracking()
            .Where(d => d.OwnerId == userId)
            .Select(d => d.StorageKey)
            .ToListAsync();

        await _db.SubjectFolders.Where(f => f.OwnerId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.OwnerId, (string?)null));
        await _db.Subjects.Where(s => s.OwnerId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(s => s.OwnerId, (string?)null));

        var deleted = await _users.DeleteAsync(user);
        if (!deleted.Succeeded)
        {
            return AccountActionResult.Fail(deleted.Errors.Select(e => e.Description));
        }

        foreach (var key in documentKeys)
        {
            _storage.Delete(key);
        }

        return AccountActionResult.Ok("Đã xóa tài khoản. Bài làm và tài liệu riêng bị xóa theo tài khoản; thư mục và môn học được giữ lại làm nội dung dùng chung.");
    }

    private Task<int> CountAdminsAsync() =>
        _db.UserRoles
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .CountAsync(name => name == CurrentUser.AdminRole);
}

public record AccountRow(
    string Id,
    string DisplayName,
    string Email,
    DateTime CreatedAt,
    bool IsAdmin,
    bool IsLocked,
    int FolderCount,
    int SubjectCount,
    int AttemptCount,
    int DocumentCount);

public record AccountActionResult(bool Success, string? Message)
{
    public static AccountActionResult Ok(string message) => new(true, message);
    public static AccountActionResult Fail(string message) => new(false, message);
    public static AccountActionResult Fail(IEnumerable<string> errors) =>
        new(false, string.Join(" ", errors));
}
