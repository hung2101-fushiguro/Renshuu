using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Test;

public class ResultModel : PageModel
{
    private readonly QuizService _quiz;

    public ResultModel(QuizService quiz) => _quiz = quiz;

    public Attempt Attempt { get; private set; } = new();

    public int WrongCount => Attempt.Answers.Count(a => !a.IsCorrect);

    public int BlankCount => Attempt.Answers.Count(a => a.SelectedOption is null);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attempt = await _quiz.GetAttemptForResultAsync(id);

        if (attempt is null)
        {
            return NotFound();
        }

        if (!attempt.IsSubmitted)
        {
            return RedirectToPage("/Test/Take", new { id });
        }

        Attempt = attempt;
        return Page();
    }
}
