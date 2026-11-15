using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Services;

/// <summary>
/// Single place that answers "which content may this account read?".
/// A learner sees their own content plus content its owner explicitly published.
/// The Admin role only adds account management, never content access.
/// </summary>
public static class ContentVisibility
{
    public static IQueryable<Subject> VisibleSubjects(this AppDbContext db, string? userId) =>
        db.Subjects.Where(s => s.OwnerId == userId || s.IsPublic);

    public static IQueryable<SubjectFolder> VisibleFolders(this AppDbContext db, string? userId) =>
        db.SubjectFolders.Where(f => f.OwnerId == userId
            || f.Subjects.Any(s => s.OwnerId == userId || s.IsPublic));
}
