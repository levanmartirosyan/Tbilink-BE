using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessageService(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<ServiceResponse<string>> DeleteMessageAsync(int userId, int messageId)
        {
            var message = await _messageRepository.GetMessageAsync(messageId);

            if (message == null)
            {
                return ServiceResponse<string>.Fail(null, "Message not found.", 404);
            }

            if (message.SenderId != userId && message.RecipientId != userId)
            {
                return ServiceResponse<string>.Fail(null, "Unauthorized.", 401);
            }

            if (message.SenderId == userId)
            {
                message.SenderDeleted = true;
            }

            if (message.RecipientId == userId)
            {
                message.RecipientDeleted = true;
            }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            if (await _messageRepository.SaveChangesAsync())
            {
                return ServiceResponse<string>.Success(null, "Message deleted successfully.");
            }
            else
            {
                return ServiceResponse<string>.Fail(null, "Unable to delete message.", 500);
            }
        }

        public async Task<ServiceResponse<List<Message>>> GetMessagesForUserAsync(int userId)
        {
            var message = await _messageRepository.GetMessagesForUserAsync(userId);

            if (message == null)
            {
                return ServiceResponse<List<Message>>.Fail(null, "Messages not found.", 404);
            }

            var user = await _userRepository.GetUserById(userId);   

            return ServiceResponse<List<Message>>.Success(message, "Messages retrieved successfully.");
        }


        public async Task<ServiceResponse<List<ChatDTO>>> GetUserChatsAsync(int userId)
        {
            try
            {
                var latestMessages = await _messageRepository.GetMessagesForUserAsync(userId);

                if (latestMessages == null || !latestMessages.Any())
                {
                    return ServiceResponse<List<ChatDTO>>.Success(new List<ChatDTO>(), "No chats found for user.");
                }

                var chats = new List<ChatDTO>();

                foreach (var message in latestMessages)
                {
                    var conversationPartnerId = message.SenderId == userId ? message.RecipientId : message.SenderId;
                    var conversationPartner = await _userRepository.GetUserById(conversationPartnerId);

                    if (conversationPartner == null) continue;

                    var participants = new List<UserDTO>
                    {
                        new UserDTO
                        {
                            Id = conversationPartner.Id,
                            FirstName = conversationPartner.FirstName,
                            LastName = conversationPartner.LastName,
                            UserName = conversationPartner.UserName,
                            ProfilePhotoUrl = conversationPartner.ProfilePhotoUrl,
                            Email = conversationPartner.Email
                        }
                    };

                    var lastMessageDto = new MessageDTO
                    {
                        Id = message.Id,
                        SenderId = message.SenderId,
                        SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
                        Content = message.Content,
                        MessageSent = message.MessageSent
                    };

                    var unreadCount = await GetUnreadMessageCountForDirectChat(userId, conversationPartnerId);

                    chats.Add(new ChatDTO
                    {
                        GroupName = $"chat_{Math.Min(userId, conversationPartnerId)}_{Math.Max(userId, conversationPartnerId)}",
                        Participants = participants,
                        LastMessage = lastMessageDto,
                        LastActivity = message.MessageSent,
                        UnreadCount = unreadCount
                    });
                }

                chats = chats.OrderByDescending(c => c.LastActivity).ToList();

                return ServiceResponse<List<ChatDTO>>.Success(chats, "User chats retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<ChatDTO>>.Fail(null, $"Error retrieving user chats: {ex.Message}", 500);
            }
        }

        private async Task<int> GetUnreadMessageCountForDirectChat(int userId, int conversationPartnerId)
        {

            try
            {
                var unreadMessages = await _messageRepository.GetMessageThreadAsync(userId, conversationPartnerId);
                return unreadMessages.Count(m => m.SenderId == conversationPartnerId && m.DateRead == null);
            }
            catch
            {
                return 0; 
            }
        }

        public async Task<ServiceResponse<List<Message>>> GetMessageThreadAsync(int currentUserId, int recipientId)
        {
            var messages = await _messageRepository.GetMessageThreadAsync(currentUserId, recipientId);

            if (messages == null)
            {
                return ServiceResponse<List<Message>>.Fail(null, "Messages not found.", 404);
            }

            return ServiceResponse<List<Message>>.Success(messages, "Messages retrieved successfully.");
        }

        public async Task<ServiceResponse<Message>> SendMessageAsync(int senderId, CreateMessageDTO createMessageDTO)
        {
            var sender = await _userRepository.GetUserById(senderId);

            var recipient = await _userRepository.GetUserById(createMessageDTO.RecipientId);

            if (recipient == null || sender == null || sender.Id == createMessageDTO.RecipientId)
            {
                return ServiceResponse<Message>.Fail(null, "Unable to send message.", 400);
            }

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDTO.Content
            };

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveChangesAsync())
            {
                return ServiceResponse<Message>.Success(message, "Message sent successfully.");
            }
            else
            {
                return ServiceResponse<Message>.Fail(null, "Failed to send message.", 500);
            }
        }
    }
}
