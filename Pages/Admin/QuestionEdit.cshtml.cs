using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Admin;

public class QuestionEditModel : PageModel
{
    private readonly AppDbContext _db;

    public QuestionEditModel(AppDbContext db) => _db = db;

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
                SubjectId = Input.SubjectId,
                Content = Input.Content.Trim(),
                OptionA = Input.OptionA.Trim(),
                OptionB = Input.OptionB.Trim(),
                OptionC = Input.OptionC.Trim(),
                OptionD = Input.OptionD.Trim(),
                CorrectOption = Input.CorrectOption.Trim().ToUpperInvariant(),
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

            question.SubjectId = Input.SubjectId;
            question.Content = Input.Content.Trim();
            question.OptionA = Input.OptionA.Trim();
            question.OptionB = Input.OptionB.Trim();
            question.OptionC = Input.OptionC.Trim();
            question.OptionD = Input.OptionD.Trim();
            question.CorrectOption = Input.CorrectOption.Trim().ToUpperInvariant();
            question.Explanation = string.IsNullOrWhiteSpace(Input.Explanation) ? null : Input.Explanation.Trim();
            TempData["Success"] = "Đã cập nhật câu hỏi.";
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/Questions", new { subjectId = Input.SubjectId });
    }
}
