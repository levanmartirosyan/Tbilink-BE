using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Application.DTOs;

namespace Tbilink_BE.Application.Repositories
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message?> GetMessageAsync(int messageId);
        Task<List<Message>> GetMessagesForUserAsync(int userId);
        Task<List<Message>> GetMessageThreadAsync(int currentUserId, int recipientId);
        Task<PaginatedResponse<Message>> GetMessageThreadPaginatedAsync(int currentUserId, int recipientId, int pageNumber = 1, int pageSize = 20);
        Task<PaginatedResponse<ChatDTO>> GetUserChatsPaginatedAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<List<Group>> GetUserChatsAsync(int userId);
        Task<List<int>> GetChatParticipantsAsync(string groupName);
        Task<Message?> GetLastMessageInGroupAsync(string groupName);
        Task<int> GetUnreadMessageCountAsync(string groupName, int userId);
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection?> GetConnectionAsync(string connectionId);
        Task<Group?> GetGroupForConnectionAsync(string connectionId);
        Task<Group?> GetMessageGroupAsync(string groupName);
        Task<bool> SaveChangesAsync();
    }
}
