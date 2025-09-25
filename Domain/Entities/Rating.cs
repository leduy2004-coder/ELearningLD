using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Ratings")]
    public class Rating : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RatingId { get; set; }

        [Required]
        [Range(1, 5)]  // Đánh giá từ 1-5 sao
        public int Score { get; set; }

        [MaxLength(1000)]
        public string? Review { get; set; }  // Nội dung đánh giá (tùy chọn)

        [Required]
        public string? UserId { get; set; }
        public User? User { get; set; }  // Người đánh giá

        // Polymorphic: Liên kết với target (Exam, Question, LessonVideo)
        public int? TargetId { get; set; }  // ID của target
        [MaxLength(50)]
        public string TargetType { get; set; } = null!;  // Loại target: "Exam", "Question", "LessonVideo"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
