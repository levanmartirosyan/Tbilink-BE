using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.WebApi.Controllers
{
    [Authorize]
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        #region Post Endpoints

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPosts()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _postService.GetAllPosts(userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("all/paginated")]
        public async Task<IActionResult> GetAllPostsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var response = await _postService.GetAllPostsPaginated(userId, pageNumber, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUserId(int userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = null;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }

            var response = await _postService.GetPostsByUserId(userId, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetPostById(int postId)
        {
            var response = await _postService.GetPostById(postId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDTO createPostDTO)
        {
            var response = await _postService.CreatePost(createPostDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdatePost([FromBody] PostDTO postDTO)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _postService.UpdatePost(int.Parse(userIdClaim), postDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var response = await _postService.DeletePost(postId, User);
            return StatusCode(response.StatusCode, response);
        }

        #endregion

        #region Like Endpoints

        [HttpPost("{postId}/like")]
        public async Task<IActionResult> TogglePostLike(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _postService.TogglePostLikeAsync(postId, userId);
            return StatusCode(response.StatusCode, response);
        }

        #endregion

        #region Comment Endpoints

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetPostComments(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = null;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                currentUserId = userId;
            }

            var response = await _postService.GetPostCommentsAsync(postId, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{postId}/comments/create")]
        public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentDTO createCommentDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            createCommentDto.PostId = postId;

            var response = await _postService.CreateCommentAsync(userId, createCommentDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("comments/{commentId}/update")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDTO updateCommentDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            updateCommentDto.Id = commentId;

            var response = await _postService.UpdateCommentAsync(userId, updateCommentDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("comments/{commentId}/delete")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _postService.DeleteCommentAsync(userId, commentId);
            return StatusCode(response.StatusCode, response);
        }

        #endregion

        #region Comment Like Endpoints

        [HttpPost("comments/{commentId}/like")]
        public async Task<IActionResult> ToggleCommentLike(int commentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _postService.ToggleCommentLikeAsync(commentId, userId);
            return StatusCode(response.StatusCode, response);
        }

        #endregion

    }
}
