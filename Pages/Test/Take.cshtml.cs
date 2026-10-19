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

    public TakeModel(AppDbContext db, QuizService quiz)
    {
        _db = db;
        _quiz = quiz;
    }

    public int AttemptId { get; private set; }
    public string SubjectName { get; private set; } = string.Empty;
    public int? TimeLimitMinutes { get; private set; }
    public DateTime StartedAt { get; private set; }
    public List<TakeQuestion> Questions { get; private set; } = new();

    [BindProperty]
    public Dictionary<int, string?> Answers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attempt = await _db.Attempts
            .AsNoTracking()
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attempt is null)
        {
            return NotFound();
        }

        if (attempt.IsSubmitted)
        {
            return RedirectToPage("/Test/Result", new { id });
        }

        AttemptId = attempt.Id;
        SubjectName = attempt.Subject?.Name ?? string.Empty;
        TimeLimitMinutes = attempt.TimeLimitMinutes;
        StartedAt = attempt.StartedAt;

        var rows = await (
            from answer in _db.AttemptAnswers
            where answer.AttemptId == id
            join question in _db.Questions on answer.QuestionId equals question.Id
            orderby answer.Id
            select new
            {
                question.Id,
                question.Content,
                question.OptionA,
                question.OptionB,
                question.OptionC,
                question.OptionD,
            })
            .AsNoTracking()
            .ToListAsync();

        Questions = rows
            .Select(r => new TakeQuestion(r.Id, r.Content, ShuffleOptions(r.OptionA, r.OptionB, r.OptionC, r.OptionD)))
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _quiz.SubmitAsync(id, Answers);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToPage("/Index");
        }

        return RedirectToPage("/Test/Result", new { id });
    }

    private static List<TakeOption> ShuffleOptions(params string[] options)
    {
        string[] keys = { "A", "B", "C", "D" };
        return keys
            .Zip(options, (key, text) => new TakeOption(key, text))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();
    }
}

public record TakeOption(string Key, string Text);

public record TakeQuestion(int Id, string Content, List<TakeOption> Options);
