namespace TracNghiemWeb.Models;

public class Attempt
{
    public int Id { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectCount { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();

    public bool IsSubmitted => SubmittedAt.HasValue;

    public TimeSpan Duration => (SubmittedAt ?? DateTime.Now) - StartedAt;

    public double Percentage => TotalQuestions == 0 ? 0 : CorrectCount * 100.0 / TotalQuestions;
}
