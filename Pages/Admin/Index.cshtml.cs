using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Admin;

public class IndexModel : PageModel
{
    private const int MaxFolderNameLength = 200;

    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    public List<SubjectRow> Subjects { get; private set; } = new();
    public List<FolderRow> Folders { get; private set; } = new();

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập tên môn học.")]
    [StringLength(200, ErrorMessage = "Tên môn học tối đa 200 ký tự.")]
    public string? NewSubjectName { get; set; }

    [BindProperty]
    public int? NewSubjectFolderId { get; set; }

    [BindProperty]
    public string? NewSubjectNewFolderName { get; set; }

    [BindProperty]
    public string? NewFolderName { get; set; }

    [BindProperty]
    public int EditId { get; set; }

    [BindProperty]
    public string? EditName { get; set; }

    [BindProperty]
    public int? EditFolderId { get; set; }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateFolderAsync()
    {
        var name = NewFolderName?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > MaxFolderNameLength)
        {
            TempData["Error"] = "Tên thư mục phải từ 1 đến 200 ký tự.";
            return RedirectToPage();
        }

        if (await _db.SubjectFolders.AnyAsync(f => f.Name == name))
        {
            TempData["Error"] = "Thư mục này đã tồn tại.";
            return RedirectToPage();
        }

        _db.SubjectFolders.Add(new SubjectFolder { Name = name });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã thêm thư mục \"{name}\".";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRenameFolderAsync(int id, string? name)
    {
        var folder = await _db.SubjectFolders.FindAsync(id);
        if (folder is null)
        {
            TempData["Error"] = "Thư mục không tồn tại.";
            return RedirectToPage();
        }

        var newName = name?.Trim();
        if (string.IsNullOrEmpty(newName) || newName.Length > MaxFolderNameLength)
        {
            TempData["Error"] = "Tên thư mục không hợp lệ.";
            return RedirectToPage();
        }

        if (await _db.SubjectFolders.AnyAsync(f => f.Name == newName && f.Id != id))
        {
            TempData["Error"] = "Đã có thư mục khác cùng tên.";
            return RedirectToPage();
        }

        folder.Name = newName;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã đổi tên thư mục.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteFolderAsync(int id)
    {
        var folder = await _db.SubjectFolders.FindAsync(id);
        if (folder is null)
        {
            TempData["Error"] = "Thư mục không tồn tại.";
            return RedirectToPage();
        }

        await _db.Subjects
            .Where(s => s.FolderId == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.FolderId, (int?)null));

        _db.SubjectFolders.Remove(folder);
        await _db.SaveChangesAsync();
        TempData["Success"] =
            $"Đã xóa thư mục \"{folder.Name}\". Các môn bên trong được chuyển ra ngoài.";
        return RedirectToPage();
    }

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

        int? folderId = null;
        var newFolderName = NewSubjectNewFolderName?.Trim();
        if (!string.IsNullOrEmpty(newFolderName))
        {
            if (newFolderName.Length > MaxFolderNameLength)
            {
                ModelState.AddModelError(nameof(NewSubjectNewFolderName), "Tên thư mục tối đa 200 ký tự.");
                await LoadAsync();
                return Page();
            }

            var folder = await _db.SubjectFolders.FirstOrDefaultAsync(f => f.Name == newFolderName);
            if (folder is null)
            {
                folder = new SubjectFolder { Name = newFolderName };
                _db.SubjectFolders.Add(folder);
                await _db.SaveChangesAsync();
            }

            folderId = folder.Id;
        }
        else if (NewSubjectFolderId is int selectedFolderId)
        {
            if (!await _db.SubjectFolders.AnyAsync(f => f.Id == selectedFolderId))
            {
                ModelState.AddModelError(nameof(NewSubjectFolderId), "Thư mục không tồn tại.");
                await LoadAsync();
                return Page();
            }

            folderId = selectedFolderId;
        }

        _db.Subjects.Add(new Subject { Name = name, FolderId = folderId });
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

        if (EditFolderId is int folderId
            && !await _db.SubjectFolders.AnyAsync(f => f.Id == folderId))
        {
            TempData["Error"] = "Thư mục không tồn tại.";
            return RedirectToPage();
        }

        subject.Name = name;
        subject.FolderId = EditFolderId;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật môn học.";
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

    private async Task LoadAsync()
    {
        Folders = await _db.SubjectFolders
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .Select(f => new FolderRow(f.Id, f.Name, f.Subjects.Count))
            .ToListAsync();

        var subjects = await _db.Subjects
            .AsNoTracking()
            .Select(s => new SubjectRow(
                s.Id,
                s.Name,
                s.FolderId,
                s.Folder != null ? s.Folder.Name : null,
                s.Questions.Count,
                s.Attempts.Count))
            .ToListAsync();

        Subjects = subjects
            .OrderBy(s => s.FolderName is null ? 1 : 0)
            .ThenBy(s => s.FolderName)
            .ThenBy(s => s.Name)
            .ToList();
    }

    public record FolderRow(int Id, string Name, int SubjectCount);

    public record SubjectRow(
        int Id,
        string Name,
        int? FolderId,
        string? FolderName,
        int QuestionCount,
        int AttemptCount);
}
