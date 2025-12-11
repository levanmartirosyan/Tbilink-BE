namespace Tbilink_BE.Application.DTOs
{
    public class UpdateUserDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public string? Description { get; set; }

        // Privacy Settings
        public bool? IsPublicProfile { get; set; }
        public bool? ShowEmail { get; set; }
        public bool? ShowPhone { get; set; }
        public bool? AllowTagging { get; set; }

        // Notification Settings
        public bool? EmailNotifications { get; set; }
        public bool? PushNotifications { get; set; }
        public bool? SmsNotifications { get; set; }
        public bool? MarketingEmails { get; set; }

        // Language & Region
        public string? Language { get; set; }
        public string? TimeZone { get; set; }
    }
}
