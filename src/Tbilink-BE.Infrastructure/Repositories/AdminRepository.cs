using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Models;

namespace Tbilink_BE.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllUsers(int pageNumber, int pageSize)
        {
            return await _db.Users
                .OrderBy(u => u.RegisterDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalUserCount()
        {
            return await _db.Users.CountAsync();
        }
    }
}
