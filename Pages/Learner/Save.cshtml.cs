using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
namespace TracNghiemWeb.Pages.Learner;
public class SaveModel(AppDbContext db) : PageModel
{
    public Question Question { get; set; } = null!;
    public SavedQuestion? Saved { get; set; }
    public async Task<IActionResult> OnGetAsync(int questionId)
    {
        var question = await db.Questions.AsNoTracking().FirstOrDefaultAsync(q => q.Id == questionId);
        if (question is null) return NotFound();
        Question = question;
        Saved = await db.SavedQuestions.AsNoTracking().SingleOrDefaultAsync(s => s.QuestionId == questionId);
        return Page();
    }
}
