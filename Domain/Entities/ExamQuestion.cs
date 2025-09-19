using System.ComponentModel.DataAnnotations.Schema;

[Table("ExamQuestions")]
public class ExamQuestion
{
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }

    public int QuestionId { get; set; }
    public Question? Question { get; set; }
}