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

    public async Task<StartResult> StartAsync(int subjectId, int questionCount, int? timeLimitMinutes)
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

        var available = await _db.Questions.CountAsync(q => q.SubjectId == subjectId);
        if (available == 0)
        {
            return StartResult.Fail($"Môn \"{subject.Name}\" chưa có câu hỏi nào.");
        }

        var count = Math.Min(questionCount, available);
        var questionIds = await _db.Questions
            .Where(q => q.SubjectId == subjectId)
            .OrderBy(q => Guid.NewGuid())
            .Select(q => q.Id)
            .Take(count)
            .ToListAsync();

        return await CreateAttemptAsync(subjectId, questionIds, timeLimitMinutes);
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
        return await CreateAttemptAsync(subjectId, questionIds, timeLimitMinutes);
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
        await LatestAnswers()
            .Where(a => !a.IsCorrect && a.Question!.SubjectId == subjectId)
            .Select(a => a.Question!)
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

    private IQueryable<AttemptAnswer> LatestAnswers() =>
        _db.AttemptAnswers.Where(a =>
            a.Attempt!.SubmittedAt != null
            && a.Id == _db.AttemptAnswers
                .Where(other => other.QuestionId == a.QuestionId && other.Attempt!.SubmittedAt != null)
                .Max(other => (int?)other.Id));

    private async Task<StartResult> CreateAttemptAsync(int subjectId, List<int> questionIds, int? timeLimitMinutes)
    {
        var attempt = new Attempt
        {
            SubjectId = subjectId,
            StartedAt = DateTime.Now,
            TimeLimitMinutes = timeLimitMinutes,
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
