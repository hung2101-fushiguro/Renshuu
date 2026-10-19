namespace TracNghiemWeb.Models;

public class AttemptAnswer
{
    public int Id { get; set; }

    public int AttemptId { get; set; }
    public Attempt? Attempt { get; set; }

    public int QuestionId { get; set; }
    public Question? Question { get; set; }

    public string? SelectedOption { get; set; }

    public bool IsCorrect { get; set; }
}
