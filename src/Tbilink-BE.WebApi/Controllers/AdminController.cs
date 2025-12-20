using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;

namespace Tbilink_BE.WebApi.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        #region Admin Endpoints
        [HttpGet("is-admin")]
        public async Task<IActionResult> IsCurrentUserAdmin()
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user ID in token.");
            }

            var response = await _adminService.IsUserAdmin(currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("users/all")]
        public async Task<IActionResult> GetAllUsersPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var response = await _adminService.GetAllUsersPaginatedAsync(userId, pageNumber, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetAdminStats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var isAdminResponse = await _adminService.IsUserAdmin(userId);
            if (!isAdminResponse.IsSuccess || !isAdminResponse.Data)
            {
                return Forbid("Only Admin or Owner can access this endpoint.");
            }

            var response = await _adminService.GetAdminStatsAsync();
            return StatusCode(response.StatusCode, response.Data);
        }

        [HttpPost("ban/{userId}")]
        public async Task<IActionResult> BanUser(int userId, [FromBody] BanUserRequestDTO dto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var response = await _adminService.BanUserAsync(adminId, userId, dto.Reason, dto.ExpiresAt);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("unban/{userId}")]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var response = await _adminService.UnbanUserAsync(adminId, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("ban-history/{userId}")]
        public async Task<IActionResult> GetBanHistory(int userId)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var response = await _adminService.GetBanHistoryAsync(adminId, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("bans/all")]
        public async Task<IActionResult> GetAllBansPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var adminId))
                return BadRequest("Invalid user ID in token");

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _adminService.GetAllBansPaginatedAsync(adminId, pageNumber, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("comments/all")]
        public async Task<IActionResult> GetAllCommentsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var adminId))
                return BadRequest("Invalid user ID in token");

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _adminService.GetAllCommentsPaginatedAsync(adminId, pageNumber, pageSize);
            return StatusCode(response.StatusCode, response);
        }
        #endregion
    }
}
