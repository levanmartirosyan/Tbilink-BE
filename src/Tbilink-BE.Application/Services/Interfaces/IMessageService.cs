using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IMessageService
    {
        Task<ServiceResponse<Message>> SendMessageAsync(int senderId, CreateMessageDTO createMessageDTO);
        Task<ServiceResponse<List<ChatDTO>>> GetUserChatsAsync(int userId);
        Task<ServiceResponse<PaginatedResponse<ChatDTO>>> GetUserChatsPaginatedAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<ServiceResponse<List<Message>>> GetMessagesForUserAsync(int userId);
        Task<ServiceResponse<List<Message>>> GetMessageThreadAsync(int currentUserId, int recipientId);
        Task<ServiceResponse<PaginatedResponse<Message>>> GetMessageThreadPaginatedAsync(int currentUserId, int recipientId, int pageNumber = 1, int pageSize = 20);
        Task<ServiceResponse<string>> DeleteMessageAsync(int userId, int messageId);
    }
}
