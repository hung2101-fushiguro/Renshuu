using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Wrong;

public class FlashcardModel : PageModel
{
    private readonly QuizService _quiz;

    public FlashcardModel(QuizService quiz) => _quiz = quiz;

    public List<SubjectSummary> Subjects { get; private set; } = new();
    public SubjectSummary? SelectedSubject { get; private set; }
    public List<Question> Questions { get; private set; } = new();
    public int SubjectId { get; private set; }

    public async Task OnGetAsync(int? subjectId)
    {
        Subjects = await _quiz.GetSubjectSummariesAsync();

        var target = subjectId
            ?? Subjects.FirstOrDefault(s => s.WrongCount > 0)?.Id
            ?? Subjects.FirstOrDefault()?.Id;

        if (target is int id)
        {
            SubjectId = id;
            SelectedSubject = Subjects.FirstOrDefault(s => s.Id == id);
            Questions = await _quiz.GetWrongQuestionsAsync(id);
        }
    }
}
