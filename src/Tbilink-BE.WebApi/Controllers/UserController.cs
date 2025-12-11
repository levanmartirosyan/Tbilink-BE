using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;

namespace Tbilink_BE.WebApi.Controllers
{
    [Authorize]
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var response = await _userService.GetUserInfoByEmail(User);

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var response = await _userService.GetUserInfoByUsername(username);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDTO updateUserDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _userService.UpdateUserAsync(userId, updateUserDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update/{targetUserId}")]
        public async Task<IActionResult> UpdateUser(int targetUserId, [FromBody] UpdateUserDTO updateUserDto)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _userService.UpdateUserAsync(currentUserId, targetUserId, updateUserDto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
