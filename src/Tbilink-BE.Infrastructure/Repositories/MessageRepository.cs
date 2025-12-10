using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Application.DTOs;

namespace Tbilink_BE.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _db;

        public MessageRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public void AddGroup(Group group)
        {
            _db.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _db.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _db.Messages.Remove(message);
        }

        public async Task<Connection?> GetConnectionAsync(string connectionId)
        {
            return await _db.Connections.FindAsync(connectionId);
        }

        public async Task<Group?> GetGroupForConnectionAsync(string connectionId)
        {
            return await _db.Groups
                .Include(g => g.Connections)
                .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message?> GetMessageAsync(int messageId)
        {
            return await _db.Messages.FindAsync(messageId);
        }

        public async Task<Group?> GetMessageGroupAsync(string groupName)
        {
            return await _db.Groups
                .Include(g => g.Connections)
                .FirstOrDefaultAsync(g => g.Name == groupName);
        }

        public async Task<List<Message>> GetMessagesForUserAsync(int userId)
        {
            var allMessages = await _db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m =>
                    (m.RecipientId == userId && !m.RecipientDeleted) ||
                    (m.SenderId == userId && !m.SenderDeleted)
                )
                .ToListAsync();

            return allMessages
                .GroupBy(m => new {
                    ConversationPartnerId = m.SenderId == userId ? m.RecipientId : m.SenderId
                })
                .Select(g => g.OrderByDescending(m => m.MessageSent).First())
                .OrderByDescending(m => m.MessageSent)
                .ToList();
        }

        public async Task<List<Group>> GetUserChatsAsync(int userId)
        {
            return await _db.Groups
                .Include(g => g.Connections)
                .Where(g => g.Connections.Any(c => c.UserId == userId.ToString()))
                .ToListAsync();
        }

        public async Task<List<int>> GetChatParticipantsAsync(string groupName)
        {
            return await _db.Connections
                .Where(c => c.Group.Name == groupName)
                .Select(c => int.Parse(c.UserId))
                .Distinct()
                .ToListAsync();
        }

        public async Task<Message?> GetLastMessageInGroupAsync(string groupName)
        {
            return await _db.Messages
                .Include(m => m.Sender)
                .Where(m => m.GroupName == groupName) 
                .OrderByDescending(m => m.MessageSent)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetUnreadMessageCountAsync(string groupName, int userId)
        {
            return await _db.Messages
                .Where(m => m.GroupName == groupName &&
                           m.SenderId != userId &&
                           m.DateRead == null)
                .CountAsync();
        }

        public async Task<List<Message>> GetAllUserMessagesDebugAsync(int userId)
        {
            return await _db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => m.RecipientId == userId || m.SenderId == userId)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessageThreadAsync(int currentUserId, int recipientId)
        {
            return await _db.Messages
                .Where(m =>
                    (m.RecipientId == currentUserId && m.SenderId == recipientId && !m.RecipientDeleted) ||
                    (m.RecipientId == recipientId && m.SenderId == currentUserId && !m.SenderDeleted))
                .OrderBy(m => m.MessageSent)
                .ToListAsync();
        }

        public async Task<PaginatedResponse<Message>> GetMessageThreadPaginatedAsync(int currentUserId, int recipientId, int pageNumber = 1, int pageSize = 20)
        {
            var query = _db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m =>
                    (m.RecipientId == currentUserId && m.SenderId == recipientId && !m.RecipientDeleted) ||
                    (m.RecipientId == recipientId && m.SenderId == currentUserId && !m.SenderDeleted))
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var messages = await query
                .OrderBy(m => m.MessageSent)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<Message>
            {
                Data = messages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<PaginatedResponse<ChatDTO>> GetUserChatsPaginatedAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var allMessages = await _db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m =>
                    (m.RecipientId == userId && !m.RecipientDeleted) ||
                    (m.SenderId == userId && !m.SenderDeleted)
                )
                .ToListAsync();

            var latestMessagesPerConversation = allMessages
                .GroupBy(m => new {
                    ConversationPartnerId = m.SenderId == userId ? m.RecipientId : m.SenderId
                })
                .Select(g => g.OrderByDescending(m => m.MessageSent).First())
                .OrderByDescending(m => m.MessageSent)
                .ToList();

            var totalCount = latestMessagesPerConversation.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginatedMessages = latestMessagesPerConversation
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var chats = new List<ChatDTO>();

            foreach (var message in paginatedMessages)
            {
                var conversationPartnerId = message.SenderId == userId ? message.RecipientId : message.SenderId;
                var conversationPartner = await _db.Users.FirstOrDefaultAsync(u => u.Id == conversationPartnerId);

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

                var unreadCount = allMessages
                    .Where(m => 
                        ((m.SenderId == conversationPartnerId && m.RecipientId == userId && !m.RecipientDeleted) ||
                         (m.SenderId == userId && m.RecipientId == conversationPartnerId && !m.SenderDeleted)) &&
                        m.SenderId == conversationPartnerId && 
                        m.DateRead == null)
                    .Count();

                chats.Add(new ChatDTO
                {
                    GroupName = $"chat_{Math.Min(userId, conversationPartnerId)}_{Math.Max(userId, conversationPartnerId)}",
                    Participants = participants,
                    LastMessage = lastMessageDto,
                    LastActivity = message.MessageSent,
                    UnreadCount = unreadCount
                });
            }

            return new PaginatedResponse<ChatDTO>
            {
                Data = chats,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public void RemoveConnection(Connection connection)
        {
            _db.Connections.Remove(connection);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
