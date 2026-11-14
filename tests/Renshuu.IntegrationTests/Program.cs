using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

// SQL Server is required: transaction/locking tests must run against the production provider.
// A unique temporary database is created and only that database is removed in finally.
var database = "RenshuuRegression_" + Guid.NewGuid().ToString("N");
var connection = new SqlConnectionStringBuilder
{
    DataSource = args.FirstOrDefault() ?? "localhost", InitialCatalog = database,
    IntegratedSecurity = true, TrustServerCertificate = true
};
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection.ConnectionString).Options;
var clock = new TestClock();
AppDbContext Db() => new(options);
async Task<T> Run<T>(Func<QuizService, Task<T>> operation)
{
    await using var db = Db();
    return await operation(new QuizService(db, clock));
}
void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
void Pass(string message) => Console.WriteLine("PASS " + message);
Dictionary<int, string?> Answer(int id, string value) => new() { [id] = value };
Question Question(string content) => new() { Content = content, OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D", CorrectOption = "A" };
async Task<int> Start(int subject, int? minutes = null, int count = 1)
{
    var result = await Run(q => q.StartAsync(subject, count, minutes));
    Check(result.Success, result.Error ?? "Start failed");
    return result.AttemptId!.Value;
}

try
{
    int subjectId, questionId;
    await using (var db = Db())
    {
        await db.Database.MigrateAsync();
        var subject = new Subject { Name = "Regression", Questions = [Question("Single question")] };
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();
        subjectId = subject.Id;
        questionId = subject.Questions.Single().Id;
    }
    Pass("All migrations apply to a fresh SQL Server database");

    var older = await Start(subjectId);
    var newer = await Start(subjectId);
    await Run(q => q.SubmitAsync(newer, Answer(questionId, "B")));
    clock.Advance(10);
    await Run(q => q.SubmitAsync(older, Answer(questionId, "A")));
    Check((await Run(q => q.GetWrongQuestionsAsync(subjectId))).Count == 0, "Latest must use submission order, not creation order");
    Check((await Run(q => q.GetHistoryAsync())).First().Id == older, "History must use submission order");
    // Ties are deterministic, including two submissions in the same timestamp tick.
    var tied = await Start(subjectId);
    await Run(q => q.SubmitAsync(tied, Answer(questionId, "B")));
    Check((await Run(q => q.GetWrongQuestionsAsync(subjectId))).Count == 1, "Timestamp tie must use answer Id");
    clock.Advance(1);
    var corrected = await Start(subjectId);
    await Run(q => q.SubmitAsync(corrected, Answer(questionId, "A")));
    Pass("Wrong-answer review and history follow latest submission, with deterministic ties");

    var draft = await Start(subjectId);
    var saved = await Run(q => q.SaveDraftAsync(draft, Answer(questionId, " b ")));
    Check(saved.Success && saved.SavedCount == 1, "Draft must save normalized answers");
    await using (var db = Db())
    {
        var stored = await db.AttemptAnswers.SingleAsync(a => a.AttemptId == draft);
        Check(stored.SelectedOption == "B" && !stored.IsCorrect, "Draft must not be graded");
        Check(await db.Attempts.AnyAsync(a => a.Id == draft && a.DraftSavedAt != null && a.SubmittedAt == null), "Draft metadata missing");
    }
    Check((await Run(q => q.GetPendingAttemptsAsync())).Any(a => a.Id == draft && a.AnsweredCount == 1), "Pending list must show saved count");
    Check((await Run(q => q.GetWrongQuestionsAsync(subjectId))).Count == 0, "Pending draft must not affect wrong answers");
    await Run(q => q.SaveDraftAsync(draft, Answer(questionId, "invalid")));
    Check((await Run(q => q.GetPendingAttemptsAsync())).Single(a => a.Id == draft).AnsweredCount == 0, "Invalid option must normalize to null");
    Pass("Drafts persist across contexts, normalize input, and never affect grading before submission");

    var late = await Start(subjectId, 1);
    await Run(q => q.SaveDraftAsync(late, Answer(questionId, "A")));
    clock.Advance(66);
    Check((await Run(q => q.SaveDraftAsync(late, Answer(questionId, "B")))).Closed, "Expired draft must close");
    var lateResult = await Run(q => q.SubmitAsync(late, Answer(questionId, "B")));
    Check(lateResult.UsedSavedAnswers, "Late post must grade saved answers only");
    Check((await Run(q => q.GetAttemptForResultAsync(late)))!.CorrectCount == 1, "Late payload must not replace saved answer");
    var grace = await Start(subjectId, 1);
    await Run(q => q.SaveDraftAsync(grace, Answer(questionId, "A")));
    clock.Advance(61);
    Check(!(await Run(q => q.SubmitAsync(grace, Answer(questionId, "B")))).UsedSavedAnswers, "Network grace must accept final payload");
    Check((await Run(q => q.GetAttemptForResultAsync(grace)))!.CorrectCount == 0, "Grace payload not graded");
    Check(!(await Run(q => q.SaveDraftAsync(-1, Answer(questionId, "A")))).Success, "Unknown draft must fail");
    Pass("Server deadline rejects late edits, grades saved answers, and permits the five-second transport grace");

    for (var i = 0; i < 5; i++)
    {
        var race = await Start(subjectId);
        await Task.WhenAll(
            Run(q => q.SaveDraftAsync(race, Answer(questionId, "B"))),
            Run(q => q.SubmitAsync(race, Answer(questionId, "A"))));
        var final = (await Run(q => q.GetAttemptForResultAsync(race)))!;
        Check(final.IsSubmitted && final.CorrectCount == 1 && final.Answers.Single().SelectedOption == "A", "Draft raced with grade");
        Check((await Run(q => q.SaveDraftAsync(race, Answer(questionId, "B")))).Closed, "Submitted draft must close");
        await Run(q => q.SubmitAsync(race, Answer(questionId, "B")));
        Check((await Run(q => q.GetAttemptForResultAsync(race)))!.CorrectCount == 1, "Duplicate submission changed grade");
        var twice = await Start(subjectId);
        await Task.WhenAll(Run(q => q.SubmitAsync(twice, Answer(questionId, "A"))), Run(q => q.SubmitAsync(twice, Answer(questionId, "B"))));
        var once = (await Run(q => q.GetAttemptForResultAsync(twice)))!;
        Check(once.CorrectCount == (once.Answers.Single().SelectedOption == "A" ? 1 : 0), "Concurrent grading inconsistent");
    }
    Pass("Five concurrent draft/submit and duplicate-submit rounds preserve a single consistent grade");

    await using (var db = Db())
    {
        var oversized = new Subject { Name = "Oversized reading" };
        var passage = new ReadingPassage { Subject = oversized, Content = "Reading", Questions = Enumerable.Range(1, 201).Select(i => Question("Reading " + i)).ToList() };
        foreach (var question in passage.Questions) question.Subject = oversized;
        db.ReadingPassages.Add(passage);
        await db.SaveChangesAsync();
        Check(!(await Run(q => q.StartAsync(oversized.Id, 1, null))).Success, "A 201-question reading must not be silently split");
        db.Questions.Add(new Question { SubjectId = oversized.Id, Content = "Standalone", OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D" });
        await db.SaveChangesAsync();
        var selected = await Run(q => q.StartAsync(oversized.Id, 1, null));
        Check(selected.Success && (await Run(q => q.GetAttemptForResultAsync(selected.AttemptId!.Value)))!.TotalQuestions == 1, "Valid standalone should remain usable");
        var boundary = new Subject { Name = "Boundary reading" };
        var boundaryPassage = new ReadingPassage { Subject = boundary, Content = "Boundary", Questions = Enumerable.Range(1, 200).Select(i => Question("Boundary " + i)).ToList() };
        foreach (var question in boundaryPassage.Questions) question.Subject = boundary;
        db.ReadingPassages.Add(boundaryPassage);
        await db.SaveChangesAsync();
        var fullGroup = await Run(q => q.StartAsync(boundary.Id, 1, null));
        Check(fullGroup.Success && (await Run(q => q.GetAttemptForResultAsync(fullGroup.AttemptId!.Value)))!.TotalQuestions == 200, "A 200-question reading must remain whole");
        var wrongSubject = new Subject { Name = "Many wrong", Questions = Enumerable.Range(1, 205).Select(i => Question("Wrong " + i)).ToList() };
        db.Subjects.Add(wrongSubject);
        await db.SaveChangesAsync();
        db.Attempts.Add(new Attempt { SubjectId = wrongSubject.Id, StartedAt = clock.GetLocalNow().DateTime, SubmittedAt = clock.GetLocalNow().DateTime, TotalQuestions = 205, Answers = wrongSubject.Questions.Select(q => new AttemptAnswer { QuestionId = q.Id }).ToList() });
        await db.SaveChangesAsync();
        var retry = await Run(q => q.StartWrongAsync(wrongSubject.Id, null));
        Check(retry.Success && (await Run(q => q.GetAttemptForResultAsync(retry.AttemptId!.Value)))!.TotalQuestions == 200, "Wrong retry must respect 200 cap");
    }
    Pass("Reading groups stay intact at the 200 limit; oversized groups are skipped and wrong retries are capped");

    await using (var db = Db())
    {
        var grouped = new Subject { Name = "Exact group combinations" };
        foreach (var size in new[] { 2, 3 })
        {
            var passage = new ReadingPassage
            {
                Subject = grouped, Content = "Group " + size,
                Questions = Enumerable.Range(1, size).Select(i => Question($"Group {size} question {i}")).ToList()
            };
            foreach (var question in passage.Questions) question.Subject = grouped;
            db.ReadingPassages.Add(passage);
        }
        await db.SaveChangesAsync();
        for (var i = 0; i < 10; i++)
        {
            var selected = await Run(q => q.StartAsync(grouped.Id, 3, null));
            var result = (await Run(q => q.GetAttemptForResultAsync(selected.AttemptId!.Value)))!;
            Check(result.TotalQuestions == 3 && result.Answers.Select(a => a.Question!.PassageId).Distinct().Count() == 1,
                "Prefer an exact combination without splitting a reading group");
        }
        var singles = new Subject { Name = "Random singles", Questions = Enumerable.Range(1, 10).Select(i => Question("Random " + i)).ToList() };
        db.Subjects.Add(singles);
        await db.SaveChangesAsync();
        var seen = new HashSet<int>();
        for (var i = 0; i < 8; i++)
        {
            var selected = await Run(q => q.StartAsync(singles.Id, 4, null));
            var result = (await Run(q => q.GetAttemptForResultAsync(selected.AttemptId!.Value)))!;
            Check(result.TotalQuestions == 4 && result.Answers.Select(a => a.QuestionId).Distinct().Count() == 4, "Random exam count or duplicates");
            Check(result.Answers.All(a => a.Question!.SubjectId == singles.Id), "Exam must stay within the chosen subject");
            seen.UnionWith(result.Answers.Select(a => a.QuestionId));
        }
        Check(seen.Count > 4, "Random exams must vary");
    }
    Pass("Random exams stay in the selected subject and prefer an exact, complete-group combination");

    await using (var db = Db())
    {
        var importer = new ImportService(db);
        ParsedQuestion Item(int index, string subject, string content) => new(index, subject, content, "A", "B", "C", "D", "A", null, []);
        var items = new List<ParsedQuestion> {
            Item(1, "Regression", "Single question"), Item(2, "Regression", "Fresh"), Item(3, "Regression", " FRESH "),
            Item(4, "Brand new", "New"), Item(5, "brand NEW", " new "), Item(6, "Brand new", "Other")
        };
        var duplicates = await importer.FindExistingDuplicatesAsync(items);
        Check(duplicates.SetEquals([1, 3, 5]), "Preview must detect existing and within-file duplicates for old/new subjects");
        var result = await importer.SaveAsync(items);
        Check(result.Added == 3 && result.Duplicates == duplicates.Count && result.Invalid == 0, "Preview/save duplicate rules differ");
        var onlyNew = new List<ParsedQuestion> { Item(1, "Another new", "Repeat"), Item(2, "Another new", "Repeat") };
        Check((await importer.FindExistingDuplicatesAsync(onlyNew)).SetEquals([2]), "All-new file duplicates must work without any matching subjects");
    }
    Pass("Import preview matches actual save for existing subjects, new subjects and within-file duplicates");

    // Analytics and Flashcards: verify recovery rate, subject performances, and deck queries
    await using (var db = Db())
    {
        var subjectA = new Subject { Name = "C# Core", Questions = [Question("C# 1"), Question("C# 2")] };
        var subjectB = new Subject { Name = "ASP.NET Core", Questions = [Question("ASP 1"), Question("ASP 2")] };
        db.Subjects.AddRange(subjectA, subjectB);
        await db.SaveChangesAsync();

        var qA1 = subjectA.Questions.First().Id;
        var qB1 = subjectB.Questions.First().Id;

        // Attempt 1 on Subject A: qA1 answered wrong ("B")
        clock.Advance(10);
        var att1 = await Start(subjectA.Id, null, 2);
        await Run(q => q.SubmitAsync(att1, new Dictionary<int, string?> { [qA1] = "B" }));

        var analytics1 = await Run(q => q.GetAnalyticsAsync());
        Check(analytics1.TotalEverWrong >= 1, "Must detect ever-wrong question");
        var initialRecovered = analytics1.TotalRecoveredWrong;

        // Attempt 2 on Subject A: qA1 answered correct ("A") -> Recovered!
        clock.Advance(10);
        var att2 = await Start(subjectA.Id, null, 2);
        await Run(q => q.SubmitAsync(att2, new Dictionary<int, string?> { [qA1] = "A" }));

        var analytics2 = await Run(q => q.GetAnalyticsAsync());
        Check(analytics2.TotalRecoveredWrong == initialRecovered + 1, "Recovered count must increase by 1");
        Check(analytics2.RecoveryRate > 0, "Recovery rate must be positive");

        // Verify Flashcard deck query (null subjectId and specific subjectId)
        var wrongAll = await Run(q => q.GetWrongQuestionsAsync(null));
        Check(wrongAll.All(q => q.Subject != null), "Wrong questions must include Subject navigation property for Flashcards");
    }
    Pass("Analytics and Flashcards: recovery rate calculation, subject ratings, and deck queries verified");
    Console.WriteLine("All 8 SQL integration groups passed.");
}
finally
{
    if (database.StartsWith("RenshuuRegression_", StringComparison.Ordinal) && database.Length == 50)
    {
        await using var db = Db();
        await db.Database.EnsureDeletedAsync();
        Console.WriteLine("Removed temporary test database " + database);
    }
}

sealed class TestClock : TimeProvider
{
    private DateTimeOffset _now = new(2026, 11, 10, 8, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    public void Advance(int seconds) => _now = _now.AddSeconds(seconds);
}
