using Tbilink_BE.Domain.Constants;

namespace Tbilink_BE.Application.DTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public int? ActorId { get; set; }
        public string ActorName { get; set; }
        public string? ActorAvatar { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
