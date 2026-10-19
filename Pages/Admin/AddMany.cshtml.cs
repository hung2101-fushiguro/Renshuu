using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Admin;

public class AddManyModel : PageModel
{
    public const int MaxRows = 50;

    private readonly AppDbContext _db;
    private readonly ImportService _import;

    public AddManyModel(AppDbContext db, ImportService import)
    {
        _db = db;
        _import = import;
    }

    public List<Subject> Subjects { get; private set; } = new();
    public List<string> RowErrors { get; private set; } = new();

    [BindProperty]
    public int SubjectId { get; set; }

    [BindProperty]
    public List<QuestionRow> Rows { get; set; } = new();

    public class QuestionRow
    {
        public string? Content { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectOption { get; set; } = "A";
        public string? Explanation { get; set; }

        public bool HasAnyValue =>
            !string.IsNullOrWhiteSpace(Content)
            || !string.IsNullOrWhiteSpace(OptionA)
            || !string.IsNullOrWhiteSpace(OptionB)
            || !string.IsNullOrWhiteSpace(OptionC)
            || !string.IsNullOrWhiteSpace(OptionD)
            || !string.IsNullOrWhiteSpace(Explanation);
    }

    public async Task<IActionResult> OnGetAsync(int? subjectId)
    {
        Subjects = await _db.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        if (Subjects.Count == 0)
        {
            TempData["Error"] = "Hãy tạo ít nhất một môn học trước.";
            return RedirectToPage("/Admin/Index");
        }

        SubjectId = subjectId is int sid && Subjects.Any(s => s.Id == sid) ? sid : Subjects[0].Id;
        if (Rows.Count == 0)
        {
            Rows = new List<QuestionRow> { new(), new(), new() };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Subjects = await _db.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        var subject = Subjects.FirstOrDefault(s => s.Id == SubjectId);
        if (subject is null)
        {
            TempData["Error"] = "Môn học không hợp lệ.";
            return RedirectToPage();
        }

        Rows = (Rows ?? new()).Take(MaxRows).ToList();
        var filled = Rows.Where(r => r.HasAnyValue).ToList();
        if (filled.Count == 0)
        {
            RowErrors.Add("Chưa có dòng nào có dữ liệu. Hãy nhập ít nhất một câu hỏi.");
            Rows = filled;
            return Page();
        }

        var drafts = new List<ParsedQuestion>();
        for (var i = 0; i < filled.Count; i++)
        {
            var row = filled[i];
            var draft = ImportService.BuildQuestion(
                i + 1, subject.Name,
                row.Content, row.OptionA, row.OptionB, row.OptionC, row.OptionD,
                row.CorrectOption, row.Explanation);
            drafts.Add(draft);
            foreach (var error in draft.Errors)
            {
                RowErrors.Add($"Dòng {i + 1}: {error}");
            }
        }

        if (RowErrors.Count > 0)
        {
            Rows = filled;
            return Page();
        }

        var result = await _import.SaveAsync(drafts);
        TempData["Success"] =
            $"Đã thêm {result.Added} câu hỏi, bỏ qua {result.Duplicates} câu trùng với câu đã có.";
        return RedirectToPage("/Admin/Questions", new { subjectId = SubjectId });
    }
}
