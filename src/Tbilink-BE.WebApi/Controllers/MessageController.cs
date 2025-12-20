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

        [HttpGet("chats/paginated")]
        public async Task<IActionResult> GetUserChatsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var response = await _messageService.GetUserChatsPaginatedAsync(userId, pageNumber, pageSize);
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

        [HttpGet("thread/{recipientId}/paginated")]
        public async Task<IActionResult> GetMessageThreadPaginated(int recipientId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID in token");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var response = await _messageService.GetMessageThreadPaginatedAsync(userId, recipientId, pageNumber, pageSize);

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
