using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services.Documents;

namespace TracNghiemWeb.Pages.Documents;

public class ViewModel(DocumentService documents, DocumentStorage storage) : PageModel
{
    public UploadedDocument Document { get; private set; } = new();
    public DocumentPreview Preview { get; private set; } = new(string.Empty, false);
    public string? PreviewError { get; private set; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var document = await documents.FindAsync(id);
        if (document is null) return NotFound();
        Document = document;
        if (document.ContentType != "application/pdf")
        {
            try { Preview = await storage.ReadPreviewAsync(document.StorageKey, HttpContext.RequestAborted); }
            catch (Exception error) when (error is IOException or XmlException or DocumentValidationException)
            { PreviewError = "Chưa tạo được bản xem trước. Bạn có thể tải tệp gốc để đọc."; }
        }
        return Page();
    }
    public async Task<IActionResult> OnPostDeleteAsync(int id, string? version)
    {
        var document = await documents.FindAsync(id);
        if (document is null) return NotFound();
        var result = await documents.DeleteAsync(id, version);
        if (!result.Success) { TempData["Error"] = result.Error; return RedirectToPage(new { id }); }
        TempData["Success"] = "Đã xóa tài liệu.";
        return RedirectToPage("Index");
    }
}
