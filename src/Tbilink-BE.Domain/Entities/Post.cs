using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Models
{
    public class Post : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public string Content { get; set; }   
        public string? ImageUrl { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;

        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<PostLike> Likes { get; set; } = new List<PostLike>(); 


    }

}
