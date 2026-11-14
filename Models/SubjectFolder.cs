using System.ComponentModel.DataAnnotations;

namespace TracNghiemWeb.Models;

public class SubjectFolder
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên thư mục.")]
    [StringLength(200, ErrorMessage = "Tên thư mục tối đa 200 ký tự.")]
    [Display(Name = "Tên thư mục")]
    public string Name { get; set; } = string.Empty;

    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
