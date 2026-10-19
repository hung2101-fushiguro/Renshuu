using System.ComponentModel.DataAnnotations;

namespace TracNghiemWeb.Models;

public class Question
{
    public const string CorrectOptionPattern = "^[ABCD]$";

    public int Id { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi.")]
    [StringLength(2000, ErrorMessage = "Nội dung câu hỏi tối đa 2000 ký tự.")]
    [Display(Name = "Nội dung câu hỏi")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập đáp án A.")]
    [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự.")]
    [Display(Name = "Đáp án A")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập đáp án B.")]
    [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự.")]
    [Display(Name = "Đáp án B")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập đáp án C.")]
    [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự.")]
    [Display(Name = "Đáp án C")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập đáp án D.")]
    [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự.")]
    [Display(Name = "Đáp án D")]
    public string OptionD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn đáp án đúng.")]
    [RegularExpression(CorrectOptionPattern, ErrorMessage = "Đáp án đúng phải là A, B, C hoặc D.")]
    [Display(Name = "Đáp án đúng")]
    public string CorrectOption { get; set; } = "A";

    [StringLength(2000, ErrorMessage = "Giải thích tối đa 2000 ký tự.")]
    [Display(Name = "Giải thích")]
    public string? Explanation { get; set; }

    public ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();

    public string GetOption(string option) => option switch
    {
        "A" => OptionA,
        "B" => OptionB,
        "C" => OptionC,
        "D" => OptionD,
        _ => string.Empty
    };
}
