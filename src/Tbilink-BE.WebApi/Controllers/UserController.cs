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

        #region User Endpoint

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user ID in token.");
            }

            var response = await _userService.GetUserInfoById(currentUserId);

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

        [HttpDelete("remove/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user ID in token.");
            }

            var response = await _userService.RemoveUser(currentUserId, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _userService.ChangePasswordAsync(userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        #endregion

        #region Follow Endpoints

        [HttpPost("follow/{userId}")]
        public async Task<IActionResult> ToggleFollowUser(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _userService.ToggleFollowUserAsync(currentUserId, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetUserFollowers(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = null;

            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }

            var response = await _userService.GetFollowersAsync(userId, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{userId}/following")]
        public async Task<IActionResult> GetUserFollowing(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = null;

            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }

            var response = await _userService.GetFollowingAsync(userId, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("mutual-follows")]
        public async Task<IActionResult> GetMutualFollows()
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _userService.GetMutualFollowsAsync(currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{userId}/follow-stats")]
        public async Task<IActionResult> GetFollowStats(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = null;

            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }

            var response = await _userService.GetFollowStatsAsync(userId, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        #endregion
    }
}
