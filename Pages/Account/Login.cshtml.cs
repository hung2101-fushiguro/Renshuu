using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Pages.Account;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public LoginModel(SignInManager<ApplicationUser> signIn, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _signIn = signIn;
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// Development-only convenience: shows the seeded admin credentials under the form.
    /// Never renders outside Development and never resets the account password.
    /// </summary>
    public bool ShowTestAccount =>
        _environment.IsDevelopment()
        && _configuration.GetValue<bool>("DevelopmentAdmin:ShowLoginCredentials")
        && !string.IsNullOrWhiteSpace(TestEmail)
        && !string.IsNullOrWhiteSpace(TestPassword);

    public string? TestEmail => _configuration["DevelopmentAdmin:Email"];

    public string? TestPassword =>
        _configuration["DevelopmentAdmin:Password"] ?? _configuration["DevelopmentAdmin:LoginPassword"];

    [BindProperty]
    [Required(ErrorMessage = "Nhập email của bạn.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Nhập mật khẩu.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu cần 8–128 ký tự.")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? LocalRedirect("/") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signIn.PasswordSignInAsync(
            Email.Trim(), Password, RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Tài khoản tạm khóa sau nhiều lần đăng nhập sai. Thử lại sau 10 phút hoặc liên hệ quản trị viên."
            : "Email hoặc mật khẩu không đúng.");

        Password = string.Empty;
        ModelState.Remove(nameof(Password));
        return Page();
    }
}
