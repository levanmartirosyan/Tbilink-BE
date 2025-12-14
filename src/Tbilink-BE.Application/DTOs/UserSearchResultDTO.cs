namespace Tbilink_BE.Application.DTOs
{
    public class UserSearchResultDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public int FollowersCount { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
    }
}
