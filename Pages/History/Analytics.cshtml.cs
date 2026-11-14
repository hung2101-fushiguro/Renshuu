using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.History;

public class AnalyticsModel : PageModel
{
    private readonly QuizService _quiz;

    public AnalyticsModel(QuizService quiz) => _quiz = quiz;

    public AnalyticsSummary Analytics { get; private set; } = null!;
    public List<SubjectSummary> Subjects { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? SubjectId { get; set; }

    public SubjectSummary? SelectedSubject { get; private set; }

    public async Task OnGetAsync()
    {
        Subjects = await _quiz.GetSubjectSummariesAsync();
        if (SubjectId.HasValue && SubjectId.Value > 0)
        {
            SelectedSubject = Subjects.FirstOrDefault(s => s.Id == SubjectId.Value);
        }

        Analytics = await _quiz.GetAnalyticsAsync(SubjectId);
    }
}
