using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TracNghiemWeb.Pages;

public class ErrorModel : PageModel
{
    public int? Code { get; private set; }

    public void OnGet(int? code)
    {
        Code = code ?? 500;
    }
}
