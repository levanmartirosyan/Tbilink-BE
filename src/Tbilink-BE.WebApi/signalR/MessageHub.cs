using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.WebApi.signalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<UserHub> _presenceHub;
        private readonly ILogger<MessageHub> _logger;

        public MessageHub(
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IHubContext<UserHub> presenceHub,
            ILogger<MessageHub> logger)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _presenceHub = presenceHub;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var otherUserId = httpContext?.Request?.Query["userId"].ToString();

                if (string.IsNullOrEmpty(otherUserId) || !int.TryParse(otherUserId, out var recipientId))
                {
                    throw new HubException("Invalid or missing recipient user ID");
                }

                var currentUserId = GetUserId();
                var groupName = GetGroupName(currentUserId.ToString(), otherUserId);

                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                await AddToGroup(groupName);

                var messages = await _messageRepository.GetMessageThreadAsync(currentUserId, recipientId);

                await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);

                //await MarkMessagesAsRead(recipientId);

                _logger.LogInformation("User {UserId} connected to message group {GroupName}", currentUserId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for user {UserId}", Context.UserIdentifier);
                throw new HubException("Failed to connect to message hub");
            }
        }

        public async Task SendMessage(CreateMessageDTO createMessageDto)
        {
            try
            {
                var senderId = GetUserId();
                var sender = await _userRepository.GetUserById(senderId);
                var recipient = await _userRepository.GetUserById(createMessageDto.RecipientId);

                if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
                {
                    throw new HubException("Cannot send message - invalid sender or recipient");
                }

                var message = new Message
                {
                    SenderId = sender.Id,
                    RecipientId = recipient.Id,
                    Content = createMessageDto.Content,
                    MessageSent = DateTime.UtcNow
                };

                var groupName = GetGroupName(sender.Id.ToString(), recipient.Id.ToString());
                var group = await _messageRepository.GetMessageGroupAsync(groupName);

                var recipientInGroup = group?.Connections?.Any(x => x.UserId == message.RecipientId.ToString()) == true;

                if (recipientInGroup)
                {
                    message.DateRead = DateTime.UtcNow; 
                }

                _messageRepository.AddMessage(message);

                if (await _messageRepository.SaveChangesAsync())
                {
                    var messageDto = new MessageDTO
                    {
                        Id = message.Id,
                        SenderId = message.SenderId,
                        SenderName = $"{sender.FirstName} {sender.LastName}",
                        Content = message.Content,
                        MessageSent = message.MessageSent
                    };

                    var enhancedMessageDto = new
                    {
                        Id = message.Id,
                        SenderId = message.SenderId,
                        SenderName = $"{sender.FirstName} {sender.LastName}",
                        SenderAvatar = sender.ProfilePhotoUrl,
                        Content = message.Content,
                        MessageSent = message.MessageSent,
                        DateRead = message.DateRead,
                        IsRead = message.DateRead.HasValue
                    };

                    await Clients.Group(groupName).SendAsync("NewMessage", enhancedMessageDto);

                    await UpdateChatLastMessage(senderId, createMessageDto.RecipientId, messageDto);
                    await UpdateChatLastMessage(createMessageDto.RecipientId, senderId, messageDto);

                    if (!recipientInGroup)
                    {
                        await SendNotificationToUser(recipient, sender, messageDto);
                    }

                    _logger.LogInformation("Message sent from {SenderId} to {RecipientId}", senderId, createMessageDto.RecipientId);
                }
                else
                {
                    throw new HubException("Failed to save message");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from user {UserId}", Context.UserIdentifier);
                throw new HubException("Failed to send message");
            }
        }

        public async Task EditMessage(int messageId, string newContent)
        {
            try
            {
                var userId = GetUserId();
                var message = await _messageRepository.GetMessageAsync(messageId);

                if (message == null)
                    throw new HubException("Message not found");

                if (message.SenderId != userId)
                    throw new HubException("You can only edit your own messages");

                message.Content = newContent;

                if (await _messageRepository.SaveChangesAsync())
                {
                    var groupName = GetGroupName(message.SenderId.ToString(), message.RecipientId.ToString());
                    await Clients.Group(groupName).SendAsync("MessageEdited", new
                    {
                        MessageId = message.Id,
                        NewContent = newContent
                    });
                }
                else
                {
                    throw new HubException("Failed to edit message");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message {MessageId} by user {UserId}", messageId, Context.UserIdentifier);
                throw new HubException("Failed to edit message");
            }
        }

        public async Task DeleteMessage(int messageId)
        {
            try
            {
                var userId = GetUserId();
                var message = await _messageRepository.GetMessageAsync(messageId);

                if (message == null)
                    throw new HubException("Message not found");

                if (message.SenderId != userId && message.RecipientId != userId)
                    throw new HubException("You can only delete your own messages");

                if (message.SenderId == userId)
                    message.SenderDeleted = true;
                if (message.RecipientId == userId)
                    message.RecipientDeleted = true;

                if (message.SenderDeleted && message.RecipientDeleted)
                    _messageRepository.DeleteMessage(message);

                if (await _messageRepository.SaveChangesAsync())
                {
                    var groupName = GetGroupName(message.SenderId.ToString(), message.RecipientId.ToString());
                    await Clients.Group(groupName).SendAsync("MessageDeleted", new
                    {
                        MessageId = message.Id
                    });
                }
                else
                {
                    throw new HubException("Failed to delete message");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId} by user {UserId}", messageId, Context.UserIdentifier);
                throw new HubException("Failed to delete message");
            }
        }

        public async Task MarkMessagesAsRead(int otherUserId)
        {
            try
            {
                var currentUserId = GetUserId();
                var messages = await _messageRepository.GetMessageThreadAsync(currentUserId, otherUserId);

                var unreadMessages = messages.Where(m =>
                    m.RecipientId == currentUserId &&
                    m.DateRead == null).ToList();

                if (unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        message.DateRead = DateTime.UtcNow;
                    }

                    if (await _messageRepository.SaveChangesAsync())
                    {
                        var groupName = GetGroupName(currentUserId.ToString(), otherUserId.ToString());

                        await Clients.Group(groupName).SendAsync("MessagesMarkedAsRead", new
                        {
                            MessageIds = unreadMessages.Select(m => m.Id).ToList(),
                            ReadByUserId = currentUserId,
                            ReadAt = DateTime.UtcNow
                        });

                        await UpdateChatUnreadCount(currentUserId, otherUserId);

                        _logger.LogInformation("Marked {Count} messages as read for user {UserId}",
                            unreadMessages.Count, currentUserId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for user {UserId}", Context.UserIdentifier);
                throw new HubException("Failed to mark messages as read");
            }
        }

        public async Task TypingStarted(int recipientId)
        {
            try
            {
                var currentUserId = GetUserId();
                var groupName = GetGroupName(currentUserId.ToString(), recipientId.ToString());

                await Clients.GroupExcept(groupName, Context.ConnectionId)
                    .SendAsync("UserTyping", new { UserId = currentUserId, IsTyping = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TypingStarted for user {UserId}", Context.UserIdentifier);
            }
        }

        public async Task TypingStopped(int recipientId)
        {
            try
            {
                var currentUserId = GetUserId();
                var groupName = GetGroupName(currentUserId.ToString(), recipientId.ToString());

                await Clients.GroupExcept(groupName, Context.ConnectionId)
                    .SendAsync("UserTyping", new { UserId = currentUserId, IsTyping = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TypingStopped for user {UserId}", Context.UserIdentifier);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var connection = await _messageRepository.GetConnectionAsync(Context.ConnectionId);
                if (connection != null)
                {
                    _messageRepository.RemoveConnection(connection);
                    await _messageRepository.SaveChangesAsync();
                }

                _logger.LogInformation("User {UserId} disconnected from message hub", Context.UserIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for user {UserId}", Context.UserIdentifier);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateChatLastMessage(int userId, int otherUserId, MessageDTO lastMessage)
        {
            try
            {
                var userConnections = await UserTracker.GetConnectionsForUser(userId.ToString());

                if (userConnections.Count > 0)
                {
                    var chatUpdateDto = new
                    {
                        ChatPartnerId = otherUserId,
                        LastMessage = lastMessage,
                        LastActivity = lastMessage.MessageSent,
                        UnreadCount = await GetUnreadCountForUser(userId, otherUserId)
                    };

                    await _presenceHub.Clients.Clients(userConnections)
                        .SendAsync("ChatUpdated", chatUpdateDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chat last message for user {UserId}", userId);
            }
        }

        private async Task UpdateChatUnreadCount(int userId, int otherUserId)
        {
            try
            {
                var userConnections = await UserTracker.GetConnectionsForUser(userId.ToString());

                if (userConnections.Count > 0)
                {
                    var unreadCount = await GetUnreadCountForUser(userId, otherUserId);

                    await _presenceHub.Clients.Clients(userConnections)
                        .SendAsync("ChatUnreadCountUpdated", new
                        {
                            ChatPartnerId = otherUserId,
                            UnreadCount = unreadCount
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unread count for user {UserId}", userId);
            }
        }

        private async Task<int> GetUnreadCountForUser(int userId, int otherUserId)
        {
            try
            {
                var messages = await _messageRepository.GetMessageThreadAsync(userId, otherUserId);
                return messages.Count(m => m.RecipientId == userId && m.DateRead == null);
            }
            catch
            {
                return 0;
            }
        }

        private async Task SendNotificationToUser(User recipient, User sender, MessageDTO message)
        {
            try
            {
                var recipientConnections = await UserTracker.GetConnectionsForUser(recipient.Id.ToString());

                if (recipientConnections.Count > 0)
                {
                    var notificationDto = new
                    {
                        Type = "NewMessage",
                        Title = $"New message from {sender.FirstName} {sender.LastName}",
                        Message = message.Content.Length > 50 ?
                            message.Content.Substring(0, 50) + "..." :
                            message.Content,
                        SenderId = sender.Id,
                        SenderName = $"{sender.FirstName} {sender.LastName}",
                        SenderAvatar = sender.ProfilePhotoUrl,
                        MessageId = message.Id,
                        Timestamp = message.MessageSent
                    };

                    await _presenceHub.Clients.Clients(recipientConnections)
                        .SendAsync("NewMessageReceived", notificationDto);

                    await _presenceHub.Clients.Clients(recipientConnections)
                        .SendAsync("NotificationReceived", notificationDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", recipient.Id);
            }
        }

        private async Task<bool> AddToGroup(string groupName)
        {
            try
            {
                var group = await _messageRepository.GetMessageGroupAsync(groupName);
                var connection = new Connection(Context.ConnectionId, GetUserId().ToString());

                if (group == null)
                {
                    group = new Group(groupName);
                    _messageRepository.AddGroup(group);
                }

                group.Connections.Add(connection);
                return await _messageRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user to group {GroupName}", groupName);
                return false;
            }
        }

        private static string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"chat_{caller}_{other}" : $"chat_{other}_{caller}";
        }

        private int GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new HubException("Cannot get user ID from token");
            }
            return userId;
        }
    }
}