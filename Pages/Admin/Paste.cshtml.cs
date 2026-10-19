using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Admin;

public class PasteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImportService _import;

    public PasteModel(AppDbContext db, ImportService import)
    {
        _db = db;
        _import = import;
    }

    public List<Subject> Subjects { get; private set; } = new();
    public ImportPreview? Preview { get; private set; }

    [BindProperty]
    public int SubjectId { get; set; }

    [BindProperty]
    public string? Text { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAsync();
        if (Subjects.Count == 0)
        {
            TempData["Error"] = "Hãy tạo ít nhất một môn học trước.";
            return RedirectToPage("/Admin/Index");
        }

        SubjectId = Subjects[0].Id;
        return Page();
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        await LoadAsync();

        var subject = Subjects.FirstOrDefault(s => s.Id == SubjectId);
        if (subject is null)
        {
            ModelState.AddModelError(string.Empty, "Hãy chọn môn học.");
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            ModelState.AddModelError(string.Empty, "Hãy dán nội dung câu hỏi vào ô văn bản.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var drafts = ImportService.ParsePaste(subject!.Name, Text);
        if (drafts.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Không tìm thấy câu hỏi nào trong nội dung đã dán.");
            return Page();
        }

        var duplicates = await _import.FindExistingDuplicatesAsync(drafts);
        Preview = new ImportPreview(drafts, duplicates);
        TempData["PasteDrafts"] = JsonSerializer.Serialize(drafts);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (TempData["PasteDrafts"] is not string json)
        {
            TempData["Error"] = "Phiên xem trước đã hết hạn. Vui lòng dán lại và bấm Xem trước.";
            return RedirectToPage();
        }

        var drafts = JsonSerializer.Deserialize<List<ParsedQuestion>>(json) ?? new List<ParsedQuestion>();
        var result = await _import.SaveAsync(drafts);
        TempData["Success"] =
            $"Đã thêm {result.Added} câu hỏi, bỏ qua {result.Duplicates} câu trùng, {result.Invalid} câu lỗi.";
        return RedirectToPage("/Admin/Questions");
    }

    private async Task LoadAsync() =>
        Subjects = await _db.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
}
