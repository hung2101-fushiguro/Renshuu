using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Content;

public class ReadingsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImportService _import;
    private readonly CurrentUser _currentUser;

    public ReadingsModel(AppDbContext db, ImportService import, CurrentUser currentUser)
    {
        _db = db;
        _import = import;
        _currentUser = currentUser;
    }

    public List<Subject> Subjects { get; private set; } = new();
    public ReadingImportPreview? Preview { get; private set; }

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
            ModelState.AddModelError(string.Empty, "Hãy dán nội dung bài đọc và câu hỏi vào ô văn bản.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var passages = ImportService.ParseReadingPassages(subjectName, Text);
        if (passages.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không tìm thấy bài đọc nào. Cần có dòng [BÀI ĐỌC] và [CÂU HỎI] theo đúng định dạng.");
            return Page();
        }

        var questions = passages.SelectMany(p => p.Questions).ToList();
        var duplicates = await _import.FindExistingDuplicatesAsync(questions);
        Preview = new ReadingImportPreview(passages, duplicates);
        TempData["ReadingDrafts"] = JsonSerializer.Serialize(passages);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (TempData["ReadingDrafts"] is not string json)
        {
            TempData["Error"] = "Phiên xem trước đã hết hạn. Vui lòng dán lại và bấm Xem trước.";
            return RedirectToPage();
        }

        var passages = JsonSerializer.Deserialize<List<ParsedPassage>>(json) ?? new List<ParsedPassage>();
        var result = await _import.SaveReadingPassagesAsync(passages);
        TempData["Success"] =
            $"Đã thêm {result.PassagesAdded} bài đọc với {result.QuestionsAdded} câu hỏi, " +
            $"bỏ qua {result.Duplicates} câu trùng, {result.Invalid} câu lỗi.";
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
