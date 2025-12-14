namespace Tbilink_BE.Application.DTOs
{
    public class FollowStatsDTO
    {
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; } 
    }
}
