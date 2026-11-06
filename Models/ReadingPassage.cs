using System.ComponentModel.DataAnnotations;

namespace TracNghiemWeb.Models;

public class ReadingPassage
{
    public int Id { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    [StringLength(300, ErrorMessage = "Tiêu đề bài đọc tối đa 300 ký tự.")]
    [Display(Name = "Tiêu đề bài đọc")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bài đọc.")]
    [StringLength(20000, ErrorMessage = "Nội dung bài đọc tối đa 20000 ký tự.")]
    [Display(Name = "Nội dung bài đọc")]
    public string Content { get; set; } = string.Empty;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
