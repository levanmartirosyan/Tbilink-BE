namespace Tbilink_BE.Application.DTOs
{
    public class AdminUserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsRegistrationCompleted { get; set; }
        public string Role { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public string? Description { get; set; }
        public bool IsPublicProfile { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowPhone { get; set; }
        public bool AllowTagging { get; set; }
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool MarketingEmails { get; set; }
        public string Language { get; set; }
        public string TimeZone { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastActive { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostCount { get; set; }
        public bool IsBanned { get; set; }
    }
}
