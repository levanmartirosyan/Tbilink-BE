namespace Tbilink_BE.Application.DTOs
{
    public class CreateCommentDTO
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
