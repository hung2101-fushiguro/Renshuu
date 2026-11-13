using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Services;

public class QuizService
{
    public const int MaxQuestionsPerAttempt = 200;
    public const int MaxTimeLimitMinutes = 240;

    private static readonly string[] OptionKeys = { "A", "B", "C", "D" };

    private readonly AppDbContext _db;

    public QuizService(AppDbContext db) => _db = db;

    public async Task<List<SubjectSummary>> GetSubjectSummariesAsync()
    {
        var wrongBySubject = await LatestAnswers()
            .Where(a => !a.IsCorrect)
            .Join(_db.Questions, a => a.QuestionId, q => q.Id, (a, q) => q.SubjectId)
            .GroupBy(subjectId => subjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SubjectId, x => x.Count);

        var subjects = await _db.Subjects
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, QuestionCount = s.Questions.Count })
            .ToListAsync();

        return subjects
            .Select(s => new SubjectSummary(
                s.Id,
                s.Name,
                s.QuestionCount,
                wrongBySubject.TryGetValue(s.Id, out var count) ? count : 0))
            .ToList();
    }

    public async Task<StartResult> StartAsync(int subjectId, int questionCount, int? timeLimitMinutes, bool isPractice = false)
    {
        var error = ValidateStartRequest(questionCount, timeLimitMinutes);
        if (error is not null)
        {
            return StartResult.Fail(error);
        }

        var subject = await _db.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == subjectId);
        if (subject is null)
        {
            return StartResult.Fail("Môn học không tồn tại.");
        }

        var rows = await _db.Questions
            .Where(q => q.SubjectId == subjectId)
            .Select(q => new { q.Id, q.PassageId })
            .ToListAsync();
        if (rows.Count == 0)
        {
            return StartResult.Fail($"Môn \"{subject.Name}\" chưa có câu hỏi nào.");
        }

        var count = Math.Min(questionCount, rows.Count);
        var groups = rows
            .GroupBy(q => q.PassageId.HasValue ? $"P{q.PassageId}" : $"Q{q.Id}")
            .Select(g => g.OrderBy(q => q.Id).Select(q => q.Id).ToList())
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        var questionIds = new List<int>();
        foreach (var group in groups)
        {
            if (questionIds.Count >= count)
            {
                break;
            }

            if (questionIds.Count > 0 && questionIds.Count + group.Count > MaxQuestionsPerAttempt)
            {
                continue;
            }

            questionIds.AddRange(group);
        }

        if (questionIds.Count > MaxQuestionsPerAttempt)
        {
            questionIds = questionIds.Take(MaxQuestionsPerAttempt).ToList();
        }

        return await CreateAttemptAsync(subjectId, questionIds, timeLimitMinutes, isPractice);
    }

    public async Task<StartResult> StartWrongAsync(int subjectId, int? timeLimitMinutes)
    {
        if (timeLimitMinutes is < 1 or > MaxTimeLimitMinutes)
        {
            return StartResult.Fail($"Thời gian làm bài phải từ 1 đến {MaxTimeLimitMinutes} phút.");
        }

        var subject = await _db.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == subjectId);
        if (subject is null)
        {
            return StartResult.Fail("Môn học không tồn tại.");
        }

        var questionIds = await LatestAnswers()
            .Where(a => !a.IsCorrect && a.Question!.SubjectId == subjectId)
            .OrderBy(a => a.QuestionId)
            .Select(a => a.QuestionId)
            .ToListAsync();

        if (questionIds.Count == 0)
        {
            return StartResult.Fail($"Bạn không còn câu sai nào ở môn \"{subject.Name}\".");
        }

        questionIds = questionIds.OrderBy(_ => Guid.NewGuid()).ToList();
        return await CreateAttemptAsync(subjectId, questionIds, timeLimitMinutes, isPractice: true);
    }

    public async Task<SubmitResult> SubmitAsync(int attemptId, IReadOnlyDictionary<int, string?> answers)
    {
        var attempt = await _db.Attempts
            .Include(a => a.Answers)
            .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt is null)
        {
            return SubmitResult.Fail("Bài làm không tồn tại.");
        }

        if (attempt.IsSubmitted)
        {
            return SubmitResult.Ok(attempt.Id);
        }

        foreach (var answer in attempt.Answers)
        {
            var selected = answers.TryGetValue(answer.QuestionId, out var value) ? value : null;
            answer.SelectedOption = NormalizeOption(selected);
            answer.IsCorrect = answer.SelectedOption is not null
                && answer.Question is not null
                && answer.SelectedOption == answer.Question.CorrectOption;
        }

        attempt.CorrectCount = attempt.Answers.Count(a => a.IsCorrect);
        attempt.SubmittedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return SubmitResult.Ok(attempt.Id);
    }

    public Task<Attempt?> GetAttemptForResultAsync(int attemptId) =>
        _db.Attempts
            .AsNoTracking()
            .Include(a => a.Subject)
            .Include(a => a.Answers.OrderBy(ans => ans.Id))
                .ThenInclude(ans => ans.Question)
                .ThenInclude(q => q!.Passage)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

    public async Task<List<Attempt>> GetHistoryAsync(int take = 100) =>
        await _db.Attempts
            .AsNoTracking()
            .Include(a => a.Subject)
            .Where(a => a.SubmittedAt != null)
            .OrderByDescending(a => a.StartedAt)
            .Take(take)
            .ToListAsync();

    public async Task<List<Question>> GetWrongQuestionsAsync(int subjectId) =>
        await _db.Questions
            .Where(q => q.SubjectId == subjectId
                && LatestAnswers().Any(a => a.QuestionId == q.Id && !a.IsCorrect))
            .Include(q => q.Passage)
            .AsNoTracking()
            .OrderBy(q => q.Id)
            .ToListAsync();

    public async Task<bool> DeleteAttemptAsync(int attemptId)
    {
        var attempt = await _db.Attempts
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.SubmittedAt != null);

        if (attempt is null)
        {
            return false;
        }

        _db.Attempts.Remove(attempt);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> ClearHistoryAsync()
    {
        var attempts = await _db.Attempts
            .Where(a => a.SubmittedAt != null)
            .ToListAsync();

        _db.Attempts.RemoveRange(attempts);
        await _db.SaveChangesAsync();
        return attempts.Count;
    }

    public async Task<AnalyticsData> GetAnalyticsAsync()
    {
        var attempts = await _db.Attempts
            .AsNoTracking()
            .Include(a => a.Subject)
            .Where(a => a.SubmittedAt != null)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        var totalAttempts = attempts.Count;
        var totalQuestionsAnswered = attempts.Sum(a => a.TotalQuestions);
        var totalCorrect = attempts.Sum(a => a.CorrectCount);
        var overallAccuracy = totalQuestionsAnswered > 0
            ? Math.Round((double)totalCorrect / totalQuestionsAnswered * 100, 1)
            : 0;

        var totalTimeSpent = TimeSpan.FromTicks(attempts.Sum(a => a.Duration.Ticks));

        var everWrongQuestionIds = await _db.AttemptAnswers
            .Where(a => a.Attempt!.SubmittedAt != null && !a.IsCorrect)
            .Select(a => a.QuestionId)
            .Distinct()
            .ToListAsync();

        var recoveredCount = 0;
        if (everWrongQuestionIds.Count > 0)
        {
            recoveredCount = await LatestAnswers()
                .Where(a => a.IsCorrect && everWrongQuestionIds.Contains(a.QuestionId))
                .Select(a => a.QuestionId)
                .Distinct()
                .CountAsync();
        }

        var recentTrends = attempts
            .Take(15)
            .OrderBy(a => a.StartedAt)
            .Select(a => new RecentAttemptTrend(
                a.Id,
                a.Subject?.Name ?? "Không rõ",
                a.SubmittedAt ?? a.StartedAt,
                a.CorrectCount,
                a.TotalQuestions,
                a.Percentage,
                a.IsPractice))
            .ToList();

        var currentWrongBySubject = await LatestAnswers()
            .Where(a => !a.IsCorrect)
            .Join(_db.Questions, a => a.QuestionId, q => q.Id, (a, q) => q.SubjectId)
            .GroupBy(subjectId => subjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SubjectId, x => x.Count);

        var subjects = await _db.Subjects
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();

        var proficiencies = new List<SubjectProficiency>();
        foreach (var sub in subjects)
        {
            var subAttempts = attempts.Where(a => a.SubjectId == sub.Id).ToList();
            var subCount = subAttempts.Count;
            var subAnswered = subAttempts.Sum(a => a.TotalQuestions);
            var subCorrect = subAttempts.Sum(a => a.CorrectCount);
            var subAccuracy = subAnswered > 0
                ? Math.Round((double)subCorrect / subAnswered * 100, 1)
                : 0;

            var wrongCount = currentWrongBySubject.TryGetValue(sub.Id, out var wc) ? wc : 0;

            string grade;
            if (subCount == 0) grade = "Chưa làm bài";
            else if (subAccuracy >= 80) grade = "Nắm vững";
            else if (subAccuracy >= 65) grade = "Khá";
            else if (subAccuracy >= 50) grade = "Trung bình";
            else grade = "Cần cải thiện";

            proficiencies.Add(new SubjectProficiency(
                sub.Id,
                sub.Name,
                subCount,
                subAnswered,
                subCorrect,
                subAccuracy,
                wrongCount,
                grade));
        }

        return new AnalyticsData(
            totalAttempts,
            totalQuestionsAnswered,
            totalTimeSpent,
            overallAccuracy,
            recoveredCount,
            everWrongQuestionIds.Count,
            recentTrends,
            proficiencies);
    }

    private IQueryable<AttemptAnswer> LatestAnswers() =>
        _db.AttemptAnswers.Where(a =>
            a.Attempt!.SubmittedAt != null
            && a.Id == _db.AttemptAnswers
                .Where(other => other.QuestionId == a.QuestionId && other.Attempt!.SubmittedAt != null)
                .Max(other => (int?)other.Id));

    private async Task<StartResult> CreateAttemptAsync(int subjectId, List<int> questionIds, int? timeLimitMinutes, bool isPractice)
    {
        var attempt = new Attempt
        {
            UserId = _db.CurrentUserId ?? throw new InvalidOperationException("Authentication required."),
            SubjectId = subjectId,
            StartedAt = DateTime.Now,
            TimeLimitMinutes = timeLimitMinutes,
            IsPractice = isPractice,
            TotalQuestions = questionIds.Count,
            Answers = questionIds.Select(id => new AttemptAnswer { QuestionId = id }).ToList(),
        };

        _db.Attempts.Add(attempt);
        await _db.SaveChangesAsync();
        return StartResult.Ok(attempt.Id);
    }

    private static string? ValidateStartRequest(int questionCount, int? timeLimitMinutes)
    {
        if (questionCount < 1)
        {
            return "Số câu hỏi phải lớn hơn 0.";
        }

        if (questionCount > MaxQuestionsPerAttempt)
        {
            return $"Số câu hỏi tối đa mỗi bài là {MaxQuestionsPerAttempt}.";
        }

        if (timeLimitMinutes is < 1 or > MaxTimeLimitMinutes)
        {
            return $"Thời gian làm bài phải từ 1 đến {MaxTimeLimitMinutes} phút (hoặc để trống nếu không giới hạn).";
        }

        return null;
    }

    private static string? NormalizeOption(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var option = value.Trim().ToUpperInvariant();
        return OptionKeys.Contains(option) ? option : null;
    }
}

