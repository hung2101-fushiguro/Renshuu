using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using TracNghiemWeb.Data;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services.Security;

namespace TracNghiemWeb.Pages.Account;

[EnableRateLimiting("accounts")]
public class RegisterModel(AppDbContext db, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty] public RegisterInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    public class RegisterInput
    {
        [Required(ErrorMessage = "Nhập tên của bạn."), StringLength(80, ErrorMessage = "Tên tối đa 80 ký tự.")]
        public string DisplayName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Nhập email của bạn."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(254)]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Nhập mật khẩu."), StringLength(128, MinimumLength = 10, ErrorMessage = "Mật khẩu cần từ 10 đến 128 ký tự."), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Nhập lại mật khẩu."), Compare(nameof(Password), ErrorMessage = "Hai mật khẩu chưa khớp."), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (string.IsNullOrWhiteSpace(Input.DisplayName)) { ModelState.AddModelError("Input.DisplayName", "Nhập tên của bạn."); return Page(); }
        var user = new ApplicationUser { DisplayName = Input.DisplayName.Trim(), Email = Input.Email.Trim(), UserName = Input.Email.Trim() };
        await using var transaction = await db.Database.BeginTransactionAsync();
        var result = await users.CreateAsync(user, Input.Password);
        if (result.Succeeded) result = await users.AddToRoleAsync(user, AppRoles.User);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
        await transaction.CommitAsync();
        HttpContext.Session.Clear();
        await signIn.SignInAsync(user, false);
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : Url.Page("/Index")!);
    }
}
