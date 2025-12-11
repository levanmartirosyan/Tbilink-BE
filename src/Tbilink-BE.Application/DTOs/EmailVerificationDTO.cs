using Tbilink_BE.Domain.Constants;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.DTOs
{
    public class SendVerificationCodeDTO
    {
        public required string Email { get; set; }
        public CodeType CodeType { get; set; }
        public int? CurrentUserId { get; set; }
    }

    public class VerifyEmailDTO
    {
        public required string Email { get; set; }
        public required string Code { get; set; }
    }
}
