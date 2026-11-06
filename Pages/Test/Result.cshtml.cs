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
    public List<ResultGroup> Groups { get; private set; } = new();

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
        Groups = BuildGroups(attempt.Answers);
        return Page();
    }

    private static List<ResultGroup> BuildGroups(IEnumerable<AttemptAnswer> answers)
    {
        var groups = new List<ResultGroup>();
        var passageGroups = new Dictionary<int, ResultGroup>();

        foreach (var answer in answers)
        {
            var passage = answer.Question?.Passage;
            if (passage is null)
            {
                groups.Add(new ResultGroup(null, new List<AttemptAnswer> { answer }));
                continue;
            }

            if (!passageGroups.TryGetValue(passage.Id, out var group))
            {
                group = new ResultGroup(passage, new List<AttemptAnswer>());
                passageGroups[passage.Id] = group;
                groups.Add(group);
            }

            group.Answers.Add(answer);
        }

        return groups;
    }
}

public record ResultGroup(ReadingPassage? Passage, List<AttemptAnswer> Answers);
