namespace Tbilink_BE.Application.DTOs
{
    public class BanUserRequestDTO
    {
        public string Reason { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
