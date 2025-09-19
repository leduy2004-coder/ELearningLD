using Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AttemptAnswers")]
public class AttemptAnswer : IDateTracking
{
    public int AttemptId { get; set; }
    public ExamAttempt? Attempt { get; set; }

    public int QuestionId { get; set; }
    public Question? Question { get; set; }

    public int? AnswerId { get; set; }
    public Answer? Answer { get; set; }

    public bool? IsCorrect { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}