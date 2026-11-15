using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Account;

[EnableRateLimiting("accounts")]
public class LoginModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    public class LoginInput
    {
        [Required(ErrorMessage = "Nhập email của bạn."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(254)]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Nhập mật khẩu."), StringLength(128), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? LocalRedirect(SafeReturn()) : Page();
    private string SafeReturn() => Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : Url.Page("/Index")!;
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await users.FindByEmailAsync(Input.Email.Trim());
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu chưa đúng, hoặc tài khoản đã ngừng hoạt động.");
            return Page();
        }
        var result = await signIn.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Bạn đã thử nhiều lần. Vui lòng đăng nhập lại sau 15 phút." : "Email hoặc mật khẩu chưa đúng, hoặc tài khoản đã ngừng hoạt động.");
            return Page();
        }
        HttpContext.Session.Clear();
        return LocalRedirect(SafeReturn());
    }
}
