using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbilink_BE.Domain.Entities;

namespace Tbilink_BE.Application.Repositories
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetNotificationsForUserAsync(int userId);
        Task<bool> SaveChangesAsync();
    }
}
