namespace Tbilink_BE.Application.DTOs
{
    public class UserFollowDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime? LastActive { get; set; }
        public DateTime FollowedAt { get; set; }
        public bool IsFollowingBack { get; set; }
    }
}
