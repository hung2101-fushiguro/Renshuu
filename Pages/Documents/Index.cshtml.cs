using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;
using TracNghiemWeb.Services.Documents;

namespace TracNghiemWeb.Pages.Documents;

public class IndexModel(DocumentService documents, AppDbContext db, CurrentUser current) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Type { get; set; }
    [BindProperty(SupportsGet = true)] public int? SubjectId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int Total { get; private set; }
    public int PageCount => Math.Max(1, (Total + 11) / 12);
    public long TotalBytes { get; private set; }
    public List<UploadedDocument> Items { get; private set; } = new();
    public List<Subject> Subjects { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Q = Q?.Trim();
        if (Q?.Length > 200) Q = Q[..200];
        var query = documents.List();
        if (!string.IsNullOrEmpty(Q)) query = query.Where(d => d.Title.Contains(Q) || d.OriginalFileName.Contains(Q));
        if (Type is "pdf" or "docx" or "pptx" or "txt") query = query.Where(d => d.StorageKey.EndsWith("." + Type));
        else Type = null;
        if (SubjectId.HasValue) query = query.Where(d => d.SubjectId == SubjectId);
        Total = await query.CountAsync();
        TotalBytes = await query.SumAsync(d => (long?)d.FileSize) ?? 0;
        PageNumber = Math.Clamp(PageNumber, 1, PageCount);
        Items = await query.Include(d => d.Owner).Include(d => d.Subject)
            .OrderByDescending(d => d.UpdatedAtUtc).ThenByDescending(d => d.Id)
            .Skip((PageNumber - 1) * 12).Take(12).ToListAsync();
        Subjects = await db.VisibleSubjects(current.UserId).AsNoTracking().OrderBy(s => s.Name).ToListAsync();
    }
}
