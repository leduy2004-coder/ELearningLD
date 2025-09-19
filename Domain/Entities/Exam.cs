using Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("Exams")]
public class Exam : IDateTracking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ExamId { get; set; }

    [Required]
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Duration { get; set; } // phút

    [Required]
    public string? CreatedBy { get; set; }
    public User? Creator { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ExamQuestion>? ExamQuestions { get; set; }
    public ICollection<ExamAttempt>? ExamAttempts { get; set; }
}
