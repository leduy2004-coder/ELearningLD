using Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("ExamAttempts")]
public class ExamAttempt : IDateTracking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AttemptId { get; set; }

    [Required]
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }

    [Required]
    public string? UserId { get; set; }
    public User? Student { get; set; }

    public DateTime StartTime { get; set; } // Thời gian bắt đầu làm bài
    public DateTime EndTime { get; set; }   // Thời gian kết thúc (StartTime + Duration)

    public double Score { get; set; } // Điểm số

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AttemptAnswer>? AttemptAnswers { get; set; }
}