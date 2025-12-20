namespace Tbilink_BE.Application.DTOs
{
    public class UserBanDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public DateTime BannedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? BannedById { get; set; }
        public string? BannedByUserName { get; set; }
        public bool IsActive { get; set; }
    }
}
