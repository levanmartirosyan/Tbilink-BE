using Tbilink_BE.Models;

namespace Tbilink_BE.Domain.Entities
{
    public class PostLike : BaseEntity
    {
        public int PostId { get; set; }
        public Post Post { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    }
}
