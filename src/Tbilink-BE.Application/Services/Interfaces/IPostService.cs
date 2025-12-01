using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IPostService
    {
        public Task<ServiceResponse<List<Post>>> GetAllPosts();
        public Task<ServiceResponse<Post?>> GetPostById(int postId);
        public Task<ServiceResponse<string>> CreatePost(CreatePostDTO createPostDTO);
        public Task<ServiceResponse<string>> UpdatePost(PostDTO postDTO);
        public Task<ServiceResponse<string>> DeletePost(int postId);
    }
}
