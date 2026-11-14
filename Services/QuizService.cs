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
    private readonly CurrentUser _currentUser;

    public QuizService(AppDbContext db, TimeProvider time, CurrentUser currentUser)
    {
        _db = db;
        _time = time;
        _currentUser = currentUser;
    }

    private DateTime Now => _time.GetLocalNow().DateTime;

    public TimeSpan? GetRemainingTime(Attempt attempt) => attempt.Deadline is DateTime deadline ? deadline - Now : null;

    public Task<List<PendingAttemptSummary>> GetPendingAttemptsAsync(int take = 5) =>
        _db.Attempts.AsNoTracking()
            .Where(a => a.OwnerId == _currentUser.UserId && a.SubmittedAt == null)
            .OrderByDescending(a => a.StartedAt).ThenByDescending(a => a.Id)
            .Take(Math.Clamp(take, 1, 20))
            .Select(a => new PendingAttemptSummary(a.Id, a.Subject!.Name, a.TotalQuestions,
                a.Answers.Count(answer => answer.SelectedOption != null), a.StartedAt, a.DraftSavedAt, a.TimeLimitMinutes))
            .ToListAsync();

    public async Task<PracticeSummary> GetPracticeSummaryAsync()
    {
        var summary = await _db.Attempts.AsNoTracking()
            .Where(a => a.OwnerId == _currentUser.UserId && a.SubmittedAt != null)
            .GroupBy(a => 1)
            .Select(g => new { Count = g.Count(), Correct = g.Sum(a => a.CorrectCount), Total = g.Sum(a => a.TotalQuestions) })
            .OrderBy(s => s.Count)
            .FirstOrDefaultAsync();

        return summary is null ? new(0, 0, 0) : new(summary.Count, summary.Correct, summary.Total);
    }

    /// <summary>
    /// Subjects the signed-in account may practise: everything it owns plus shared
    /// library content that has no owner (seed data and admin-managed content).
    /// </summary>
    public async Task<List<SubjectSummary>> GetSubjectSummariesAsync()
    {
        var userId = _currentUser.UserId;

        var wrongBySubject = await LatestAnswers()
            .Where(a => !a.IsCorrect)
            .Join(_db.Questions, a => a.QuestionId, q => q.Id, (a, q) => q.SubjectId)
            .GroupBy(subjectId => subjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SubjectId, x => x.Count);

        var subjects = await VisibleSubjects(includeAllWhenAdmin: false)
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

        var subject = await VisibleSubjects(includeAllWhenAdmin: false)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subjectId);
        if (subject is null)
        {
            return StartResult.Fail("Môn học không tồn tại hoặc bạn không có quyền truy cập.");
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

            if (questionIds.Count + group.Count > MaxQuestionsPerAttempt)
            {
                continue;
            }

            questionIds.AddRange(group);
        }

        if (questionIds.Count == 0)
        {
            return StartResult.Fail("Không có nhóm câu hỏi phù hợp. Một bài đọc không được vượt quá 200 câu; vui lòng tách bài đọc trước khi luyện tập.");
        }

        return await CreateAttemptAsync(subjectId, questionIds, timeLimitMinutes);
    }

    public async Task<StartResult> StartWrongAsync(int subjectId, int? timeLimitMinutes)
    {
        if (timeLimitMinutes is < 1 or > MaxTimeLimitMinutes)
        {
            return StartResult.Fail($"Thời gian làm bài phải từ 1 đến {MaxTimeLimitMinutes} phút.");
        }

        var subject = await VisibleSubjects(includeAllWhenAdmin: false)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subjectId);
        if (subject is null)
        {
            return StartResult.Fail("Môn học không tồn tại hoặc bạn không có quyền truy cập.");
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
        var attempt = await _db.Attempts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.OwnerId == _currentUser.UserId);
        if (attempt is null) return new(false, 0, null, "Bài làm không tồn tại.");
        if (attempt.IsSubmitted || attempt.Deadline is DateTime deadline && now >= deadline)
        {
            return new(false, 0, null, "Bài đã nộp hoặc hết giờ. Vui lòng xem kết quả.", true);
        }

        // The conditional update holds the attempt's write lock until answers commit.
        // Submission claims the same row, so an in-flight draft cannot rewrite a grade.
        var claimed = await _db.Attempts
            .Where(a => a.Id == attemptId && a.OwnerId == _currentUser.UserId && a.SubmittedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.DraftSavedAt, (DateTime?)now));
        if (claimed == 0) return new(false, 0, null, "Bài đã được nộp.", true);
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
        var claimed = await _db.Attempts
            .Where(a => a.Id == attemptId && a.OwnerId == _currentUser.UserId && a.SubmittedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.SubmittedAt, (DateTime?)now));
        if (claimed == 0)
        {
            return await _db.Attempts.AnyAsync(a => a.Id == attemptId && a.OwnerId == _currentUser.UserId)
                ? SubmitResult.Ok(attemptId) : SubmitResult.Fail("Bài làm không tồn tại.");
        }
        var attempt = await _db.Attempts.AsNoTracking().SingleAsync(a => a.Id == attemptId);
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
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.OwnerId == _currentUser.UserId);

    public async Task<List<Attempt>> GetHistoryAsync(int take = 100) =>
        await _db.Attempts
            .AsNoTracking()
            .Include(a => a.Subject)
            .Where(a => a.OwnerId == _currentUser.UserId && a.SubmittedAt != null)
            .OrderByDescending(a => a.SubmittedAt).ThenByDescending(a => a.Id)
            .Take(take)
            .ToListAsync();

    public async Task<List<Question>> GetWrongQuestionsAsync(int? subjectId = null)
    {
        var query = _db.Questions
            .Where(q => LatestAnswers().Any(a => a.QuestionId == q.Id && !a.IsCorrect));

        if (subjectId.HasValue && subjectId.Value > 0)
        {
            query = query.Where(q => q.SubjectId == subjectId.Value);
        }

        return await query
            .Include(q => q.Subject)
            .Include(q => q.Passage)
            .AsNoTracking()
            .OrderBy(q => q.SubjectId)
            .ThenBy(q => q.Id)
            .ToListAsync();
    }

    public async Task<bool> DeleteAttemptAsync(int attemptId)
    {
        var attempt = await _db.Attempts
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.OwnerId == _currentUser.UserId && a.SubmittedAt != null);

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
            .Where(a => a.OwnerId == _currentUser.UserId && a.SubmittedAt != null)
            .ToListAsync();

        _db.Attempts.RemoveRange(attempts);
        await _db.SaveChangesAsync();
        return attempts.Count;
    }

    public async Task<AnalyticsSummary> GetAnalyticsAsync(int? subjectId = null)
    {
        var submittedQuery = _db.Attempts
            .AsNoTracking()
            .Where(a => a.OwnerId == _currentUser.UserId && a.SubmittedAt != null);

        if (subjectId.HasValue && subjectId.Value > 0)
        {
            submittedQuery = submittedQuery.Where(a => a.SubjectId == subjectId.Value);
        }

        var attempts = await submittedQuery
            .OrderByDescending(a => a.SubmittedAt)
            .ThenByDescending(a => a.Id)
            .Include(a => a.Subject)
                .ThenInclude(s => s!.Folder)
            .ToListAsync();

        if (attempts.Count == 0)
        {
            return new AnalyticsSummary(
                TotalAttempts: 0,
                TotalQuestionsAnswered: 0,
                TotalStudyTime: TimeSpan.Zero,
                AverageAccuracy: 0,
                TotalEverWrong: 0,
                TotalRecoveredWrong: 0,
                RecoveryRate: 100.0,
                RecentAttempts: new List<RecentAttemptData>(),
                SubjectPerformances: new List<SubjectPerformance>(),
                Trend: new AttemptTrend("stable", 0, "Chưa có bài làm nào để phân tích tiến độ.")
            );
        }

        int totalAttempts = attempts.Count;
        int totalQuestions = attempts.Sum(a => a.TotalQuestions);
        int totalCorrect = attempts.Sum(a => a.CorrectCount);
        double totalSeconds = attempts.Sum(a => Math.Max(0, a.Duration.TotalSeconds));
        var totalStudyTime = TimeSpan.FromSeconds(totalSeconds);
        double averageAccuracy = totalQuestions > 0 ? (totalCorrect * 100.0 / totalQuestions) : 0;

        // Tỷ lệ khắc phục câu sai (Bao nhiêu câu từng sai nay đã làm lại đúng)
        var everWrongQuery = _db.AttemptAnswers
            .Where(a => a.Attempt!.OwnerId == _currentUser.UserId
                && a.Attempt.SubmittedAt != null && !a.IsCorrect);

        if (subjectId.HasValue && subjectId.Value > 0)
        {
            everWrongQuery = everWrongQuery.Where(a => a.Question!.SubjectId == subjectId.Value);
        }

        var everWrongQuestionIds = await everWrongQuery
            .Select(a => a.QuestionId)
            .Distinct()
            .ToListAsync();

        int totalEverWrong = everWrongQuestionIds.Count;
        int totalRecovered = 0;

        if (totalEverWrong > 0)
        {
            totalRecovered = await LatestAnswers()
                .Where(a => a.IsCorrect && everWrongQuestionIds.Contains(a.QuestionId))
                .CountAsync();
        }

        double recoveryRate = totalEverWrong > 0 ? (totalRecovered * 100.0 / totalEverWrong) : 100.0;

        // Biểu đồ tiến độ (tối đa 20 bài gần nhất theo thứ tự thời gian tăng dần từ trái sang phải)
        var recent = attempts
            .Take(20)
            .OrderBy(a => a.SubmittedAt)
            .ThenBy(a => a.Id)
            .Select(a => new RecentAttemptData(
                a.Id,
                a.SubjectId,
                a.Subject?.Name ?? "Môn #" + a.SubjectId,
                a.SubmittedAt!.Value,
                a.Percentage,
                a.CorrectCount,
                a.TotalQuestions,
                a.Duration
            ))
            .ToList();

        // Xu hướng (Trend)
        AttemptTrend trend;
        if (attempts.Count >= 2)
        {
            var countHalf = Math.Max(1, Math.Min(5, attempts.Count / 2));
            var recentHalf = attempts.Take(countHalf).ToList();
            var olderHalf = attempts.Skip(countHalf).Take(countHalf).ToList();

            double recentAvg = recentHalf.Average(a => a.Percentage);
            double olderAvg = olderHalf.Count > 0 ? olderHalf.Average(a => a.Percentage) : attempts[^1].Percentage;
            double diff = recentAvg - olderAvg;

            if (diff > 1.5)
            {
                trend = new AttemptTrend("up", Math.Round(diff, 1),
                    $"Điểm số các bài gần đây tăng trung bình {Math.Round(diff, 1):0.#}% so với trước đó. Phong độ đang đi lên!");
            }
            else if (diff < -1.5)
            {
                trend = new AttemptTrend("down", Math.Round(Math.Abs(diff), 1),
                    $"Điểm số các bài gần đây giảm trung bình {Math.Round(Math.Abs(diff), 1):0.#}% so với trước đó. Cần chú ý củng cố bài học.");
            }
            else
            {
                trend = new AttemptTrend("stable", Math.Round(Math.Abs(diff), 1),
                    "Điểm số duy trì ổn định qua các lần làm bài gần đây.");
            }
        }
        else
        {
            trend = new AttemptTrend("stable", 0, "Cần làm thêm ít nhất 2 bài để phân tích xu hướng tăng/giảm điểm.");
        }

        // Thống kê theo môn học
        var wrongBySubject = await LatestAnswers()
            .Where(a => !a.IsCorrect)
            .Join(_db.Questions, a => a.QuestionId, q => q.Id, (a, q) => q.SubjectId)
            .GroupBy(sId => sId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SubjectId, x => x.Count);

        var subjectPerformances = attempts
            .GroupBy(a => a.SubjectId)
            .Select(g =>
            {
                var subj = g.First().Subject;
                var attemptCount = g.Count();
                var questionsAnswered = g.Sum(a => a.TotalQuestions);
                var correctAnswers = g.Sum(a => a.CorrectCount);
                var accuracy = questionsAnswered > 0 ? (correctAnswers * 100.0 / questionsAnswered) : 0;
                var currentWrong = wrongBySubject.GetValueOrDefault(g.Key, 0);

                string level;
                string levelClass;
                if (accuracy >= 80.0)
                {
                    level = "Giỏi";
                    levelClass = "success";
                }
                else if (accuracy >= 65.0)
                {
                    level = "Khá";
                    levelClass = "primary";
                }
                else if (accuracy >= 50.0)
                {
                    level = "Trung bình";
                    levelClass = "accent";
                }
                else
                {
                    level = "Cần cải thiện";
                    levelClass = "danger";
                }

                return new SubjectPerformance(
                    SubjectId: g.Key,
                    SubjectName: subj?.Name ?? "Môn #" + g.Key,
                    FolderName: subj?.Folder?.Name,
                    AttemptCount: attemptCount,
                    QuestionsAnswered: questionsAnswered,
                    CorrectAnswers: correctAnswers,
                    Accuracy: accuracy,
                    CurrentWrongCount: currentWrong,
                    Level: level,
                    LevelClass: levelClass
                );
            })
            .OrderByDescending(s => s.Accuracy)
            .ToList();

        return new AnalyticsSummary(
            TotalAttempts: totalAttempts,
            TotalQuestionsAnswered: totalQuestions,
            TotalStudyTime: totalStudyTime,
            AverageAccuracy: averageAccuracy,
            TotalEverWrong: totalEverWrong,
            TotalRecoveredWrong: totalRecovered,
            RecoveryRate: recoveryRate,
            RecentAttempts: recent,
            SubjectPerformances: subjectPerformances,
            Trend: trend
        );
    }

    private IQueryable<Subject> VisibleSubjects(bool includeAllWhenAdmin)
    {
        if (includeAllWhenAdmin && _currentUser.IsAdmin)
        {
            return _db.Subjects.AsQueryable();
        }

        return _db.VisibleSubjects(_currentUser.UserId);
    }

    private IQueryable<AttemptAnswer> LatestAnswers()
    {
        var userId = _currentUser.UserId;

        return _db.AttemptAnswers.Where(a =>
            a.Attempt!.OwnerId == userId
            && a.Attempt.SubmittedAt != null
            && !_db.AttemptAnswers.Any(other => other.QuestionId == a.QuestionId
                && other.Attempt!.OwnerId == userId
                && other.Attempt.SubmittedAt != null
                && (other.Attempt.SubmittedAt.Value > a.Attempt.SubmittedAt.Value
                    || other.Attempt.SubmittedAt.Value == a.Attempt.SubmittedAt.Value && other.Id > a.Id)));
    }

    private async Task<StartResult> CreateAttemptAsync(int subjectId, List<int> questionIds, int? timeLimitMinutes)
    {
        if (_currentUser.UserId is not string ownerId)
        {
            return StartResult.Fail("Vui lòng đăng nhập để bắt đầu luyện tập.");
        }

        var attempt = new Attempt
        {
            SubjectId = subjectId,
            OwnerId = ownerId,
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

public record PracticeSummary(int AttemptCount, int CorrectCount, int TotalQuestions)
{
    public double Accuracy => TotalQuestions == 0 ? 0 : CorrectCount * 100.0 / TotalQuestions;
}

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

public record AnalyticsSummary(
    int TotalAttempts,
    int TotalQuestionsAnswered,
    TimeSpan TotalStudyTime,
    double AverageAccuracy,
    int TotalEverWrong,
    int TotalRecoveredWrong,
    double RecoveryRate,
    List<RecentAttemptData> RecentAttempts,
    List<SubjectPerformance> SubjectPerformances,
    AttemptTrend Trend
);

public record RecentAttemptData(
    int Id,
    int SubjectId,
    string SubjectName,
    DateTime SubmittedAt,
    double Percentage,
    int CorrectCount,
    int TotalQuestions,
    TimeSpan Duration
);

public record SubjectPerformance(
    int SubjectId,
    string SubjectName,
    string? FolderName,
    int AttemptCount,
    int QuestionsAnswered,
    int CorrectAnswers,
    double Accuracy,
    int CurrentWrongCount,
    string Level,
    string LevelClass
);

public record AttemptTrend(
    string Direction,
    double Difference,
    string Summary
);
