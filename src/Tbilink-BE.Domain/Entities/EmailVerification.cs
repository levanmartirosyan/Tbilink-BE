using Tbilink_BE.Domain.Constants;
using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Models
{
    public class EmailVerification : BaseEntity
    {
        public required int UserId { get; set; }
        public required string CodeHash { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public required CodeType CodeType { get; set; }
        public required bool IsVerified { get; set; }
        public User? User { get; set; }
    }
}
