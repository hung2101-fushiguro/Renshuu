using System.ComponentModel.DataAnnotations;

namespace TracNghiemWeb.Pages.Documents;

public class DocumentForm
{
    [Required(ErrorMessage = "Nhập tên tài liệu."), StringLength(200, ErrorMessage = "Tên tối đa 200 ký tự.")]
    public string Title { get; set; } = string.Empty;
    [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1.000 ký tự.")]
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public string? Version { get; set; }
    [StringLength(100000, ErrorMessage = "Nội dung sửa trực tiếp tối đa 100.000 ký tự.")]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? TextContent { get; set; }
}
