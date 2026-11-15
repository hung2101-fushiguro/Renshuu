using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TracNghiemWeb.Pages.Admin;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Admin/Accounts");
}
