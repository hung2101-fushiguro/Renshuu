using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace TracNghiemWeb.Pages;

[IgnoreAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ErrorModel : PageModel
{
    public int? Code { get; private set; }
    public string Title => Code switch { 401 => "Phiên đăng nhập đã hết", 403 => "Không có quyền truy cập", 404 => "Không tìm thấy nội dung", 413 => "Tệp quá lớn", 429 => "Bạn thao tác quá nhanh", _ => "Có lỗi xảy ra" };
    public string Message => Code switch { 401 => "Đăng nhập lại để tiếp tục.", 403 => "Tài khoản hiện tại không có quyền thực hiện thao tác này.", 404 => "Nội dung không tồn tại hoặc bạn không có quyền mở nội dung này.", 413 => "Mỗi tài liệu tối đa 20 MB. Vui lòng chọn tệp nhỏ hơn.", 429 => "Chờ một lát rồi thử lại.", _ => "Vui lòng thử lại sau." };

    public void OnGet(int? code)
    {
        Code = code is >= 400 and <= 599 ? code : 500;
        Response.StatusCode = Code.Value;
    }
    public void OnPost(int? code) => OnGet(code);
}
