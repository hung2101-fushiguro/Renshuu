using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Admin;

public class ImportModel : PageModel
{
    private const long MaxFileSize = 1024 * 1024;

    private readonly ImportService _import;

    public ImportModel(ImportService import) => _import = import;

    public ImportPreview? Preview { get; private set; }

    [BindProperty]
    public IFormFile? Upload { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        if (Upload is null || Upload.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Hãy chọn một file CSV hoặc JSON.");
            return Page();
        }

        if (Upload.Length > MaxFileSize)
        {
            ModelState.AddModelError(string.Empty, "File tối đa 1 MB.");
            return Page();
        }

        var extension = Path.GetExtension(Upload.FileName).ToLowerInvariant();
        if (extension is not (".csv" or ".json"))
        {
            ModelState.AddModelError(string.Empty, "Chỉ hỗ trợ file .csv hoặc .json.");
            return Page();
        }

        string text;
        using (var reader = new StreamReader(Upload.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            text = await reader.ReadToEndAsync();
        }

        List<ParsedQuestion> drafts;
        try
        {
            drafts = extension == ".csv"
                ? ImportService.ParseCsv(text)
                : ImportService.ParseJson(text);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            ModelState.AddModelError(string.Empty, "Không đọc được file: " + ex.Message);
            return Page();
        }

        if (drafts.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "File không có câu hỏi nào.");
            return Page();
        }

        var duplicates = await _import.FindExistingDuplicatesAsync(drafts);
        Preview = new ImportPreview(drafts, duplicates);
        TempData["ImportDrafts"] = JsonSerializer.Serialize(drafts);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (TempData["ImportDrafts"] is not string json)
        {
            TempData["Error"] = "Phiên xem trước đã hết hạn. Vui lòng chọn file lại và bấm Xem trước.";
            return RedirectToPage();
        }

        var drafts = JsonSerializer.Deserialize<List<ParsedQuestion>>(json) ?? new List<ParsedQuestion>();
        var result = await _import.SaveAsync(drafts);
        TempData["Success"] =
            $"Đã thêm {result.Added} câu hỏi, bỏ qua {result.Duplicates} câu trùng, {result.Invalid} câu lỗi.";
        return RedirectToPage("/Admin/Questions");
    }
}
