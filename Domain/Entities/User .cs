using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Users")]
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(150)]
        public string? FullName { get; set; }

        public string? Gender { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ hiện tại
        public ICollection<Exam>? ExamsCreated { get; set; }
        public ICollection<Question>? QuestionsCreated { get; set; }
        public ICollection<ExamAttempt>? ExamAttempts { get; set; }

        // Thêm mới: Quan hệ theo dõi (Followers và Following)
        public ICollection<UserFollow>? Followers { get; set; }  // Những người theo dõi user này
        public ICollection<UserFollow>? Following { get; set; }  // Những user mà người này theo dõi

        // Quan hệ mới cho video bài học
        public ICollection<LessonVideo>? LessonVideosCreated { get; set; }

        // Quan hệ cho bình luận và đánh giá (nếu cần, nhưng sẽ xử lý ở entity Comment/Rating)
    }
}
