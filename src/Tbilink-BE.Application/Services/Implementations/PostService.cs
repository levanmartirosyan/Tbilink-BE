using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Domain.Entities;
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

        #region Post Methods

        public async Task<ServiceResponse<List<PostWithUserDTO>>> GetAllPosts(int? currentUserId)
        {
           var posts = await _postRepository.GetAllPosts(currentUserId);

            if (!posts.Any())
            {
                return ServiceResponse<List<PostWithUserDTO>>.Fail(null, "No posts found.", 404);
            }

            var postsWithUser = posts.Select(p => new PostWithUserDTO
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                Username = p.User.UserName,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Avatar = p.User.ProfilePhotoUrl,
                UserId = p.User.Id,
                IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value)
            }).ToList();

            return ServiceResponse<List<PostWithUserDTO>>.Success(postsWithUser, "Posts retrieved successfully.");
        }

        public async Task<ServiceResponse<PaginatedResponse<PostWithUserDTO>>> GetAllPostsPaginated(int? currentUserId, int pageNumber = 1, int pageSize = 10)
        {
            var paginatedPosts = await _postRepository.GetAllPostsPaginated(currentUserId, pageNumber, pageSize);

            if (!paginatedPosts.Data.Any())
            {
                return ServiceResponse<PaginatedResponse<PostWithUserDTO>>.Fail(null, "No posts found.", 404);
            }

            var postsWithUser = paginatedPosts.Data.Select(p => new PostWithUserDTO
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                Username = p.User.UserName,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Avatar = p.User.ProfilePhotoUrl,
                UserId = p.User.Id,
                IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value)
            }).ToList();

            var paginatedResponse = new PaginatedResponse<PostWithUserDTO>
            {
                Data = postsWithUser,
                PageNumber = paginatedPosts.PageNumber,
                PageSize = paginatedPosts.PageSize,
                TotalCount = paginatedPosts.TotalCount,
                TotalPages = paginatedPosts.TotalPages
            };

            return ServiceResponse<PaginatedResponse<PostWithUserDTO>>.Success(paginatedResponse, "Posts retrieved successfully.");
        }

        public async Task<ServiceResponse<List<PostWithUserDTO>>> GetPostsByUserId(int userId, int? currentUserId)
        {
            var posts = await _postRepository.GetPostsByUserId(userId, currentUserId);

            if (!posts.Any())
            {
                return ServiceResponse<List<PostWithUserDTO>>.Fail(null, "No posts found for this user.", 404);
            }

            var postsWithUser = posts.Select(p => new PostWithUserDTO
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                Username = p.User.UserName,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Avatar = p.User.ProfilePhotoUrl,
                UserId = p.User.Id,
                IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value)
            }).ToList();

            return ServiceResponse<List<PostWithUserDTO>>.Success(postsWithUser, "User posts retrieved successfully.");
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

        public async Task<ServiceResponse<PostResponseDTO>> CreatePost(
            CreatePostDTO createPostDTO,
            int currentUserId
        )
        {
            if (string.IsNullOrWhiteSpace(createPostDTO.Content))
            {
                return ServiceResponse<PostResponseDTO>.Fail(
                    null, "Post content cannot be empty.", 400);
            }

            var post = new Post
            {
                UserId = currentUserId,
                Content = createPostDTO.Content,
                ImageUrl = createPostDTO.ImageUrl
            };

            await _postRepository.CreatePost(post);

            if (!await _postRepository.SaveChangesAsync())
            {
                return ServiceResponse<PostResponseDTO>.Fail(
                    null, "Failed to create post.", 500);
            }

            var newPost = await _postRepository.GetPostWithDetails(post.Id);

            var response = new PostResponseDTO
            {
                Id = newPost.Id,
                Content = newPost.Content,
                ImageUrl = newPost.ImageUrl,
                CreatedAt = newPost.CreatedAt,
                LikeCount = newPost.LikeCount,
                CommentCount = newPost.CommentCount,

                UserId = newPost.User.Id,
                Username = newPost.User.UserName,
                FirstName = newPost.User.FirstName,
                LastName = newPost.User.LastName,
                Avatar = newPost.User.ProfilePhotoUrl,

                IsLikedByCurrentUser = newPost.Likes
                    .Any(l => l.UserId == currentUserId)
            };

            return ServiceResponse<PostResponseDTO>.Success(
                response, "Post created successfully.", 201);
        }


        public async Task<ServiceResponse<PostResponseDTO>> UpdatePost(int userId, PostDTO postDTO)
        {
            if (postDTO == null)
            {
                return ServiceResponse<PostResponseDTO>.Fail(
                    null, "Post content cannot be empty.", 400);
            }

            var post = await _postRepository.GetPostById(postDTO.Id);

            if (post == null)
            {
                return ServiceResponse<PostResponseDTO>.Fail(
                    null, "Post not found.", 404);
            }

            var currentUserResponse = await _userService.GetUserInfoById(userId);

            if (currentUserResponse.Data == null)
            {
                return ServiceResponse<PostResponseDTO>.Fail(
                    null, "Current user not found.", 401);
            }

            var currentUser = currentUserResponse.Data;

            var isOwner = currentUser.Id == post.UserId;
            var isAdminOrOwner =
                currentUser.Role == "Admin" || currentUser.Role == "Owner";

            if (!isOwner && !isAdminOrOwner)
            {
                return ServiceResponse<PostResponseDTO>.Fail(
                    null, "You are not allowed to edit this post.", 403);
            }

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
                return ServiceResponse<PostResponseDTO>.Fail(
                    null, "Failed to update post.", 500);
            }

            var updatedPost = await _postRepository.GetPostWithDetails(post.Id);

            var response = new PostResponseDTO
            {
                Id = updatedPost.Id,
                Content = updatedPost.Content,
                ImageUrl = updatedPost.ImageUrl,
                CreatedAt = updatedPost.CreatedAt,

                LikeCount = updatedPost.LikeCount,
                CommentCount = updatedPost.CommentCount,

                UserId = updatedPost.User.Id,
                Username = updatedPost.User.UserName,
                FirstName = updatedPost.User.FirstName,
                LastName = updatedPost.User.LastName,
                Avatar = updatedPost.User.ProfilePhotoUrl,

                IsLikedByCurrentUser = updatedPost.Likes
                    .Any(l => l.UserId == userId)
            };

            return ServiceResponse<PostResponseDTO>.Success(
                response, "Post updated successfully.");
        }


        public async Task<ServiceResponse<string>> DeletePost(int postId, int userId)
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

            var currentUserResponse = await _userService.GetUserInfoById(userId);

            if (currentUserResponse.Data == null)
            {
                return ServiceResponse<string>.Fail(null, "Current user not found.", 401);
            }

            var currentUser = currentUserResponse.Data;

            var isOwner = currentUser.Id == post.UserId;
            var isAdminOrOwner = currentUser.Role == "Admin" || currentUser.Role == "Owner";

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

        #endregion

        #region Like Methods
        public async Task<ServiceResponse<string>> TogglePostLikeAsync(int postId, int userId)
        {
            try
            {
                var post = await _postRepository.GetPostById(postId);
                if (post == null)
                {
                    return ServiceResponse<string>.Fail(null, "Post not found.", 404);
                }

                var existingLike = await _postRepository.GetPostLikeAsync(postId, userId);

                if (existingLike != null)
                {
                    await _postRepository.RemovePostLikeAsync(existingLike);
                    post.LikeCount = Math.Max(0, post.LikeCount - 1);
                }
                else
                {
                    var postLike = new PostLike
                    {
                        PostId = postId,
                        UserId = userId,
                        LikedAt = DateTime.UtcNow
                    };

                    await _postRepository.AddPostLikeAsync(postLike);
                    post.LikeCount += 1;

                    // Send notification to post owner (if not liking own post)
                    //if (post.UserId != userId)
                    //{
                    //    await _notificationService.SendPostLikeNotificationAsync(post.UserId, userId, postId);
                    //}
                }

                if (await _postRepository.SaveChangesAsync())
                {
                    var action = existingLike != null ? "unliked" : "liked";
                    return ServiceResponse<string>.Success(null, $"Post {action} successfully.");
                }

                return ServiceResponse<string>.Fail(null, "Failed to update post like.", 500);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, "An error occurred while updating post like.", 500);
            }
        }

        #endregion

        #region Comment Methods

        public async Task<ServiceResponse<List<CommentDTO>>> GetPostCommentsAsync(int postId, int? currentUserId)
        {
            try
            {
                var post = await _postRepository.GetPostById(postId);
                if (post == null)
                {
                    return ServiceResponse<List<CommentDTO>>.Fail(null, "Post not found.", 404);
                }

                var comments = await _postRepository.GetPostCommentsAsync(postId, currentUserId);

                var commentDtos = comments.Select(c => new CommentDTO
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    FirstName = c.User.FirstName,
                    LastName = c.User.LastName,
                    Avatar = c.User.ProfilePhotoUrl,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    LikeCount = c.LikeCount,
                    IsLikedByCurrentUser = currentUserId.HasValue && c.Likes.Any(l => l.UserId == currentUserId.Value)
                }).ToList();

                return ServiceResponse<List<CommentDTO>>.Success(commentDtos, "Comments retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<CommentDTO>>.Fail(null, "An error occurred while retrieving comments.", 500);
            }
        }

        public async Task<ServiceResponse<CommentDTO>> CreateCommentAsync(int userId, CreateCommentDTO createCommentDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createCommentDto.Content))
                {
                    return ServiceResponse<CommentDTO>.Fail(
                        null, "Comment content cannot be empty.", 400);
                }

                var post = await _postRepository.GetPostById(createCommentDto.PostId);
                if (post == null)
                {
                    return ServiceResponse<CommentDTO>.Fail(
                        null, "Post not found.", 404);
                }

                var userResponse = await _userService.GetUserInfoById(userId);
                if (userResponse.Data == null)
                {
                    return ServiceResponse<CommentDTO>.Fail(
                        null, "User not found.", 404);
                }

                var comment = new Comment
                {
                    PostId = createCommentDto.PostId,
                    UserId = userId,
                    Content = createCommentDto.Content.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                await _postRepository.AddCommentAsync(comment);

                post.CommentCount += 1;
                _postRepository.UpdatePost(post);

                if (!await _postRepository.SaveChangesAsync())
                {
                    return ServiceResponse<CommentDTO>.Fail(
                        null, "Failed to create comment.", 500);
                }

                var createdComment =
                    await _postRepository.GetCommentWithDetailsAsync(comment.Id);

                var commentDto = new CommentDTO
                {
                    Id = createdComment.Id,
                    PostId = createdComment.PostId,
                    UserId = createdComment.UserId,
                    UserName = createdComment.User.UserName,
                    FirstName = createdComment.User.FirstName,
                    LastName = createdComment.User.LastName,
                    Avatar = createdComment.User.ProfilePhotoUrl,
                    Content = createdComment.Content,
                    CreatedAt = createdComment.CreatedAt,
                    LikeCount = createdComment.LikeCount,
                    IsLikedByCurrentUser =
                        createdComment.Likes.Any(l => l.UserId == userId)
                };

                return ServiceResponse<CommentDTO>.Success(
                    commentDto, "Comment created successfully.", 201);
            }
            catch
            {
                return ServiceResponse<CommentDTO>.Fail(
                    null, "An error occurred while creating comment.", 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateCommentAsync(int userId, UpdateCommentDTO updateCommentDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updateCommentDto.Content))
                {
                    return ServiceResponse<string>.Fail(null, "Comment content cannot be empty.", 400);
                }

                var comment = await _postRepository.GetCommentByIdAsync(updateCommentDto.Id);
                if (comment == null)
                {
                    return ServiceResponse<string>.Fail(null, "Comment not found.", 404);
                }

                var currentUser = await _userService.GetUserInfoById(userId);
                if (currentUser.Data == null)
                {
                    return ServiceResponse<string>.Fail(null, "User not found.", 404);
                }

                var isOwner = comment.UserId == userId;
                var isAdminOrOwner = currentUser.Data.Role == "Admin" || currentUser.Data.Role == "Owner";

                if (!isOwner && !isAdminOrOwner)
                {
                    return ServiceResponse<string>.Fail(null, "You are not allowed to edit this comment.", 403);
                }

                comment.Content = updateCommentDto.Content.Trim();
                _postRepository.UpdateComment(comment);

                if (await _postRepository.SaveChangesAsync())
                {
                    return ServiceResponse<string>.Success(null, "Comment updated successfully.");
                }

                return ServiceResponse<string>.Fail(null, "Failed to update comment.", 500);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, "An error occurred while updating comment.", 500);
            }
        }

        public async Task<ServiceResponse<string>> DeleteCommentAsync(int userId, int commentId)
        {
            try
            {
                var comment = await _postRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                {
                    return ServiceResponse<string>.Fail(null, "Comment not found.", 404);
                }

                var currentUser = await _userService.GetUserInfoById(userId);
                if (currentUser.Data == null)
                {
                    return ServiceResponse<string>.Fail(null, "User not found.", 404);
                }

                var isCommentOwner = comment.UserId == userId;
                var isPostOwner = comment.Post.UserId == userId;
                var isAdminOrOwner = currentUser.Data.Role == "Admin" || currentUser.Data.Role == "Owner";

                if (!isCommentOwner && !isPostOwner && !isAdminOrOwner)
                {
                    return ServiceResponse<string>.Fail(null, "You are not allowed to delete this comment.", 403);
                }

                _postRepository.DeleteComment(comment);

                var post = comment.Post;
                post.CommentCount = Math.Max(0, post.CommentCount - 1);
                _postRepository.UpdatePost(post);

                if (await _postRepository.SaveChangesAsync())
                {
                    return ServiceResponse<string>.Success(null, "Comment deleted successfully.");
                }

                return ServiceResponse<string>.Fail(null, "Failed to delete comment.", 500);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, "An error occurred while deleting comment.", 500);
            }
        }

        #endregion

        #region Comment Like Methods
        public async Task<ServiceResponse<string>> ToggleCommentLikeAsync(int commentId, int userId)
        {
            try
            {
                var comment = await _postRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                {
                    return ServiceResponse<string>.Fail(null, "Comment not found.", 404);
                }

                var existingLike = await _postRepository.GetCommentLikeAsync(commentId, userId);

                if (existingLike != null)
                {
                    await _postRepository.RemoveCommentLikeAsync(existingLike);
                    comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
                }
                else
                {
                    var commentLike = new CommentLike
                    {
                        CommentId = commentId,
                        UserId = userId,
                        LikedAt = DateTime.UtcNow
                    };

                    await _postRepository.AddCommentLikeAsync(commentLike);
                    comment.LikeCount += 1;

                    // Send notification to comment owner (if not liking own comment)
                    //if (comment.UserId != userId)
                    //{
                    //    await _notificationService.SendCommentLikeNotificationAsync(comment.UserId, userId, comment.PostId, commentId);
                    //}
                }

                _postRepository.UpdateComment(comment);

                if (await _postRepository.SaveChangesAsync())
                {
                    var action = existingLike != null ? "unliked" : "liked";
                    return ServiceResponse<string>.Success(null, $"Comment {action} successfully.");
                }

                return ServiceResponse<string>.Fail(null, "Failed to update comment like.", 500);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(null, "An error occurred while updating comment like.", 500);
            }
        }

        #endregion
    }
}
