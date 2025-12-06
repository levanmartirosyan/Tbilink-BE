using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Services.Interfaces;

namespace Tbilink_BE.WebApi.Controllers
{
    [Authorize]
    [Route("api/message")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetUserChats()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            var response = await _messageService.GetUserChatsAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDTO createMessageDTO)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var response = await _messageService.SendMessageAsync(int.Parse(userIdClaim), createMessageDTO);

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int recipientId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var response = await _messageService.GetMessageThreadAsync(int.Parse(userIdClaim), recipientId);

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetMessagesForUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var response = await _messageService.GetMessagesForUserAsync(int.Parse(userIdClaim));

            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var response = await _messageService.DeleteMessageAsync(int.Parse(userIdClaim), messageId);

            return StatusCode(response.StatusCode, response);
        }
    }
}
