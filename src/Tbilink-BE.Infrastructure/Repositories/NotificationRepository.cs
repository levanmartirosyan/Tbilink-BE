using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _db;

        public NotificationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _db.Notifications.AddAsync(notification);
            await SaveChangesAsync();
        }

        public async Task<List<Notification>> GetNotificationsForUserAsync(int userId)
        {
            return await _db.Notifications
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
