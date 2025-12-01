using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;

        public PostService(IPostRepository postRepository, IUserService userService)
        {
            _postRepository = postRepository;
            _userService = userService;
        }

        public async Task<ServiceResponse<List<Post>>> GetAllPosts()
        {
           var posts = await _postRepository.GetAllPosts();

            if (!posts.Any())
            {
                return ServiceResponse<List<Post>>.Fail(null, "No posts found.", 404);
            }

            return ServiceResponse<List<Post>>.Success(posts, "Posts retrieved successfully.");
        }

        public async Task<ServiceResponse<Post?>> GetPostById(int postId)
        {
            if (postId < 0)
            {
                return ServiceResponse<Post?>.Fail(null, "Invalid post ID.", 400);
            }

            var post = await _postRepository.GetPostById(postId);

            if (post == null)
            {
                return ServiceResponse<Post?>.Fail(null, "Post not found.", 404);
            }

            return ServiceResponse<Post?>.Success(post, "Post retrieved successfully.");
        }

        public async Task<ServiceResponse<string>> CreatePost(CreatePostDTO createPostDTO)
        {
            if (string.IsNullOrWhiteSpace(createPostDTO.Content))
            {
                return ServiceResponse<string>.Fail(null, "Post content cannot be empty.", 400);
            }

            await _postRepository.CreatePost(createPostDTO);

            if (!await _postRepository.SaveChangesAsync())
            {
                ServiceResponse<string>.Fail(null, "Failed to create post.", 500);
            }

            return ServiceResponse<string>.Success(null, "Post created successfully.", 201);
        }

        public async Task<ServiceResponse<string>> UpdatePost(PostDTO postDTO)
        {
            if (postDTO == null)
            {
                return ServiceResponse<string>.Fail(null, "Post content cannot be empty.", 400);
            }

            var post = await _postRepository.GetPostById(postDTO.Id);

            if (post == null)
            {
                return ServiceResponse<string>.Fail(null, "Post not found.", 404);
            }

            post.UserId = postDTO.UserId;

            if (!string.IsNullOrWhiteSpace(postDTO.Content))
            {
                post.Content = postDTO.Content;
            }

            if (!string.IsNullOrWhiteSpace(postDTO.ImageUrl))
            {
                post.ImageUrl = postDTO.ImageUrl;
            }

            _postRepository.UpdatePost(post);

            if (!await _postRepository.SaveChangesAsync())
            {
                return ServiceResponse<string>.Fail(null, "Failed to update post.", 500);
            }

            return ServiceResponse<string>.Success(null, "Post updated successfully.");
        }

        public async Task<ServiceResponse<string>> DeletePost(int postId)
        {
            if (postId < 0)
            {
                return ServiceResponse<string>.Fail(null, "Invalid post ID.", 400);
            }

            var post = await _postRepository.GetPostById(postId);

            if (post == null)
            {
                return ServiceResponse<string>.Fail(null, "Post not found.", 404);
            }

            var user = await _userService.GetUserInfoById(post.UserId);

            if (user.Data == null)
            {
                return ServiceResponse<string>.Fail(null, "User not found.", 404);
            }

            var isOwner = user.Data.Id == post.UserId;
            var isAdminOrOwner = user.Data.Role == "Admin" || user.Data.Role == "Owner";

            if (!isOwner && !isAdminOrOwner)
            {
                return ServiceResponse<string>.Fail(null, "You are not allowed to delete this post.", 403);
            }

            _postRepository.DeletePost(post);

            if (!await _postRepository.SaveChangesAsync())
            {
                return ServiceResponse<string>.Fail(null, "Failed to delete post.", 500);
            }

            return ServiceResponse<string>.Success(null, "Post deleted successfully.");
        }
    }
}
