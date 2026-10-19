using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages;

public class IndexModel : PageModel
{
    private readonly QuizService _quiz;

    public IndexModel(QuizService quiz) => _quiz = quiz;

    public List<SubjectSummary> Subjects { get; private set; } = new();

    [BindProperty]
    public StartInput Input { get; set; } = new();

    public class StartInput
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học.")]
        public int SubjectId { get; set; }

        [Range(1, QuizService.MaxQuestionsPerAttempt,
            ErrorMessage = "Số câu hỏi phải từ 1 đến 200.")]
        public int QuestionCount { get; set; } = 10;

        [Range(1, QuizService.MaxTimeLimitMinutes,
            ErrorMessage = "Thời gian làm bài phải từ 1 đến 240 phút.")]
        public int? TimeLimitMinutes { get; set; }
    }

    public async Task OnGetAsync(int? subjectId)
    {
        await LoadSubjectsAsync();

        if (subjectId.HasValue && Subjects.Any(s => s.Id == subjectId.Value))
        {
            Input.SubjectId = subjectId.Value;
            var subject = Subjects.First(s => s.Id == subjectId.Value);
            Input.QuestionCount = Math.Clamp(10, 1, Math.Max(1, subject.QuestionCount));
        }
        else if (Subjects.Count > 0)
        {
            Input.SubjectId = Subjects[0].Id;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSubjectsAsync();
            return Page();
        }

        var result = await _quiz.StartAsync(Input.SubjectId, Input.QuestionCount, Input.TimeLimitMinutes);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await LoadSubjectsAsync();
            return Page();
        }

        return RedirectToPage("/Test/Take", new { id = result.AttemptId });
    }

    private async Task LoadSubjectsAsync() => Subjects = await _quiz.GetSubjectSummariesAsync();
}
