using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;
using Tbilink_BE.WebApi.Attributes;


namespace Tbilink_BE.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signin")]
        public async Task<ActionResult<ServiceResponse<string>>> Login(LoginDTO loginDTO)
        {
            var response = await _authService.Login(loginDTO);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var response = await _authService.Register(registerDTO);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeDTO sendVerificationCodeDTO)
        {
            var response = await _authService.SendtVerificationCode(sendVerificationCodeDTO);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int? userId = null;
            if (int.TryParse(userIdClaim, out var parsed))
            {
                userId = parsed;
            }

            var response = await _authService.VerifyEmail(request.Email, request.Code, userId);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> CreateNewPassword([FromBody] CreateNewPasswordDTO request)
        {
            var response = await _authService.CreateNewPassword(request);

            return StatusCode(response.StatusCode, response);
        }
    }
}
