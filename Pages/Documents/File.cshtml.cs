using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Services.Documents;

namespace TracNghiemWeb.Pages.Documents;

public class FileModel(DocumentService documents, DocumentStorage storage) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int id, bool download = false)
    {
        var document = await documents.FindAsync(id);
        if (document is null) return NotFound();
        FileStream input;
        try { input = storage.Open(document.StorageKey); }
        catch (IOException) { return NotFound(); }
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.ContentSecurityPolicy = document.ContentType == "application/pdf" ? "frame-ancestors 'self'" : "default-src 'none'; sandbox";
        if (!download && document.ContentType == "application/pdf")
        {
            var disposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("inline");
            disposition.SetHttpFileName(document.OriginalFileName);
            Response.Headers.ContentDisposition = disposition.ToString();
        }
        return new FileStreamResult(input, document.ContentType)
        {
            FileDownloadName = download || document.ContentType != "application/pdf" ? document.OriginalFileName : null,
            EnableRangeProcessing = true
        };
    }
}
