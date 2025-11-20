using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Models
{
    public class Comment : BaseEntity
    {
        public int PostId { get; set; }
        public Post Post { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
