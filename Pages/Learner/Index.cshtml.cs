using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services;
using TracNghiemWeb.Data;
namespace TracNghiemWeb.Pages.Learner;
public class IndexModel(LearnerService learner, UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, AppDbContext db) : PageModel
{
    [BindProperty, StringLength(100)] public string? DisplayName { get; set; }
    [BindProperty, Range(1,500)] public int DailyGoal { get; set; } = 20;
    public List<SavedQuestion> Saved { get; set; } = new();
    public List<Attempt> Pending { get; set; } = new();
    public List<Subject> Subjects { get; set; } = new();
    public int Today { get; set; }
    public async Task OnGetAsync(int? subjectId, string? search)
    {
        var profile = await learner.ProfileAsync(); DisplayName = profile.DisplayName; DailyGoal = profile.DailyGoal;
        Saved = await learner.SavedAsync(subjectId, search); Pending = await learner.PendingAsync(); Today = await learner.TodayAsync();
        Subjects = await db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
    }
    public async Task<IActionResult> OnPostProfileAsync()
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Tên tối đa 100 ký tự; mục tiêu từ 1 đến 500 câu."; return RedirectToPage(); }
        await learner.SaveProfileAsync(DisplayName ?? "", DailyGoal); TempData["Success"] = "Đã lưu hồ sơ và mục tiêu."; return RedirectToPage();
    }
    public async Task<IActionResult> OnPostSaveAsync(int questionId, bool favorite, string? note)
    {
        TempData[await learner.SaveQuestionAsync(questionId, favorite, note) ? "Success" : "Error"] = "Đã xử lý yêu cầu lưu. Ghi chú tối đa 2000 ký tự.";
        return RedirectToPage();
    }
    public async Task<IActionResult> OnPostPasswordAsync(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
        { TempData["Error"] = "Nhập đủ mật khẩu và xác nhận trùng khớp."; return RedirectToPage(); }
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        var result = await users.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded) { await signIn.RefreshSignInAsync(user); TempData["Success"] = "Đã đổi mật khẩu."; }
        else TempData["Error"] = "Mật khẩu hiện tại chưa đúng hoặc mật khẩu mới chưa đủ mạnh (8 ký tự, chữ hoa, chữ thường, số, ký tự đặc biệt).";
        return RedirectToPage();
    }
}
