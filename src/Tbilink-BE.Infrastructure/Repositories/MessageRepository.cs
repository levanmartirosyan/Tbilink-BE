using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Domain.Entities;

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
