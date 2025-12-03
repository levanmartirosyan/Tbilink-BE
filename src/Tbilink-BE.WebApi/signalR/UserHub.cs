using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Infrastructure.Repositories;
using Tbilink_BE.Models;

namespace Tbilink_BE.WebApi.signalR
{
    [Authorize]
    public class UserHub(UserTracker presenceTracker, IUserRepository userRepository) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await presenceTracker.UserConnected(GetUserId(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserOnline", GetUserId());

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            await presenceTracker.UserDisconnected(GetUserId(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserOffline", GetUserId());

            await UpdateUserLastActive(userId);

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }

        private string GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Can't get user id from token.");
        }

        private async Task UpdateUserLastActive(string userId)
        {
            try
            {
                if (int.TryParse(userId, out var userIdInt))
                {
                    var user = await userRepository.GetUserById(userIdInt);
                    if (user != null)
                    {
                        user.LastActive = DateTime.UtcNow;
                        userRepository.UpdateUser(user);
                        await userRepository.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating LastActive for user {userId}: {ex.Message}");
            }
        }
    }
}
