using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Account;

/// <summary>
/// Public self-service registration. Only the four fields below are bound; there is no
/// role/admin property, so a crafted request cannot create anything but a learner.
/// Password hashing and the password policy come from the same Identity managers used
/// by the admin reset-password flow.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[EnableRateLimiting(Security.AuthFormPolicy)]
public class RegisterModel : PageModel
{
    private const string DuplicateMessage =
        "Không thể tạo tài khoản với email này. Nếu bạn đã có tài khoản, hãy đăng nhập.";

    private readonly UserManager<ApplicationUser> _users;

    public RegisterModel(UserManager<ApplicationUser> users) => _users = users;

    [BindProperty]
    [StringLength(120, ErrorMessage = "Tên hiển thị tối đa 120 ký tự.")]
    public string? DisplayName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Nhập email của bạn.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Nhập mật khẩu.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu cần 8–128 ký tự.")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Nhập lại mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? LocalRedirect("/") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Email.Trim().ToLowerInvariant();
        Email = email;
        DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? null : DisplayName.Trim();

        if (await _users.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(string.Empty, DuplicateMessage);
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = DisplayName,
        };

        var created = await _users.CreateAsync(user, Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors)
            {
                ModelState.AddModelError(string.Empty, Describe(error));
            }

            return Page();
        }

        TempData["Success"] = "Đã tạo tài khoản. Hãy đăng nhập bằng email và mật khẩu vừa đăng ký.";
        return RedirectToPage("/Account/Login");
    }

    private static string Describe(IdentityError error) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => DuplicateMessage,
        "PasswordTooShort" => "Mật khẩu cần ít nhất 8 ký tự.",
        "PasswordRequiresNonAlphanumeric" => "Mật khẩu cần ít nhất một ký tự đặc biệt.",
        "PasswordRequiresDigit" => "Mật khẩu cần ít nhất một chữ số.",
        "PasswordRequiresLower" => "Mật khẩu cần ít nhất một chữ thường.",
        "PasswordRequiresUpper" => "Mật khẩu cần ít nhất một chữ hoa.",
        _ => "Không thể tạo tài khoản. Kiểm tra lại thông tin và thử lại.",
    };
}
