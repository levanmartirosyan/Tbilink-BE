using Tbilink_BE.Domain.Constants;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.DTOs
{
    public class RequestVerificationDTO
    {
        public int UserId { get; set; }
        public required string CodeHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public CodeType CodeType { get; set; }
        public bool IsVerified { get; set; }
        public User? User { get; set; }
    }

    public class VerifyEmailDTO
    {
        public required string Email { get; set; }
        public required string Code { get; set; }
    }
}
