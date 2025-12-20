using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Domain.Entities;
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

        public async Task<int> GetTotalPostCount()
        {
            return await _db.Posts.CountAsync();
        }

        public async Task<int> GetTotalCommentCount()
        {
            return await _db.Comments.CountAsync();
        }

        public async Task<UserBan?> GetActiveBanAsync(int userId)
        {
            return await _db.UserBans
                .Where(b => b.UserId == userId && b.IsActive)
                .OrderByDescending(b => b.BannedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserBan>> GetBanHistoryAsync(int userId)
        {
            return await _db.UserBans
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BannedAt)
                .ToListAsync();
        }

        public async Task<PaginatedResponse<UserBan>> GetAllBansPaginated(int pageNumber, int pageSize)
        {
            var query = _db.UserBans
                .Include(b => b.User)
                .Include(b => b.BannedBy)
                .OrderByDescending(b => b.BannedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var bans = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<UserBan>
            {
                Data = bans,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task BanUserAsync(UserBan ban)
        {
            var user = await _db.Users.FindAsync(ban.UserId);
            if (user != null)
            {
                user.IsBanned = true;
                _db.UserBans.Add(ban);
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UnbanUserAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsBanned = false;
                var activeBan = await _db.UserBans
                    .Where(b => b.UserId == userId && b.IsActive)
                    .OrderByDescending(b => b.BannedAt)
                    .FirstOrDefaultAsync();
                if (activeBan != null)
                {
                    activeBan.IsActive = false;
                    _db.UserBans.Update(activeBan);
                }
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<PaginatedResponse<AdminCommentDTO>> GetAllCommentsPaginated(int pageNumber, int pageSize)
        {
            var query = _db.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var comments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new AdminCommentDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    PostId = c.PostId,
                    PostContent = c.Post.Content,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    UserEmail = c.User.Email
                })
                .ToListAsync();

            return new PaginatedResponse<AdminCommentDTO>
            {
                Data = comments,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
}
