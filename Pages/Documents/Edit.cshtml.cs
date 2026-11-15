using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services.Documents;

namespace TracNghiemWeb.Pages.Documents;

[RequestSizeLimit(DocumentStorage.MaxRequestBytes)]
public class EditModel(DocumentService documents, DocumentStorage storage, AppDbContext db) : PageModel
{
    [BindProperty] public DocumentForm Input { get; set; } = new();
    [BindProperty] public IFormFile? Upload { get; set; }
    public UploadedDocument Document { get; private set; } = new();
    public List<Subject> Subjects { get; private set; } = new();
    public bool CanEditText { get; private set; }
    private async Task<bool> LoadAsync(int id)
    {
        var document = await documents.FindAsync(id);
        if (document is null) return false;
        Document = document;
        if (Document.StorageKey.EndsWith(".txt"))
        {
            try { CanEditText = !(await storage.ReadPreviewAsync(Document.StorageKey)).Truncated; }
            catch (IOException) { CanEditText = false; }
        }
        Subjects = await db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        return true;
    }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAsync(id)) return NotFound();
        Input = new() { Title = Document.Title, Description = Document.Description, SubjectId = Document.SubjectId, Version = Convert.ToBase64String(Document.RowVersion) };
        if (CanEditText) Input.TextContent = (await storage.ReadPreviewAsync(Document.StorageKey)).Text;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!await LoadAsync(id)) return NotFound();
        if (!ModelState.IsValid) return Page();
        var result = await documents.UpdateAsync(id, new(Input.Title, Input.Description, Input.SubjectId, Input.Version, Input.TextContent), Upload, HttpContext.RequestAborted);
        if (!result.Success) { ModelState.AddModelError(string.Empty, result.Error!); return Page(); }
        TempData["Success"] = "Đã cập nhật tài liệu.";
        return RedirectToPage("View", new { id });
    }
}
