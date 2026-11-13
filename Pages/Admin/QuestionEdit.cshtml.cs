using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Admin;

public class QuestionEditModel : PageModel
{
    private readonly AppDbContext _db;

    private readonly QuestionAnalysisService _analysis;
    public QuestionEditModel(AppDbContext db, QuestionAnalysisService analysis) { _db = db; _analysis = analysis; }

    public async Task<IActionResult> OnPostAnalyzeAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return new JsonResult(new { error = "Hãy nhập đầy đủ câu hỏi và bốn đáp án hợp lệ trước khi phân tích." }) { StatusCode = 400 };
        var subject = await _db.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == Input.SubjectId, cancellationToken);
        if (subject is null) return BadRequest();
        // Read passage from the saved question, never trust a posted PassageId.
        var passage = Input.Id == 0 ? null : await _db.Questions.Where(q => q.Id == Input.Id)
            .Select(q => q.Passage == null ? null : q.Passage.Content).FirstOrDefaultAsync(cancellationToken);
        try { return new JsonResult(await _analysis.AnalyzeAsync(Input, subject.Name, passage, cancellationToken)); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or System.Text.Json.JsonException or KeyNotFoundException)
        {
            return new JsonResult(new { error = "Chưa phân tích được. Kiểm tra Ollama đang chạy và đã tải model; bạn vẫn có thể phân loại thủ công." }) { StatusCode = 503 };
        }
    }

    public List<Subject> Subjects { get; private set; } = new();
    public bool IsNew { get; private set; }

    [BindProperty]
    public Question Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id, int? subjectId)
    {
        Subjects = await _db.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        if (Subjects.Count == 0)
        {
            TempData["Error"] = "Hãy tạo ít nhất một môn học trước.";
            return RedirectToPage("/Admin/Index");
        }

        if (id is int questionId)
        {
            var question = await _db.Questions.AsNoTracking().FirstOrDefaultAsync(q => q.Id == questionId);
            if (question is null)
            {
                return NotFound();
            }

            Input = question;
            IsNew = false;
        }
        else
        {
            Input = new Question
            {
                SubjectId = subjectId is int sid && Subjects.Any(s => s.Id == sid) ? sid : Subjects[0].Id,
            };
            IsNew = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Subjects = await _db.Subjects.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        IsNew = Input.Id == 0;

        if (!Subjects.Any(s => s.Id == Input.SubjectId))
        {
            ModelState.AddModelError("Input.SubjectId", "Môn học không hợp lệ.");
        }

        if (ModelState.IsValid)
        {
            var content = Input.Content.Trim();
            var isDuplicate = await _db.Questions.AnyAsync(q =>
                q.SubjectId == Input.SubjectId && q.Content == content && q.Id != Input.Id);
            if (isDuplicate)
            {
                ModelState.AddModelError("Input.Content", "Đã có câu hỏi trùng nội dung trong môn này.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (IsNew)
        {
            _db.Questions.Add(new Question
            {
                Chapter = Input.Chapter?.Trim(),
                Topic = Input.Topic?.Trim(),
                Difficulty = Input.Difficulty,
                SubjectId = Input.SubjectId,
                Content = Input.Content.Trim(),
                OptionA = Input.OptionA.Trim(),
                OptionB = Input.OptionB.Trim(),
                OptionC = Input.OptionC.Trim(),
                OptionD = Input.OptionD.Trim(),
                CorrectOption = Input.CorrectOption.Trim().ToUpperInvariant(),
                OptionStyle = Input.OptionStyle,
                Explanation = string.IsNullOrWhiteSpace(Input.Explanation) ? null : Input.Explanation.Trim(),
            });
            TempData["Success"] = "Đã thêm câu hỏi.";
        }
        else
        {
            var question = await _db.Questions.FindAsync(Input.Id);
            if (question is null)
            {
                return NotFound();
            }

            question.Chapter = Input.Chapter?.Trim();
            question.Topic = Input.Topic?.Trim();
            question.Difficulty = Input.Difficulty;
            question.SubjectId = Input.SubjectId;
            question.Content = Input.Content.Trim();
            question.OptionA = Input.OptionA.Trim();
            question.OptionB = Input.OptionB.Trim();
            question.OptionC = Input.OptionC.Trim();
            question.OptionD = Input.OptionD.Trim();
            question.CorrectOption = Input.CorrectOption.Trim().ToUpperInvariant();
            question.OptionStyle = Input.OptionStyle;
            question.Explanation = string.IsNullOrWhiteSpace(Input.Explanation) ? null : Input.Explanation.Trim();
            TempData["Success"] = "Đã cập nhật câu hỏi.";
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/Questions", new { subjectId = Input.SubjectId });
    }
}
