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
    private readonly CurrentUser _currentUser;

    public IndexModel(AppDbContext db, QuizService quiz, CurrentUser currentUser)
    {
        _db = db;
        _quiz = quiz;
        _currentUser = currentUser;
    }

    public SubjectFolder Folder { get; private set; } = new();
    public List<SubjectSummary> Subjects { get; private set; } = new();

    public int TotalQuestions => Subjects.Sum(s => s.QuestionCount);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var folder = await _db.VisibleFolders(_currentUser.UserId)
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
