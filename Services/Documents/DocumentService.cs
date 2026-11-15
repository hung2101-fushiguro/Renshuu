using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services.Security;

namespace TracNghiemWeb.Services.Documents;

public record DocumentEdit(string Title, string? Description, int? SubjectId, string? Version = null, string? TextContent = null);
public record DocumentResult(bool Success, int? Id = null, string? Error = null);

public sealed class DocumentService(AppDbContext db, ICurrentUser current, DocumentStorage storage, TimeProvider time)
{
    private IQueryable<UploadedDocument> Accessible => db.Documents.Where(d => current.Id != null && (d.OwnerId == current.Id || current.IsAdmin));
    public IQueryable<UploadedDocument> List(bool all) => Accessible.AsNoTracking().Where(d => all && current.IsAdmin || d.OwnerId == current.Id);
    public Task<UploadedDocument?> FindAsync(int id) => Accessible.AsNoTracking().Include(d => d.Owner).Include(d => d.Subject).FirstOrDefaultAsync(d => d.Id == id);

    private async Task<string?> ValidateAsync(DocumentEdit input)
    {
        if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 200) return "Tên tài liệu cần từ 1 đến 200 ký tự.";
        if (input.Description?.Length > 1000) return "Ghi chú tối đa 1.000 ký tự.";
        if (input.SubjectId.HasValue && !await db.Subjects.AnyAsync(s => s.Id == input.SubjectId)) return "Môn học không tồn tại.";
        return null;
    }

    public async Task<DocumentResult> UploadAsync(DocumentEdit input, IFormFile file, CancellationToken cancellation = default)
    {
        if (current.Id is null) return new(false, Error: "Vui lòng đăng nhập.");
        if (await ValidateAsync(input) is string invalid) return new(false, Error: invalid);
        StoredDocument stored;
        try { stored = await storage.SaveAsync(file, cancellation); }
        catch (DocumentValidationException error) { return new(false, Error: error.Message); }
        catch (IOException) { return new(false, Error: "Chưa lưu được tệp. Vui lòng thử lại."); }
        try
        {
            var document = new UploadedDocument
            {
                OwnerId = current.Id, Title = input.Title.Trim(), Description = input.Description?.Trim(), SubjectId = input.SubjectId,
                StorageKey = stored.Key, OriginalFileName = stored.OriginalName, ContentType = stored.ContentType, FileSize = stored.Size,
                UploadedAtUtc = time.GetUtcNow().UtcDateTime, UpdatedAtUtc = time.GetUtcNow().UtcDateTime
            };
            db.Documents.Add(document);
            await db.SaveChangesAsync(cancellation);
            return new(true, document.Id);
        }
        catch { storage.Delete(stored.Key); throw; }
    }

    private static byte[]? Version(string? value)
    {
        try { var version = Convert.FromBase64String(value ?? ""); return version.Length == 8 ? version : null; }
        catch (FormatException) { return null; }
    }

    public async Task<DocumentResult> UpdateAsync(int id, DocumentEdit input, IFormFile? file, CancellationToken cancellation = default)
    {
        var document = await Accessible.FirstOrDefaultAsync(d => d.Id == id, cancellation);
        if (document is null) return new(false, Error: "Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
        if (await ValidateAsync(input) is string invalid) return new(false, Error: invalid);
        if (Version(input.Version) is not { } version) return new(false, Error: "Vui lòng tải lại trang trước khi lưu.");
        if (!document.RowVersion.SequenceEqual(version)) return new(false, Error: "Tài liệu vừa được cập nhật. Vui lòng tải lại trang.");
        if (file is null && input.TextContent is not null)
        {
            if (!document.StorageKey.EndsWith(".txt") || input.TextContent.Length > 100_000)
                return new(false, Error: "Chỉ sửa trực tiếp văn bản TXT tối đa 100.000 ký tự.");
            var previous = await storage.ReadPreviewAsync(document.StorageKey, cancellation);
            if (previous.Truncated || previous.Text != input.TextContent)
            {
                var content = System.Text.Encoding.UTF8.GetBytes(input.TextContent);
                file = new FormFile(new MemoryStream(content), 0, content.Length, "Upload", document.OriginalFileName);
            }
        }
        StoredDocument? replacement = null;
        if (file is not null)
        {
            try { replacement = await storage.SaveAsync(file, cancellation); }
            catch (DocumentValidationException error) { return new(false, Error: error.Message); }
            catch (IOException) { return new(false, Error: "Chưa lưu được tệp mới. Tệp cũ vẫn được giữ."); }
        }
        var oldKey = document.StorageKey;
        document.Title = input.Title.Trim(); document.Description = input.Description?.Trim(); document.SubjectId = input.SubjectId;
        document.UpdatedAtUtc = time.GetUtcNow().UtcDateTime;
        if (replacement is not null)
        {
            document.StorageKey = replacement.Key; document.OriginalFileName = replacement.OriginalName;
            document.FileSize = replacement.Size; document.ContentType = replacement.ContentType;
        }
        db.Entry(document).Property(d => d.RowVersion).OriginalValue = version;
        try { await db.SaveChangesAsync(cancellation); }
        catch (DbUpdateConcurrencyException)
        {
            if (replacement is not null) storage.Delete(replacement.Key);
            return new(false, Error: "Tài liệu vừa được cập nhật hoặc đã xóa. Vui lòng tải lại trang.");
        }
        catch { if (replacement is not null) storage.Delete(replacement.Key); throw; }
        if (replacement is not null) storage.Delete(oldKey);
        return new(true, id);
    }

    public async Task<DocumentResult> DeleteAsync(int id, string? token)
    {
        var document = await Accessible.FirstOrDefaultAsync(d => d.Id == id);
        if (document is null) return new(false, Error: "Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
        if (Version(token) is not { } version) return new(false, Error: "Vui lòng tải lại trang trước khi xóa.");
        db.Entry(document).Property(d => d.RowVersion).OriginalValue = version;
        db.Documents.Remove(document);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return new(false, Error: "Tài liệu vừa được cập nhật hoặc đã xóa. Vui lòng tải lại trang."); }
        storage.Delete(document.StorageKey);
        return new(true, id);
    }
}
