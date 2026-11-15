using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Account;

public class ProfileModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty] public ProfileInput Input { get; set; } = new();
    public string Email { get; private set; } = string.Empty;
    public class ProfileInput
    {
        [Required(ErrorMessage = "Nhập tên của bạn."), StringLength(80)] public string DisplayName { get; set; } = string.Empty;
        [DataType(DataType.Password), StringLength(128)] public string? CurrentPassword { get; set; }
        [DataType(DataType.Password), StringLength(128, MinimumLength = 10, ErrorMessage = "Mật khẩu cần từ 10 đến 128 ký tự.")] public string? NewPassword { get; set; }
        [DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Hai mật khẩu mới chưa khớp.")] public string? ConfirmPassword { get; set; }
    }
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        Email = user.Email!;
        Input.DisplayName = user.DisplayName;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        Email = user.Email!;
        if (string.IsNullOrWhiteSpace(Input.DisplayName)) ModelState.AddModelError("Input.DisplayName", "Nhập tên của bạn.");
        if (!string.IsNullOrEmpty(Input.NewPassword) && string.IsNullOrEmpty(Input.CurrentPassword)) ModelState.AddModelError("Input.CurrentPassword", "Nhập mật khẩu hiện tại để đổi mật khẩu.");
        if (!ModelState.IsValid) return Page();
        user.DisplayName = Input.DisplayName.Trim();
        var result = string.IsNullOrEmpty(Input.NewPassword) ? await users.UpdateAsync(user) : await users.ChangePasswordAsync(user, Input.CurrentPassword!, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
        await signIn.RefreshSignInAsync(user);
        TempData["Success"] = "Đã cập nhật tài khoản.";
        return RedirectToPage();
    }
}
