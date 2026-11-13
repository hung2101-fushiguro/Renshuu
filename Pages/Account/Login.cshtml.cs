using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace TracNghiemWeb.Pages.Account;
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class LoginModel(SignInManager<IdentityUser> signIn, UserManager<IdentityUser> users,
    IConfiguration configuration, IWebHostEnvironment environment) : PageModel
{
    public bool ShowTestAccount => environment.IsDevelopment()
        && configuration.GetValue<bool>("DevelopmentAdmin:ShowLoginCredentials")
        && !Register
        && !string.IsNullOrWhiteSpace(configuration["DevelopmentAdmin:Email"])
        && !string.IsNullOrWhiteSpace(configuration["DevelopmentAdmin:LoginPassword"]);
    public string? TestEmail => ShowTestAccount ? configuration["DevelopmentAdmin:Email"] : null;
    public string? TestPassword => ShowTestAccount ? configuration["DevelopmentAdmin:LoginPassword"] : null;

    [BindProperty, Required(ErrorMessage="Nhập email của bạn."), EmailAddress(ErrorMessage="Email không hợp lệ.")] public string Email { get; set; } = "";
    [BindProperty, Required(ErrorMessage="Nhập mật khẩu."), StringLength(128, MinimumLength=8, ErrorMessage="Mật khẩu cần 8–128 ký tự.")] public string Password { get; set; } = "";
    [BindProperty] public bool RememberMe { get; set; }
    [BindProperty(SupportsGet=true)] public string? ReturnUrl { get; set; }
    public bool Register => Request.Query["mode"] == "register";
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? LocalRedirect("/") : Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (Register)
        {
            var user = new IdentityUser { UserName=Email.Trim(), Email=Email.Trim() };
            var result = await users.CreateAsync(user, Password);
            if (result.Succeeded) { await signIn.SignInAsync(user, false); return LocalRedirect("/"); }
            ModelState.AddModelError("", "Không thể tạo tài khoản. Hãy kiểm tra email và dùng mật khẩu có chữ hoa, chữ thường, số, ký tự đặc biệt.");
        }
        else
        {
            var result = await signIn.PasswordSignInAsync(Email.Trim(), Password, RememberMe, lockoutOnFailure: true);
            if (result.Succeeded) return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
            ModelState.AddModelError("", "Không thể đăng nhập. Kiểm tra thông tin hoặc thử lại sau.");
        }
        Password = ""; ModelState.Remove(nameof(Password)); return Page();
    }
}
