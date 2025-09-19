using Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("Questions")]
public class Question : IDateTracking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int QuestionId { get; set; }

    [Required]
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Level { get; set; } = null!; // easy, medium, hard

    [Required]
    public string? CreatedBy { get; set; }
    public User? Creator { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Answer>? Answers { get; set; }
}