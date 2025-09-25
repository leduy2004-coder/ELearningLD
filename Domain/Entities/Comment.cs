using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Comments")]
    public class Comment : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }

        [Required, MaxLength(2000)]
        public string Content { get; set; } = null!;

        [Required]
        public string? UserId { get; set; }
        public User? User { get; set; }  // Người bình luận

        // Polymorphic: Liên kết với target (Exam, Question, LessonVideo)
        public int? TargetId { get; set; }  // ID của target
        [MaxLength(50)]
        public string TargetType { get; set; } = null!;  // Loại target: "Exam", "Question", "LessonVideo"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
