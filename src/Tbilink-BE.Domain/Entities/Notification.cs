using Tbilink_BE.Domain.Constants;
using Tbilink_BE.Models;

namespace Tbilink_BE.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int RecipientId { get; set; }
        public User Recipient { get; set; }
        public int? ActorId { get; set; }
        public User Actor { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
