namespace Tbilink_BE.Application.DTOs
{
    public class SearchResultDTO
    {
        public List<UserSearchResultDTO> Users { get; set; } = new List<UserSearchResultDTO>();
        public List<PostSearchResultDTO> Posts { get; set; } = new List<PostSearchResultDTO>();
        public SearchPaginationDTO Pagination { get; set; } = new SearchPaginationDTO();
    }
}
