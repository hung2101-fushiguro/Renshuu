using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Folder;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly QuizService _quiz;

    public IndexModel(AppDbContext db, QuizService quiz)
    {
        _db = db;
        _quiz = quiz;
    }

    public SubjectFolder Folder { get; private set; } = new();
    public List<SubjectSummary> Subjects { get; private set; } = new();

    public int TotalQuestions => Subjects.Sum(s => s.QuestionCount);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var folder = await _db.SubjectFolders
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (folder is null)
        {
            return NotFound();
        }

        Folder = folder;

        var subjects = await _quiz.GetSubjectSummariesAsync();
        Subjects = subjects
            .Where(s => s.FolderId == id)
            .ToList();

        return Page();
    }
}
