using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Services;

/// <summary>
/// Single place that answers "which content may this account read?".
/// Shared library content has no owner; content created by an administrator is
/// teaching material visible to everyone; content created by a learner stays private.
/// </summary>
public static class ContentVisibility
{
    public static IQueryable<Subject> VisibleSubjects(this AppDbContext db, string? userId)
    {
        var adminUserIds = AdminUserIds(db);
        return db.Subjects.Where(s =>
            s.OwnerId == null || s.OwnerId == userId || adminUserIds.Contains(s.OwnerId!));
    }

    public static IQueryable<SubjectFolder> VisibleFolders(this AppDbContext db, string? userId)
    {
        var adminUserIds = AdminUserIds(db);
        return db.SubjectFolders.Where(f =>
            f.OwnerId == null || f.OwnerId == userId || adminUserIds.Contains(f.OwnerId!));
    }

    private static IQueryable<string> AdminUserIds(AppDbContext db)
    {
        var adminRoleIds = db.Roles
            .Where(r => r.Name == CurrentUser.AdminRole)
            .Select(r => r.Id);

        return db.UserRoles
            .Where(ur => adminRoleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId);
    }
}
