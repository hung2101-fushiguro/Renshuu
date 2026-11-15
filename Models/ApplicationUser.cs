using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TracNghiemWeb.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(120, ErrorMessage = "Tên hiển thị tối đa 120 ký tự.")]
    [Display(Name = "Tên hiển thị")]
    public string? DisplayName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<SubjectFolder> Folders { get; set; } = new List<SubjectFolder>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public string NameOrEmail => string.IsNullOrWhiteSpace(DisplayName) ? (Email ?? UserName ?? "Người dùng") : DisplayName!;
}
