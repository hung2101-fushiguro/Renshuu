using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public List<SubjectRow> Subjects { get; private set; } = new();

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập tên môn học.")]
    [StringLength(200, ErrorMessage = "Tên môn học tối đa 200 ký tự.")]
    public string? NewSubjectName { get; set; }

    [BindProperty]
    public int EditId { get; set; }

    [BindProperty]
    public string? EditName { get; set; }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var name = NewSubjectName!.Trim();
        if (await _db.Subjects.AnyAsync(s => s.Name == name))
        {
            ModelState.AddModelError(nameof(NewSubjectName), "Môn học này đã tồn tại.");
            await LoadAsync();
            return Page();
        }

        _db.Subjects.Add(new Subject { Name = name });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã thêm môn \"{name}\".";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRenameAsync()
    {
        var subject = await _db.Subjects.FindAsync(EditId);
        if (subject is null)
        {
            TempData["Error"] = "Môn học không tồn tại.";
            return RedirectToPage();
        }

        var name = EditName?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 200)
        {
            TempData["Error"] = "Tên môn học không hợp lệ.";
            return RedirectToPage();
        }

        if (await _db.Subjects.AnyAsync(s => s.Name == name && s.Id != EditId))
        {
            TempData["Error"] = "Đã có môn học khác cùng tên.";
            return RedirectToPage();
        }

        subject.Name = name;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã đổi tên môn học.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject is null)
        {
            TempData["Error"] = "Môn học không tồn tại.";
            return RedirectToPage();
        }

        try
        {
            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa môn \"{subject.Name}\" và toàn bộ câu hỏi của môn.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] =
                $"Không thể xóa môn \"{subject.Name}\" vì đã có bài làm liên quan. " +
                "Hãy xóa lịch sử làm bài trước (hoặc giữ lại môn học).";
        }

        return RedirectToPage();
    }

    private async Task LoadAsync() =>
        Subjects = await _db.Subjects
            .OrderBy(s => s.Name)
            .Select(s => new SubjectRow(s.Id, s.Name, s.Questions.Count, s.Attempts.Count))
            .ToListAsync();

    public record SubjectRow(int Id, string Name, int QuestionCount, int AttemptCount);
}
