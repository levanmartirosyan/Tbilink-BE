namespace Tbilink_BE.Application.DTOs
{
    public class PostLikeToggleResponseDTO
    {
        public bool IsLikedByCurrentUser { get; set; }
        public int LikeCount { get; set; }
    }
}
