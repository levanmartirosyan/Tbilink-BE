using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.WebApi.Attributes;

namespace Tbilink_BE.WebApi.Controllers
{
    [Authorize]
    [CheckUserBanned]
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IUserService _userService;

        public SearchController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? keyword = null,
            [FromQuery] string category = "all",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = null;

            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }

            var searchRequest = new SearchRequestDTO
            {
                Keyword = keyword,
                Category = category,
                Page = page,
                PageSize = pageSize
            };

            var response = await _userService.SearchAsync(searchRequest, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        //[HttpPost]
        //public async Task<IActionResult> SearchPost([FromBody] SearchRequestDTO searchRequest)
        //{
        //    var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    int? currentUserId = null;

        //    if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var parsedUserId))
        //    {
        //        currentUserId = parsedUserId;
        //    }

        //    var response = await _userService.SearchAsync(searchRequest, currentUserId);
        //    return StatusCode(response.StatusCode, response);
        //}
    }
}