public record SubjectSummary(int Id, string Name, int QuestionCount, int WrongCount);

public record StartResult(bool Success, int? AttemptId, string? Error)
{
    public static StartResult Ok(int attemptId) => new(true, attemptId, null);
    public static StartResult Fail(string error) => new(false, null, error);
}

public record SubmitResult(bool Success, int? AttemptId, string? Error)
{
    public static SubmitResult Ok(int attemptId) => new(true, attemptId, null);
    public static SubmitResult Fail(string error) => new(false, null, error);
}

public record AnalyticsData(
    int TotalAttempts,
    int TotalQuestionsAnswered,
    TimeSpan TotalTimeSpent,
    double OverallAccuracy,
    int RecoveredWrongQuestions,
    int TotalEverWrongQuestions,
    List<RecentAttemptTrend> RecentTrends,
    List<SubjectProficiency> SubjectProficiencies
)
{
    public double RecoveryRate => TotalEverWrongQuestions > 0
        ? Math.Round((double)RecoveredWrongQuestions / TotalEverWrongQuestions * 100, 1)
        : 100.0;
}

public record RecentAttemptTrend(
    int AttemptId,
    string SubjectName,
    DateTime SubmittedAt,
    int CorrectCount,
    int TotalQuestions,
    double Percentage,
    bool IsPractice
);

public record SubjectProficiency(
    int SubjectId,
    string SubjectName,
    int AttemptCount,
    int TotalAnswered,
    int TotalCorrect,
    double AveragePercentage,
    int CurrentWrongCount,
    string GradeLevel
);
