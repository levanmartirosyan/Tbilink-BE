using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IPostService
    {
        public Task<ServiceResponse<List<PostWithUserDTO>>> GetAllPosts(int? currentUserId);
        public Task<ServiceResponse<Post?>> GetPostById(int postId);
        public Task<ServiceResponse<string>> CreatePost(CreatePostDTO createPostDTO);
        public Task<ServiceResponse<string>> UpdatePost(int userId, PostDTO postDTO);
        public Task<ServiceResponse<string>> DeletePost(int postId, ClaimsPrincipal currentUserPrincipal);

        public Task<ServiceResponse<string>> TogglePostLikeAsync(int postId, int userId);
        public Task<ServiceResponse<bool>> HasUserLikedPostAsync(int postId, int userId);
        public Task<ServiceResponse<int>> GetPostLikeCountAsync(int postId);

        public Task<ServiceResponse<List<CommentDTO>>> GetPostCommentsAsync(int postId, int? currentUserId);
        public Task<ServiceResponse<CommentDTO>> CreateCommentAsync(int userId, CreateCommentDTO createCommentDto);
        public Task<ServiceResponse<string>> UpdateCommentAsync(int userId, UpdateCommentDTO updateCommentDto);
        public Task<ServiceResponse<string>> DeleteCommentAsync(int userId, int commentId);

        public Task<ServiceResponse<string>> ToggleCommentLikeAsync(int commentId, int userId);
        public Task<ServiceResponse<bool>> HasUserLikedCommentAsync(int commentId, int userId);
        public Task<ServiceResponse<int>> GetCommentLikeCountAsync(int commentId);
    }
}
