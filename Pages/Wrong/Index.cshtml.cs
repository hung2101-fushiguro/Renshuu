using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Wrong;

public class IndexModel : PageModel
{
    private readonly QuizService _quiz;

    public IndexModel(QuizService quiz) => _quiz = quiz;

    public List<SubjectSummary> Subjects { get; private set; } = new();

    public SubjectSummary? Selected { get; private set; }

    public List<Question> Questions { get; private set; } = new();

    [BindProperty]
    public int SubjectId { get; set; }

    [BindProperty]
    [Range(1, QuizService.MaxTimeLimitMinutes, ErrorMessage = "Thời gian làm bài phải từ 1 đến 240 phút.")]
    public int? TimeLimitMinutes { get; set; }

    public async Task OnGetAsync(int? subjectId)
    {
        Subjects = await _quiz.GetSubjectSummariesAsync();

        var target = subjectId
            ?? Subjects.FirstOrDefault(s => s.WrongCount > 0)?.Id
            ?? Subjects.FirstOrDefault()?.Id;

        if (target is int id)
        {
            SubjectId = id;
            Selected = Subjects.FirstOrDefault(s => s.Id == id);
            Questions = await _quiz.GetWrongQuestionsAsync(id);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(SubjectId);
            return Page();
        }

        var result = await _quiz.StartWrongAsync(SubjectId, TimeLimitMinutes);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToPage("/Wrong/Index", new { subjectId = SubjectId });
        }

        return RedirectToPage("/Test/Take", new { id = result.AttemptId });
    }
}
