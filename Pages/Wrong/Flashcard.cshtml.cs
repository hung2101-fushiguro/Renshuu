using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Wrong;

public class FlashcardModel : PageModel
{
    private readonly QuizService _quiz;

    public FlashcardModel(QuizService quiz) => _quiz = quiz;

    public List<SubjectSummary> Subjects { get; private set; } = new();
    public SubjectSummary? Selected { get; private set; }
    public List<Question> Questions { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? SubjectId { get; set; }

    public async Task OnGetAsync()
    {
        Subjects = await _quiz.GetSubjectSummariesAsync();

        var target = SubjectId
            ?? Subjects.FirstOrDefault(s => s.WrongCount > 0)?.Id
            ?? Subjects.FirstOrDefault()?.Id;

        if (target is int id && id > 0)
        {
            SubjectId = id;
            Selected = Subjects.FirstOrDefault(s => s.Id == id);
            Questions = await _quiz.GetWrongQuestionsAsync(id);
        }
        else
        {
            Questions = await _quiz.GetWrongQuestionsAsync(null);
        }
    }
}
