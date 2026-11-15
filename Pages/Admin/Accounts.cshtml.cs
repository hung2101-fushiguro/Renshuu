using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Admin;

public class AccountsModel : PageModel
{
    private readonly AccountAdminService _accounts;

    public AccountsModel(AccountAdminService accounts) => _accounts = accounts;

    public List<AccountRow> Accounts { get; private set; } = new();

    public int AdminCount => Accounts.Count(a => a.IsAdmin);

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Nhập email đăng nhập.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(256)]
    public string? NewEmail { get; set; }

    [BindProperty]
    [StringLength(120, ErrorMessage = "Tên hiển thị tối đa 120 ký tự.")]
    public string? NewDisplayName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Nhập mật khẩu tạm.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu cần 8–128 ký tự.")]
    public string? NewPassword { get; set; }

    [BindProperty]
    public bool NewIsAdmin { get; set; }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var result = await _accounts.CreateAsync(NewEmail!, NewDisplayName, NewPassword!, NewIsAdmin);
        TempData[result.Success ? "Success" : "Error"] = result.Message;

        return result.Success ? RedirectToPage() : await ReloadAsync();
    }

    private async Task<IActionResult> ReloadAsync()
    {
        await LoadAsync();
        return Page();
    }

    private async Task LoadAsync() =>
        Accounts = await _accounts.GetAccountsAsync(Search);
}
