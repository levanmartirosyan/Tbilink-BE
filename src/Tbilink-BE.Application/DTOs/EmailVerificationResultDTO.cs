namespace Tbilink_BE.Application.DTOs
{
    public class EmailVerificationResultDTO
    {
        public string? PasswordResetToken { get; set; }
        public string? Email { get; set; } = default;
    }
}
