using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Models
{
    public class UserPhoto : BaseEntity
    {
        public string Url { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
