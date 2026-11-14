using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TracNghiemWeb.Services;

namespace TracNghiemWeb.Pages.Admin;

public class AccountDetailModel : PageModel
{
    private readonly AccountAdminService _accounts;
    private readonly CurrentUser _currentUser;

    public AccountDetailModel(AccountAdminService accounts, CurrentUser currentUser)
    {
        _accounts = accounts;
        _currentUser = currentUser;
    }

    public AccountRow Account { get; private set; } = null!;
    public List<AccountContentRow> ContentRows { get; private set; } = new();

    [BindProperty]
    [Required(ErrorMessage = "Nhập mật khẩu mới.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu cần 8–128 ký tự.")]
    public string? NewPassword { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var loaded = await LoadAsync(id);
        return loaded is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostToggleAdminAsync(string id)
    {
        var account = await FindAsync(id);
        if (account is null)
        {
            return NotFound();
        }

        var result = await _accounts.SetAdminAsync(id, !account.IsAdmin);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostToggleLockAsync(string id)
    {
        var account = await FindAsync(id);
        if (account is null)
        {
            return NotFound();
        }

        var result = await _accounts.SetLockedAsync(id, !account.IsLocked, _currentUser.UserId ?? string.Empty);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(string id)
    {
        var loaded = await LoadAsync(id);
        if (loaded is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _accounts.ResetPasswordAsync(id, NewPassword!);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return result.Success ? RedirectToPage(new { id }) : Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var result = await _accounts.DeleteAsync(id, _currentUser.UserId ?? string.Empty);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToPage("/Admin/Accounts");
        }

        TempData["Error"] = result.Message;
        return RedirectToPage(new { id });
    }

    private async Task<AccountRow?> FindAsync(string id) =>
        (await _accounts.GetAccountsAsync()).FirstOrDefault(a => a.Id == id);

    private async Task<AccountRow?> LoadAsync(string id)
    {
        var account = await FindAsync(id);
        if (account is null)
        {
            return null;
        }

        Account = account;
        ContentRows = await _accounts.GetContentAsync(id);
        return account;
    }
}
