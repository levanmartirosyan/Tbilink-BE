using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllPosts(int? currentUserId);
        Task<PaginatedResponse<Post>> GetAllPostsPaginated(int? currentUserId, int pageNumber = 1, int pageSize = 10);
        Task<List<Post>> GetPostsByUserId(int userId, int? currentUserId);
        Task<Post?> GetPostById(int postId);
        Task CreatePost(CreatePostDTO createPostDTO);
        void UpdatePost(Post post);
        void DeletePost(Post post);

        Task<PostLike?> GetPostLikeAsync(int postId, int userId);
        Task<List<PostLike>> GetPostLikesAsync(int postId);
        Task AddPostLikeAsync(PostLike postLike);
        Task RemovePostLikeAsync(PostLike postLike);

        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<List<Comment>> GetPostCommentsAsync(int postId, int? currentUserId);
        Task AddCommentAsync(Comment comment);
        void UpdateComment(Comment comment);
        void DeleteComment(Comment comment);

        Task<CommentLike?> GetCommentLikeAsync(int commentId, int userId);
        Task<List<CommentLike>> GetCommentLikesAsync(int commentId);
        Task AddCommentLikeAsync(CommentLike commentLike);
        Task RemoveCommentLikeAsync(CommentLike commentLike);

        Task<List<Post>> SearchPostsAsync(string? keyword, int page, int pageSize, int? currentUserId = null);
        Task<int> GetSearchPostsCountAsync(string? keyword);

        Task<bool> SaveChangesAsync();
    }
}
