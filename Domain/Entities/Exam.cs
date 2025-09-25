using Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

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

    public int Duration { get; set; } // phút, thời gian làm bài

    [Required]
    public string? CreatedBy { get; set; }
    public User? Creator { get; set; }

    public bool IsPublic { get; set; } = true; // Public hay cần mã mời
    [MaxLength(50)]
    public string? InvitationCode { get; set; } // Mã mời (tùy chọn)

    public DateTime? OpenTime { get; set; } // Thời gian bắt đầu cho phép truy cập
    public DateTime? CloseTime { get; set; } // Thời gian kết thúc truy cập

    // Thêm mới: Số lần làm bài tối đa
    public int? MaxAttempts { get; set; } // Số lần tối đa một người dùng được làm (null = không giới hạn)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ExamQuestion>? ExamQuestions { get; set; }
    public ICollection<ExamAttempt>? ExamAttempts { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Rating>? Ratings { get; set; }
}