using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.History;

public class IndexModel : PageModel
{
    private readonly QuizService _quiz;

    public IndexModel(QuizService quiz) => _quiz = quiz;

    public List<Attempt> Attempts { get; private set; } = new();

    public async Task OnGetAsync() => Attempts = await _quiz.GetHistoryAsync();

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _quiz.DeleteAttemptAsync(id);

        if (deleted)
        {
            TempData["Success"] = "Đã xóa bài làm.";
        }
        else
        {
            TempData["Error"] = "Không tìm thấy bài làm cần xóa.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearAsync()
    {
        var count = await _quiz.ClearHistoryAsync();

        TempData[count > 0 ? "Success" : "Error"] = count > 0
            ? $"Đã xóa toàn bộ lịch sử ({count} bài làm)."
            : "Không có bài làm nào để xóa.";

        return RedirectToPage();
    }
}
