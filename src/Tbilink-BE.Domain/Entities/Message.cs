using System.Text.Json.Serialization;
using Tbilink_BE.Models;

namespace Tbilink_BE.Domain.Entities
{
    public class Message : BaseEntity
    {
        public int SenderId { get; set; }

        [JsonIgnore]
        public User Sender { get; set; }
        public int RecipientId { get; set; }

        [JsonIgnore]
        public User Recipient { get; set; }
        public string Content { get; set; }
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
        public DateTime? DateRead { get; set; }
        public bool SenderDeleted { get; set; } = false;
        public bool RecipientDeleted { get; set; } = false;

        public string? GroupName { get; set; }

        [JsonIgnore]
        public Group? Group { get; set; }
    }
}
