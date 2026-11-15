using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace TracNghiemWeb.Services.Documents;

public sealed class DocumentValidationException(string message) : Exception(message);
public record StoredDocument(string Key, string OriginalName, string ContentType, long Size);
public record DocumentPreview(string Text, bool Truncated);

public sealed class DocumentStorage
{
    public const long MaxFileBytes = 20 * 1024 * 1024;
    public const long MaxRequestBytes = MaxFileBytes + 1024 * 1024;
    private const int PreviewLimit = 100_000;
    private readonly string _root;
    private readonly ILogger<DocumentStorage> _logger;
    private static readonly IReadOnlyDictionary<string, string> Types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf", [".txt"] = "text/plain; charset=utf-8",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    };

    public DocumentStorage(IConfiguration config, IWebHostEnvironment environment, ILogger<DocumentStorage> logger)
    {
        _logger = logger;
        var configured = config["Documents:StoragePath"];
        _root = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TracNghiemWeb", "Documents") : configured);
        var appRoot = Path.GetFullPath(environment.ContentRootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (_root.Equals(appRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) || _root.StartsWith(appRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Documents:StoragePath phải nằm ngoài thư mục ứng dụng.");
        Directory.CreateDirectory(_root);
    }

    private string Resolve(string key)
    {
        if (!Regex.IsMatch(key, @"\A[a-f0-9]{32}\.(pdf|txt|docx|pptx)\z")) throw new InvalidOperationException("Invalid document storage key.");
        return Path.Combine(_root, key);
    }

    public FileStream Open(string key) => new(Resolve(key), FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 65536, true);

    public void Delete(string key)
    {
        try { File.Delete(Resolve(key)); }
        catch (IOException error) { _logger.LogWarning(error, "Could not remove detached document {Key}", key); }
        catch (UnauthorizedAccessException error) { _logger.LogWarning(error, "Could not remove detached document {Key}", key); }
    }

    public async Task<StoredDocument> SaveAsync(IFormFile file, CancellationToken cancellation = default)
    {
        if (file.Length <= 0) throw new DocumentValidationException("Tệp đang trống.");
        if (file.Length > MaxFileBytes) throw new DocumentValidationException("Mỗi tệp tối đa 20 MB.");
        var name = Path.GetFileName(file.FileName.Replace('\\', '/'));
        name = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (name.Length is 0 or > 255) throw new DocumentValidationException("Tên tệp cần từ 1 đến 255 ký tự.");
        var extension = Path.GetExtension(name).ToLowerInvariant();
        if (!Types.TryGetValue(extension, out var type)) throw new DocumentValidationException("Chỉ nhận tệp PDF, DOCX, PPTX hoặc TXT.");
        var key = Guid.NewGuid().ToString("N") + extension;
        var path = Resolve(key);
        try
        {
            await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
            await using (var input = file.OpenReadStream())
            {
                var buffer = new byte[65536];
                long size = 0;
                int read;
                while ((read = await input.ReadAsync(buffer, cancellation)) > 0)
                {
                    size += read;
                    if (size > MaxFileBytes) throw new DocumentValidationException("Mỗi tệp tối đa 20 MB.");
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellation);
                }
                if (size == 0 || size != file.Length) throw new DocumentValidationException("Tệp chưa được tải lên đầy đủ. Vui lòng chọn lại.");
            }
            await ValidateAsync(key, extension, cancellation);
            return new(key, name, type, new FileInfo(path).Length);
        }
        catch (Exception error) when (error is InvalidDataException or XmlException or DecoderFallbackException)
        {
            Delete(key);
            throw new DocumentValidationException("Nội dung tệp không khớp định dạng hoặc không đọc được. Vui lòng chọn tệp hợp lệ.");
        }
        catch { Delete(key); throw; }
    }

    private async Task ValidateAsync(string key, string extension, CancellationToken cancellation)
    {
        await using var stream = Open(key);
        if (extension == ".pdf")
        {
            var header = new byte[5];
            if (await stream.ReadAsync(header, cancellation) != 5 || Encoding.ASCII.GetString(header) != "%PDF-")
                throw new DocumentValidationException("Tệp này không phải tài liệu PDF hợp lệ.");
            return;
        }
        if (extension == ".txt")
        {
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true, 4096, true);
            var chars = new char[8192];
            int length;
            while ((length = await reader.ReadAsync(chars.AsMemory(), cancellation)) > 0)
                if (chars.Take(length).Any(c => char.IsControl(c) && c is not ('\r' or '\n' or '\t' or '\f')))
                    throw new DocumentValidationException("Tệp TXT cần chứa văn bản, không phải dữ liệu nhị phân.");
            return;
        }
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
        if (archive.Entries.Count > 5000 || archive.Entries.Sum(e => e.Length) > 100L * 1024 * 1024)
            throw new DocumentValidationException("Tài liệu có cấu trúc quá lớn để xử lý.");
        if (archive.Entries.Any(e => e.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase)))
            throw new DocumentValidationException("Vui lòng dùng tài liệu không chứa macro.");
        var required = extension == ".docx" ? "word/document.xml" : "ppt/presentation.xml";
        if (archive.GetEntry("[Content_Types].xml") is null || archive.GetEntry(required) is not { } entry)
            throw new DocumentValidationException("Nội dung tệp không khớp định dạng DOCX/PPTX.");
        ReadXml(entry);
    }

    private static XDocument ReadXml(ZipArchiveEntry entry)
    {
        if (entry.Length > 4 * 1024 * 1024) throw new DocumentValidationException("Phần văn bản trong tài liệu quá lớn để xem trước.");
        using var input = entry.Open();
        using var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 4 * 1024 * 1024 });
        return XDocument.Load(reader);
    }

    public async Task<DocumentPreview> ReadPreviewAsync(string key, CancellationToken cancellation = default)
    {
        var text = new StringBuilder();
        var extension = Path.GetExtension(key);
        await using var stream = Open(key);
        if (extension == ".txt")
        {
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true));
            var buffer = new char[PreviewLimit + 1];
            var length = await reader.ReadBlockAsync(buffer.AsMemory(), cancellation);
            return new(new string(buffer, 0, Math.Min(length, PreviewLimit)), length > PreviewLimit);
        }
        if (extension == ".pdf") return new(string.Empty, false);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
        if (extension == ".docx")
        {
            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            var document = ReadXml(archive.GetEntry("word/document.xml")!);
            foreach (var paragraph in document.Descendants(w + "p"))
            {
                text.AppendLine(string.Concat(paragraph.Descendants(w + "t").Select(t => t.Value)));
                if (text.Length > PreviewLimit) break;
            }
        }
        else
        {
            XNamespace a = "http://schemas.openxmlformats.org/drawingml/2006/main";
            var slides = archive.Entries.Where(e => Regex.IsMatch(e.FullName, @"\Appt/slides/slide\d{1,6}\.xml\z"))
                .OrderBy(e => int.Parse(Regex.Match(e.FullName, @"slide(\d+)\.xml").Groups[1].Value));
            var number = 0;
            foreach (var slide in slides)
            {
                text.AppendLine($"Slide {++number}");
                foreach (var paragraph in ReadXml(slide).Descendants(a + "p"))
                    text.AppendLine(string.Concat(paragraph.Descendants(a + "t").Select(t => t.Value)));
                text.AppendLine();
                if (text.Length > PreviewLimit) break;
            }
        }
        return new(text.ToString(0, Math.Min(text.Length, PreviewLimit)), text.Length > PreviewLimit);
    }
}
