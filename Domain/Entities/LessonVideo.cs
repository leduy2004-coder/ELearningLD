using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("LessonVideos")]
    public class LessonVideo : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VideoId { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MaxLength(500)]
        public string VideoUrl { get; set; } = null!;  // Link video (ví dụ: YouTube, Vimeo, hoặc lưu trữ nội bộ)

        public bool IsPublic { get; set; } = true;  // Public hoặc private (nếu private, chỉ creator hoặc người theo dõi xem được)

        [Required]
        public string? CreatedBy { get; set; }
        public User? Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ bình luận và đánh giá
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}
