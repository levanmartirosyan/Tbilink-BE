using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllPosts();
        Task<Post?> GetPostById(int postId);
        Task CreatePost(CreatePostDTO createPostDTO);
        void UpdatePost(Post post);
        void DeletePost(Post post);
        Task<bool> SaveChangesAsync();
    }
}
