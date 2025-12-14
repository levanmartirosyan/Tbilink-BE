namespace Tbilink_BE.Application.DTOs
{
    public class SearchPaginationDTO
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string Category { get; set; }
    }
}
