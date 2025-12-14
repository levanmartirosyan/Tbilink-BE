using Tbilink_BE.Models;

namespace Tbilink_BE.Domain.Entities
{
    public class UserFollow : BaseEntity
    {
        public int FollowerId { get; set; }
        public User Follower { get; set; }

        public int FollowedId { get; set; }
        public User Followed { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
