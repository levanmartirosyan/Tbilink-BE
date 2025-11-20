namespace Tbilink_BE.Application.DTOs
{
    public class AuthTokensDTO
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
    }
}
