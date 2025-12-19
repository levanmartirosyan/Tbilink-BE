using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.WebApi.signalR;

namespace Tbilink_BE.WebApi.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepository)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _notificationRepository.GetNotificationsForUserAsync(userId);
            return Ok(notifications);
        }
    }
}
