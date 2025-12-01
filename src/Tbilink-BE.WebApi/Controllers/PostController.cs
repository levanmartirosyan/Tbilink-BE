using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.WebApi.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllPosts()
        {
            var response = await _postService.GetAllPosts();

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("id")]
        [Authorize]
        public async Task<IActionResult> GetPostById(int postId)
        {
            var response = await _postService.GetPostById(postId);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] PostDTO postDTO)
        {
            var response = await _postService.CreatePost(postDTO);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdatePost([FromBody] PostDTO postDTO)
        {
            var response = await _postService.UpdatePost(postDTO);

            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var response = await _postService.DeletePost(postId);

            return StatusCode(response.StatusCode, response);
        }
    }
}
