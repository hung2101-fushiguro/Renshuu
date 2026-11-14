using System.ComponentModel.DataAnnotations;

namespace TracNghiemWeb.Models;

public class Subject
{
    public int Id { get; set; }

    public int? FolderId { get; set; }
    public SubjectFolder? Folder { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên môn học.")]
    [StringLength(200, ErrorMessage = "Tên môn học tối đa 200 ký tự.")]
    [Display(Name = "Tên môn học")]
    public string Name { get; set; } = string.Empty;

    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ReadingPassage> ReadingPassages { get; set; } = new List<ReadingPassage>();
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
}
