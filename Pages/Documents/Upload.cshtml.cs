using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services.Documents;

namespace TracNghiemWeb.Pages.Documents;

[RequestSizeLimit(DocumentStorage.MaxRequestBytes)]
public class UploadModel(DocumentService documents, AppDbContext db) : PageModel
{
    [BindProperty] public DocumentForm Input { get; set; } = new();
    [BindProperty] public IFormFile? Upload { get; set; }
    public List<Subject> Subjects { get; private set; } = new();
    private async Task LoadAsync() => Subjects = await db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
    public async Task OnGetAsync() => await LoadAsync();
    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();
        if (Upload is null) ModelState.AddModelError(nameof(Upload), "Chọn tài liệu cần tải lên.");
        if (!ModelState.IsValid) return Page();
        var result = await documents.UploadAsync(new(Input.Title, Input.Description, Input.SubjectId), Upload!, HttpContext.RequestAborted);
        if (!result.Success) { ModelState.AddModelError(string.Empty, result.Error!); return Page(); }
        TempData["Success"] = "Đã tải tài liệu lên.";
        return RedirectToPage("View", new { id = result.Id });
    }
}
