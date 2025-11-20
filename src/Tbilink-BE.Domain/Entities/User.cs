using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Models
{
    public class User : BaseEntity
    {
        // Basic Info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public bool IsEmailVerified { get; set; } = false;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpires { get; set; }

        // Auth
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Role { get; set; }

        // Profile
        public string? ProfilePhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public string? Description { get; set; }
        public List<UserPhoto> Photos { get; set; } = new List<UserPhoto>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Post> LikedPosts { get; set; } = new List<Post>();

        // Stats
        public int PostCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

        // Privacy Settings
        public bool IsPublicProfile { get; set; } = true;
        public bool ShowEmail { get; set; } = false;
        public bool ShowPhone { get; set; } = false;
        public bool AllowTagging { get; set; } = true;

        // Notifications
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool MarketingEmails { get; set; } = false;

        // Language & Region
        public string Language { get; set; } = "en";
        public string TimeZone { get; set; } = "UTC";

        // Online/Activity
        public bool IsOnline { get; set; } = false;
        public DateTime? LastActive { get; set; }
    }

}
