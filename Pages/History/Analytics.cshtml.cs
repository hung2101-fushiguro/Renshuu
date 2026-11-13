using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.History;

public class AnalyticsModel : PageModel
{
    private readonly QuizService _quiz;

    public AnalyticsModel(QuizService quiz) => _quiz = quiz;

    public AnalyticsData Analytics { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Analytics = await _quiz.GetAnalyticsAsync();
    }
}
