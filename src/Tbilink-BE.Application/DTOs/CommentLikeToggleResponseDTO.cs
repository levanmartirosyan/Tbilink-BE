namespace Tbilink_BE.Application.DTOs
{
    public class CommentLikeToggleResponseDTO
    {
        public bool IsLikedByCurrentUser { get; set; }
        public int LikeCount { get; set; }
    }
}
