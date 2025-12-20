using Tbilink_BE.Models;

namespace Tbilink_BE.Domain.Entities
{
    public class UserBan : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string Reason { get; set; }
        public DateTime BannedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; } 
        public int? BannedById { get; set; } 
        public User? BannedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
