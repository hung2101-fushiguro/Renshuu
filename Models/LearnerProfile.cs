using System.ComponentModel.DataAnnotations;
namespace TracNghiemWeb.Models;
public class LearnerProfile
{
    [Key] public string UserId { get; set; } = "";
    [StringLength(100)] public string DisplayName { get; set; } = "";
    [Range(1, 500)] public int DailyGoal { get; set; } = 20;
}
public class SavedQuestion
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    public bool Favorite { get; set; }
    [StringLength(2000)] public string? Note { get; set; }
}
