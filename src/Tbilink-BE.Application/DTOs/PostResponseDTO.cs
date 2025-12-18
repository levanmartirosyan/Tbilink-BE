namespace Tbilink_BE.Application.DTOs
{
    public class PostResponseDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public int LikeCount { get; set; }
        public int CommentCount { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Avatar { get; set; }

        public bool IsLikedByCurrentUser { get; set; }
    }
}
