using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IPostService
    {
        public Task<ServiceResponse<List<PostWithUserDTO>>> GetAllPosts(int? currentUserId);
        public Task<ServiceResponse<PaginatedResponse<PostWithUserDTO>>> GetAllPostsPaginated(int? currentUserId, int pageNumber = 1, int pageSize = 10);
        public Task<ServiceResponse<List<PostWithUserDTO>>> GetPostsByUserId(int userId, int? currentUserId);
        public Task<ServiceResponse<Post?>> GetPostById(int postId);
        public Task<ServiceResponse<PostResponseDTO>> CreatePost(
            CreatePostDTO createPostDTO,
            int currentUserId
        );
        public Task<ServiceResponse<PostResponseDTO>> UpdatePost(int userId, PostDTO postDTO);
        public Task<ServiceResponse<string>> DeletePost(int postId, int userId);

        public Task<ServiceResponse<string>> TogglePostLikeAsync(int postId, int userId);

        public Task<ServiceResponse<List<CommentDTO>>> GetPostCommentsAsync(int postId, int? currentUserId);
        public Task<ServiceResponse<CommentDTO>> CreateCommentAsync(int userId, CreateCommentDTO createCommentDto);
        public Task<ServiceResponse<string>> UpdateCommentAsync(int userId, UpdateCommentDTO updateCommentDto);
        public Task<ServiceResponse<string>> DeleteCommentAsync(int userId, int commentId);

        public Task<ServiceResponse<string>> ToggleCommentLikeAsync(int commentId, int userId);
    }
}
