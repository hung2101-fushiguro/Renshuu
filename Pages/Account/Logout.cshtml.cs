using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace TracNghiemWeb.Pages.Account;
public class LogoutModel(SignInManager<IdentityUser> signIn) : PageModel {
 public IActionResult OnGet() => RedirectToPage("/Index");
 public async Task<IActionResult> OnPostAsync() { await signIn.SignOutAsync(); return RedirectToPage("/Account/Login"); }
}