using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
namespace TracNghiemWeb.Services;
public class LearnerService(AppDbContext db)
{
    private string UserId => db.CurrentUserId ?? throw new UnauthorizedAccessException();
    public async Task<LearnerProfile> ProfileAsync() => await db.LearnerProfiles.FirstOrDefaultAsync() ?? new LearnerProfile { UserId = UserId };
    public async Task SaveProfileAsync(string name, int goal)
    {
        if (name.Length > 100 || goal is < 1 or > 500) throw new ArgumentException();
        var profile = await db.LearnerProfiles.FirstOrDefaultAsync();
        if (profile is null) { profile = new LearnerProfile { UserId = UserId }; db.LearnerProfiles.Add(profile); }
        profile.DisplayName = name.Trim(); profile.DailyGoal = goal;
        await db.SaveChangesAsync();
    }
    public Task<List<SavedQuestion>> SavedAsync(int? subjectId, string? search) => db.SavedQuestions.AsNoTracking()
        .Include(s => s.Question).ThenInclude(q => q!.Subject)
        .Include(s => s.Question).ThenInclude(q => q!.Passage)
        .Where(s => (subjectId == null || s.Question!.SubjectId == subjectId)
            && (search == null || s.Question!.Content.Contains(search)))
        .OrderByDescending(s => s.Id).Take(200).ToListAsync();
    public async Task<bool> SaveQuestionAsync(int questionId, bool favorite, string? note)
    {
        if (note?.Length > 2000 || !await db.Questions.AnyAsync(q => q.Id == questionId)) return false;
        var saved = await db.SavedQuestions.SingleOrDefaultAsync(s => s.QuestionId == questionId);
        if (saved is null) { saved = new SavedQuestion { UserId = UserId, QuestionId = questionId }; db.SavedQuestions.Add(saved); }
        saved.Favorite = favorite; saved.Note = note?.Trim();
        if (!favorite && string.IsNullOrEmpty(saved.Note)) db.SavedQuestions.Remove(saved);
        await db.SaveChangesAsync(); return true;
    }
    public Task<List<Attempt>> PendingAsync() => db.Attempts.AsNoTracking().Include(a => a.Subject)
        .Where(a => a.SubmittedAt == null).OrderByDescending(a => a.StartedAt).Take(100).ToListAsync();
    public Task<int> TodayAsync()
    {
        // A fixed Vietnam day boundary, independent of the server's local time zone.
        var start = DateTime.UtcNow.AddHours(7).Date.AddHours(-7).ToLocalTime();
        var end = start.AddDays(1);
        return db.AttemptAnswers.Where(a => a.Attempt!.SubmittedAt >= start && a.Attempt.SubmittedAt < end
            && a.SelectedOption != null).Select(a => a.QuestionId).Distinct().CountAsync();
    }
    public async Task<bool> SaveDraftAsync(int attemptId, IReadOnlyDictionary<int,string?> answers)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var attempt = await db.Attempts.Include(a => a.Answers).SingleOrDefaultAsync(a => a.Id == attemptId);
        if (attempt is null || attempt.IsSubmitted || (attempt.TimeLimitMinutes is int limit && DateTime.Now >= attempt.StartedAt.AddMinutes(limit))) return false;
        foreach (var answer in attempt.Answers)
            if (answers.TryGetValue(answer.QuestionId, out var selected) && selected is "A" or "B" or "C" or "D") answer.SelectedOption = selected;
        await db.SaveChangesAsync(); await transaction.CommitAsync(); return true;
    }
}
