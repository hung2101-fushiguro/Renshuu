using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Admin;

public class QuestionsModel : PageModel
{
    public const int PageSize = 15;

    private readonly AppDbContext _db;

    public QuestionsModel(AppDbContext db) => _db = db;

    public List<Subject> Subjects { get; private set; } = new();
    public List<Question> Questions { get; private set; } = new();
    public int? SubjectId { get; private set; }
    public string? Search { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public int TotalCount { get; private set; }

    public async Task OnGetAsync(int? subjectId, string? search, int page = 1)
    {
        Subjects = await _db.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        SubjectId = subjectId;
        Search = search?.Trim();
        PageNumber = Math.Max(1, page);

        var query = _db.Questions.AsNoTracking();
        if (subjectId is int id)
        {
            query = query.Where(q => q.SubjectId == id);
        }

        if (!string.IsNullOrEmpty(Search))
        {
            query = query.Where(q => q.Content.Contains(Search));
        }

        TotalCount = await query.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        PageNumber = Math.Min(PageNumber, TotalPages);

        Questions = await query
            .Include(q => q.Subject)
            .OrderBy(q => q.SubjectId)
            .ThenBy(q => q.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int? subjectId, string? search, int page = 1)
    {
        var question = await _db.Questions.FindAsync(id);
        if (question is null)
        {
            TempData["Error"] = "Câu hỏi không tồn tại.";
        }
        else
        {
            try
            {
                _db.Questions.Remove(question);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa câu hỏi.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa câu hỏi này vì đã xuất hiện trong bài làm của người dùng.";
            }
        }

        return RedirectToPage(new { subjectId, search, page });
    }
}
