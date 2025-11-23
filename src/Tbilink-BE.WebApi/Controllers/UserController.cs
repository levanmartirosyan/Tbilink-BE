using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tbilink_BE.Application.Services.Interfaces;

namespace Tbilink_BE.WebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var response = await _userService.GetUserInfoByEmail(User);

            return StatusCode(response.StatusCode, response);
        }
    }
}
