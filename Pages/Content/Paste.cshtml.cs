using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Content;

public class PasteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImportService _import;
    private readonly CurrentUser _currentUser;

    public PasteModel(AppDbContext db, ImportService import, CurrentUser currentUser)
    {
        _db = db;
        _import = import;
        _currentUser = currentUser;
    }

    public List<Subject> Subjects { get; private set; } = new();
    public ImportPreview? Preview { get; private set; }

    [BindProperty]
    public int SubjectId { get; set; }

    [BindProperty]
    [StringLength(200, ErrorMessage = "Tên môn học tối đa 200 ký tự.")]
    public string? NewSubjectName { get; set; }

    [BindProperty]
    public string? Text { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAsync();
        SubjectId = Subjects.FirstOrDefault()?.Id ?? 0;
        return Page();
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        await LoadAsync();

        var newSubjectName = NewSubjectName?.Trim();
        string? subjectName = null;
        if (!string.IsNullOrEmpty(newSubjectName))
        {
            subjectName = Subjects.FirstOrDefault(s => string.Equals(s.Name, newSubjectName, StringComparison.OrdinalIgnoreCase))?.Name
                ?? newSubjectName;
        }
        else
        {
            subjectName = Subjects.FirstOrDefault(s => s.Id == SubjectId)?.Name;
            if (subjectName is null)
            {
                ModelState.AddModelError(string.Empty, "Hãy chọn môn học hoặc nhập tên môn mới.");
            }
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            ModelState.AddModelError(string.Empty, "Hãy dán nội dung câu hỏi vào ô văn bản.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var drafts = ImportService.ParsePaste(subjectName, Text);
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
        return RedirectToPage("/Content/Questions");
    }

    private async Task LoadAsync() =>
        Subjects = await _db.Subjects
            .Include(s => s.Folder)
            .Where(s => s.OwnerId == _currentUser.UserId)
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
}
