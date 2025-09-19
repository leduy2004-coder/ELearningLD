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

        // Quan hệ
        public ICollection<Exam>? ExamsCreated { get; set; }
        public ICollection<Question>? QuestionsCreated { get; set; }
        public ICollection<ExamAttempt>? ExamAttempts { get; set; }
    }
}
