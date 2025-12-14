namespace Tbilink_BE.Application.DTOs
{
    public class SearchRequestDTO
    {
        public string? Keyword { get; set; }
        public string Category { get; set; } = "all"; 
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
