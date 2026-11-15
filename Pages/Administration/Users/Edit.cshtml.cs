using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Models;
using TracNghiemWeb.Services.Security;

namespace TracNghiemWeb.Pages.Administration.Users;

public class EditModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, UserAdministrationService administration, ICurrentUser current) : PageModel
{
    [BindProperty] public UserInput Input { get; set; } = new();
    public string? AccountId { get; private set; }
    public bool IsNew => AccountId is null;
    public bool IsSelf => AccountId == current.Id;
    public class UserInput
    {
        [Required(ErrorMessage = "Nhập tên hiển thị."), StringLength(80)] public string DisplayName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(254)] public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = AppRoles.User;
        public bool IsActive { get; set; } = true;
        [DataType(DataType.Password), StringLength(128, MinimumLength = 10, ErrorMessage = "Mật khẩu cần từ 10 đến 128 ký tự.")] public string? Password { get; set; }
        public string? Version { get; set; }
    }
    public async Task<IActionResult> OnGetAsync(string? id)
    {
        AccountId = id;
        if (IsNew) return Page();
        var user = await users.FindByIdAsync(id!);
        if (user is null || user.IsDeleted) return NotFound();
        Input = new() { DisplayName = user.DisplayName, Email = user.Email!, Role = await users.IsInRoleAsync(user, AppRoles.Admin) ? AppRoles.Admin : AppRoles.User, IsActive = user.IsActive, Version = user.ConcurrencyStamp };
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(string? id)
    {
        AccountId = id;
        if (IsNew && string.IsNullOrEmpty(Input.Password)) ModelState.AddModelError("Input.Password", "Nhập mật khẩu cho tài khoản mới.");
        if (!ModelState.IsValid) return Page();
        var input = new AccountEdit(Input.DisplayName, Input.Email, Input.Role, Input.IsActive, Input.Password, Input.Version);
        var result = IsNew ? await administration.CreateAsync(input) : await administration.UpdateAsync(id!, input);
        if (!result.Success) { ModelState.AddModelError(string.Empty, result.Error!); return Page(); }
        if (IsSelf && await users.FindByIdAsync(id!) is { } self) await signIn.RefreshSignInAsync(self);
        TempData["Success"] = IsNew ? "Đã tạo tài khoản." : "Đã cập nhật tài khoản.";
        return RedirectToPage("Index");
    }
    public async Task<IActionResult> OnPostDeleteAsync(string id, string? version)
    {
        var result = await administration.DeleteAsync(id, version);
        if (!result.Success) { TempData["Error"] = result.Error; return RedirectToPage(new { id }); }
        TempData["Success"] = "Đã xóa tài khoản. Tài liệu và dữ liệu học được giữ lại.";
        return RedirectToPage("Index");
    }
}
