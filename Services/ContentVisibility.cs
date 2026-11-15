using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Services;

/// <summary>
/// Single place that answers "which content may this account read?".
/// Shared library content has no owner; everything else is private to the account
/// that created it. The Admin role only adds account management, never content access.
/// </summary>
public static class ContentVisibility
{
    public static IQueryable<Subject> VisibleSubjects(this AppDbContext db, string? userId) =>
        db.Subjects.Where(s => s.OwnerId == null || s.OwnerId == userId);

    public static IQueryable<SubjectFolder> VisibleFolders(this AppDbContext db, string? userId) =>
        db.SubjectFolders.Where(f => f.OwnerId == null || f.OwnerId == userId);
}
