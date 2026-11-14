using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Test;

public class TakeModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly QuizService _quiz;
    private readonly CurrentUser _currentUser;

    public TakeModel(AppDbContext db, QuizService quiz, CurrentUser currentUser)
    {
        _db = db;
        _quiz = quiz;
        _currentUser = currentUser;
    }

    public int AttemptId { get; private set; }
    public string SubjectName { get; private set; } = string.Empty;
    public int? TimeLimitMinutes { get; private set; }
    public DateTime StartedAt { get; private set; }
    public double? RemainingSeconds { get; private set; }
    public DateTime? DraftSavedAt { get; private set; }
    public List<TakeQuestion> Questions { get; private set; } = new();
    public List<TakeGroup> Groups { get; private set; } = new();

    [BindProperty(Name = "Answers")]
    public Dictionary<int, string?> Answers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attempt = await _db.Attempts
            .AsNoTracking()
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == _currentUser.UserId);

        if (attempt is null)
        {
            return NotFound();
        }

        if (attempt.IsSubmitted)
        {
            return RedirectToPage("/Test/Result", new { id });
        }

        var remaining = _quiz.GetRemainingTime(attempt);
        if (remaining is TimeSpan time && time <= TimeSpan.Zero)
        {
            await _quiz.SubmitAsync(id, new Dictionary<int, string?>(), savedOnly: true);
            TempData["Warning"] = "Bài đã hết giờ. Kết quả được chấm từ các đáp án đã lưu trên máy chủ.";
            return RedirectToPage("/Test/Result", new { id });
        }

        AttemptId = attempt.Id;
        SubjectName = attempt.Subject?.Name ?? string.Empty;
        TimeLimitMinutes = attempt.TimeLimitMinutes;
        StartedAt = attempt.StartedAt;
        RemainingSeconds = remaining?.TotalSeconds;
        DraftSavedAt = attempt.DraftSavedAt;

        var rows = await (
            from answer in _db.AttemptAnswers
            where answer.AttemptId == id
            join question in _db.Questions on answer.QuestionId equals question.Id
            join passage in _db.ReadingPassages on question.PassageId equals passage.Id into passageJoin
            from passage in passageJoin.DefaultIfEmpty()
            orderby answer.Id
            select new
            {
                question.Id,
                question.Content,
                question.OptionA,
                question.OptionB,
                question.OptionC,
                question.OptionD,
                question.OptionStyle,
                answer.SelectedOption,
                PassageId = passage == null ? (int?)null : passage.Id,
                PassageTitle = passage == null ? null : passage.Title,
                PassageContent = passage == null ? null : passage.Content,
            })
            .AsNoTracking()
            .ToListAsync();

        Questions = rows
            .Select(r =>
            {
                var passage = r.PassageId is int passageId
                    ? new TakePassage(passageId, r.PassageTitle, r.PassageContent ?? string.Empty)
                    : null;
                return new TakeQuestion(
                    r.Id,
                    r.Content,
                    r.OptionStyle,
                    passage,
                    BuildOptions(r.OptionA, r.OptionB, r.OptionC, r.OptionD, r.OptionStyle), r.SelectedOption);
            })
            .ToList();

        Groups = BuildGroups(Questions);

        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync(int id)
    {
        var result = await _quiz.SaveDraftAsync(id, Answers);
        return new JsonResult(new { result.Success, result.SavedCount, result.SavedAt, result.Error, result.Closed })
        { StatusCode = result.Success ? 200 : result.Closed ? 409 : 404 };
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _quiz.SubmitAsync(id, Answers);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToPage("/Index");
        }

        if (result.UsedSavedAnswers)
            TempData["Warning"] = "Bài đã quá hạn nộp. Máy chủ chỉ chấm các đáp án đã được lưu trước khi hết giờ.";

        return RedirectToPage("/Test/Result", new { id });
    }

    private static List<TakeGroup> BuildGroups(IReadOnlyList<TakeQuestion> questions)
    {
        var groups = new List<TakeGroup>();
        var passageGroups = new Dictionary<int, TakeGroup>();

        foreach (var question in questions)
        {
            if (question.Passage is null)
            {
                groups.Add(new TakeGroup(null, new List<TakeQuestion> { question }));
                continue;
            }

            if (!passageGroups.TryGetValue(question.Passage.Id, out var group))
            {
                group = new TakeGroup(question.Passage, new List<TakeQuestion>());
                passageGroups[question.Passage.Id] = group;
                groups.Add(group);
            }

            group.Questions.Add(question);
        }

        return groups;
    }

    private static List<TakeOption> BuildOptions(
        string a, string b, string c, string d, string? optionStyle) =>
        new()
        {
            new TakeOption("A", a, Display.OptionLabel(optionStyle, "A")),
            new TakeOption("B", b, Display.OptionLabel(optionStyle, "B")),
            new TakeOption("C", c, Display.OptionLabel(optionStyle, "C")),
            new TakeOption("D", d, Display.OptionLabel(optionStyle, "D")),
        };
}

public record TakePassage(int Id, string? Title, string Content);

public record TakeOption(string Key, string Text, string Label);

public record TakeQuestion(
    int Id,
    string Content,
    string OptionStyle,
    TakePassage? Passage,
    List<TakeOption> Options,
    string? SelectedOption);

public record TakeGroup(TakePassage? Passage, List<TakeQuestion> Questions);
