using Tbilink_BE.Models;

namespace Tbilink_BE.Application.DTOs
{
    public class CreatePostDTO
    {
        public int UserId { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }
}
