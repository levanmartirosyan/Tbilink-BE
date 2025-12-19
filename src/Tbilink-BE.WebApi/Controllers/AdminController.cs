using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tbilink_BE.Application.Services.Implementations;
using Tbilink_BE.Application.Services.Interfaces;

namespace Tbilink_BE.WebApi.Controllers
{
    [Authorize]
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
        #endregion
    }
}
