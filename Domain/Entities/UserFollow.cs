using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("UserFollows")]
    public class UserFollow
    {
        public string? FollowerId { get; set; }  // ID người theo dõi
        public User? Follower { get; set; }

        public string? FollowedId { get; set; }  // ID giáo viên/user được theo dõi
        public User? Followed { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
