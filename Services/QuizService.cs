using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Services;

public class QuizService
{
    public const int MaxQuestionsPerAttempt = 200;
    public const int MaxTimeLimitMinutes = 240;
    public const int SubmissionGraceSeconds = 5;

    private static readonly string[] OptionKeys = { "A", "B", "C", "D" };

    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public QuizService(AppDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    private DateTime Now => _time.GetLocalNow().DateTime;

    public TimeSpan? GetRemainingTime(Attempt attempt) => attempt.Deadline is DateTime deadline ? deadline - Now : null;

    public Task<List<PendingAttemptSummary>> GetPendingAttemptsAsync(int take = 5) =>
        _db.Attempts.AsNoTracking()
            .Where(a => a.SubmittedAt == null)
            .OrderByDescending(a => a.StartedAt).ThenByDescending(a => a.Id)
            .Take(Math.Clamp(take, 1, 20))
            .Select(a => new PendingAttemptSummary(a.Id, a.Subject!.Name, a.TotalQuestions,
                a.Answers.Count(answer => answer.SelectedOption != null), a.StartedAt, a.DraftSavedAt, a.TimeLimitMinutes))
            .ToListAsync();

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
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.FolderId,
                FolderName = s.Folder != null ? s.Folder.Name : null,
                QuestionCount = s.Questions.Count,
            })
            .ToListAsync();

        return subjects
            .Select(s => new SubjectSummary(
                s.Id,
                s.Name,
                s.QuestionCount,
                wrongBySubject.TryGetValue(s.Id, out var count) ? count : 0)
            {
                FolderId = s.FolderId,
                FolderName = s.FolderName,
            })
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

        // Shuffle whole groups, then prefer an exact-size combination. Reading questions
        // must never be split just to reach the requested count.
        var selections = new List<int>?[MaxQuestionsPerAttempt + 1];
        selections[0] = new List<int>();
        foreach (var group in groups)
        {
            for (var size = MaxQuestionsPerAttempt; size >= group.Count; size--)
                if (selections[size] is null && selections[size - group.Count] is { } previous)
                    selections[size] = previous.Concat(group).ToList();
            if (selections[count] is not null) break;
        }
        var selectedSize = Enumerable.Range(count, MaxQuestionsPerAttempt - count + 1)
            .FirstOrDefault(size => selections[size] is not null);
        if (selectedSize == 0)
            selectedSize = Enumerable.Range(1, count).Reverse().FirstOrDefault(size => selections[size] is not null);
        if (selectedSize == 0)
        {
            return StartResult.Fail("Không có nhóm câu hỏi phù hợp. Một bài đọc không được vượt quá 200 câu; vui lòng tách bài đọc trước khi luyện tập.");
        }

        return await CreateAttemptAsync(subjectId, selections[selectedSize]!, timeLimitMinutes);
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

        questionIds = questionIds.OrderBy(_ => Guid.NewGuid()).Take(MaxQuestionsPerAttempt).ToList();
        return await CreateAttemptAsync(subjectId, questionIds, timeLimitMinutes);
    }

    public async Task<DraftSaveResult> SaveDraftAsync(int attemptId, IReadOnlyDictionary<int, string?> answers)
    {
        var now = Now;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        // The conditional update holds the attempt's write lock until answers commit.
        // Submission claims the same row, so an in-flight draft cannot rewrite a grade.
        var claimed = await _db.Attempts.Where(a => a.Id == attemptId && a.SubmittedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.DraftSavedAt, (DateTime?)now));
        if (claimed == 0)
            return await _db.Attempts.AnyAsync(a => a.Id == attemptId)
                ? new(false, 0, null, "Bài đã được nộp.", true)
                : new(false, 0, null, "Bài làm không tồn tại.");
        // Recheck after obtaining the write lock; a queued request may cross the deadline.
        var attempt = await _db.Attempts.AsNoTracking().SingleAsync(a => a.Id == attemptId);
        now = Now;
        if (attempt.Deadline is DateTime deadline && now >= deadline)
            return new(false, 0, null, "Bài đã hết giờ. Vui lòng xem kết quả.", true);
        var rows = await _db.AttemptAnswers.Where(a => a.AttemptId == attemptId).ToListAsync();
        foreach (var row in rows)
        {
            row.SelectedOption = NormalizeOption(answers.GetValueOrDefault(row.QuestionId));
        }
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new(true, rows.Count(a => a.SelectedOption != null), now, null);
    }

    public async Task<SubmitResult> SubmitAsync(int attemptId, IReadOnlyDictionary<int, string?> answers, bool savedOnly = false)
    {
        var now = Now;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        // Claim once, inside the same transaction as grading. Concurrent POSTs cannot both grade.
        var claimed = await _db.Attempts.Where(a => a.Id == attemptId && a.SubmittedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.SubmittedAt, (DateTime?)now));
        if (claimed == 0)
        {
            return await _db.Attempts.AnyAsync(a => a.Id == attemptId)
                ? SubmitResult.Ok(attemptId) : SubmitResult.Fail("Bài làm không tồn tại.");
        }
        var attempt = await _db.Attempts.AsNoTracking().SingleAsync(a => a.Id == attemptId);
        now = Now;
        await _db.Attempts.Where(a => a.Id == attemptId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.SubmittedAt, (DateTime?)now));
        var useSaved = savedOnly || attempt.Deadline is DateTime deadline && now > deadline.AddSeconds(SubmissionGraceSeconds);
        var rows = await _db.AttemptAnswers.Where(a => a.AttemptId == attemptId).Include(a => a.Question).ToListAsync();
        foreach (var answer in rows)
        {
            var selected = useSaved ? answer.SelectedOption : answers.GetValueOrDefault(answer.QuestionId);
            answer.SelectedOption = NormalizeOption(selected);
            answer.IsCorrect = answer.SelectedOption is not null
                && answer.Question is not null
                && answer.SelectedOption == answer.Question.CorrectOption;
        }

        var correct = rows.Count(a => a.IsCorrect);
        await _db.SaveChangesAsync();
        await _db.Attempts.Where(a => a.Id == attemptId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.CorrectCount, correct));
        await transaction.CommitAsync();
        return SubmitResult.Ok(attemptId, useSaved);
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
            .OrderByDescending(a => a.SubmittedAt).ThenByDescending(a => a.Id)
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

    private IQueryable<AttemptAnswer> LatestAnswers() =>
        _db.AttemptAnswers.Where(a =>
            a.Attempt!.SubmittedAt != null
            && !_db.AttemptAnswers.Any(other => other.QuestionId == a.QuestionId
                && other.Attempt!.SubmittedAt != null
                && (other.Attempt.SubmittedAt.Value > a.Attempt.SubmittedAt.Value
                    || other.Attempt.SubmittedAt.Value == a.Attempt.SubmittedAt.Value && other.Id > a.Id)));

    private async Task<StartResult> CreateAttemptAsync(int subjectId, List<int> questionIds, int? timeLimitMinutes)
    {
        var attempt = new Attempt
        {
            SubjectId = subjectId,
            StartedAt = Now,
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

public record SubjectSummary(int Id, string Name, int QuestionCount, int WrongCount)
{
    public int? FolderId { get; init; }
    public string? FolderName { get; init; }
}

public record FolderSummary(int Id, string Name, int SubjectCount, int QuestionCount, int WrongCount);

public record StartResult(bool Success, int? AttemptId, string? Error)
{
    public static StartResult Ok(int attemptId) => new(true, attemptId, null);
    public static StartResult Fail(string error) => new(false, null, error);
}

public record DraftSaveResult(bool Success, int SavedCount, DateTime? SavedAt, string? Error, bool Closed = false);

public record PendingAttemptSummary(int Id, string SubjectName, int TotalQuestions, int AnsweredCount,
    DateTime StartedAt, DateTime? DraftSavedAt, int? TimeLimitMinutes);

public record SubmitResult(bool Success, int? AttemptId, string? Error, bool UsedSavedAnswers = false)
{
    public static SubmitResult Ok(int attemptId, bool usedSavedAnswers = false) => new(true, attemptId, null, usedSavedAnswers);
    public static SubmitResult Fail(string error) => new(false, null, error);
}
