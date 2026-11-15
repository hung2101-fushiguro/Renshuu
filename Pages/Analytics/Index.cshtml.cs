using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TracNghiemWeb.Pages.Analytics;

public class IndexModel : PageModel
{
    public IActionResult OnGet(int? subjectId) =>
        subjectId.HasValue
            ? RedirectToPage("/History/Analytics", new { subjectId = subjectId.Value })
            : RedirectToPage("/History/Analytics");
}
