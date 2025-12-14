using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserById(int userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task CreateUser(User user)
        {
            await _db.Users.AddAsync(user);
        }

        public void UpdateUser(User user)
        {
            _db.Users.Update(user);
        }

        public void RemoveUser(User user)
        {
            _db.Users.Remove(user);
        }

        public async Task RegisterUser(User newUser)
        {
            await _db.Users.AddAsync(newUser);
        }

        public async Task<UserFollow?> GetFollowAsync(int followerId, int followedId)
        {
            return await _db.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowedId == followedId);
        }

        public async Task<List<UserFollow>> GetUserFollowersAsync(int userId)
        {
            return await _db.UserFollows
                .Include(uf => uf.Follower)
                .Where(uf => uf.FollowedId == userId)
                .OrderByDescending(uf => uf.FollowedAt)
                .ToListAsync();
        }

        public async Task<List<UserFollow>> GetUserFollowingAsync(int userId)
        {
            return await _db.UserFollows
                .Include(uf => uf.Followed)
                .Where(uf => uf.FollowerId == userId)
                .OrderByDescending(uf => uf.FollowedAt)
                .ToListAsync();
        }

        public async Task<List<UserFollow>> GetMutualFollowsAsync(int userId)
        {
            return await _db.UserFollows
                .Include(uf => uf.Followed)
                .Where(uf => uf.FollowerId == userId &&
                            _db.UserFollows.Any(uf2 =>
                                uf2.FollowerId == uf.FollowedId &&
                                uf2.FollowedId == userId))
                .OrderByDescending(uf => uf.FollowedAt)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followedId)
        {
            return await _db.UserFollows
                .AnyAsync(uf => uf.FollowerId == followerId && uf.FollowedId == followedId);
        }

        public async Task AddFollowAsync(UserFollow userFollow)
        {
            await _db.UserFollows.AddAsync(userFollow);
        }

        public async Task RemoveFollowAsync(UserFollow userFollow)
        {
            _db.UserFollows.Remove(userFollow);
        }

        public async Task<int> GetFollowersCountAsync(int userId)
        {
            return await _db.UserFollows.CountAsync(uf => uf.FollowedId == userId);
        }

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _db.UserFollows.CountAsync(uf => uf.FollowerId == userId);
        }


        public async Task<List<User>> SearchUsersAsync(string? keyword, int page, int pageSize)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var searchTerm = keyword.ToLower().Trim();

                var searchTerms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (searchTerms.Length == 1)
                {
                    var singleTerm = searchTerms[0];
                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(singleTerm) ||
                        u.LastName.ToLower().Contains(singleTerm) ||
                        u.UserName.ToLower().Contains(singleTerm));
                }
                else if (searchTerms.Length >= 2)
                {
                    var firstName = searchTerms[0];
                    var lastName = searchTerms[1];

                    query = query.Where(u =>
                        (u.FirstName.ToLower().Contains(firstName) && u.LastName.ToLower().Contains(lastName)) ||
                        (u.FirstName.ToLower().Contains(lastName) && u.LastName.ToLower().Contains(firstName)) ||
                        (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(searchTerm) ||
                        u.FirstName.ToLower().Contains(firstName) ||
                        u.LastName.ToLower().Contains(firstName) ||
                        u.FirstName.ToLower().Contains(lastName) ||
                        u.LastName.ToLower().Contains(lastName) ||
                        u.UserName.ToLower().Contains(firstName) ||
                        u.UserName.ToLower().Contains(lastName) ||
                        u.UserName.ToLower().Contains(searchTerm));
                }
            }

            return await query
                .OrderByDescending(u => u.FollowersCount) 
                .ThenBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSearchUsersCountAsync(string? keyword)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var searchTerm = keyword.ToLower().Trim();

                var searchTerms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (searchTerms.Length == 1)
                {
                    var singleTerm = searchTerms[0];
                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(singleTerm) ||
                        u.LastName.ToLower().Contains(singleTerm) ||
                        u.UserName.ToLower().Contains(singleTerm));
                }
                else if (searchTerms.Length >= 2)
                {
                    var firstName = searchTerms[0];
                    var lastName = searchTerms[1];

                    query = query.Where(u =>
                        (u.FirstName.ToLower().Contains(firstName) && u.LastName.ToLower().Contains(lastName)) ||
                        (u.FirstName.ToLower().Contains(lastName) && u.LastName.ToLower().Contains(firstName)) ||
                        (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(searchTerm) ||
                        u.FirstName.ToLower().Contains(firstName) ||
                        u.LastName.ToLower().Contains(firstName) ||
                        u.FirstName.ToLower().Contains(lastName) ||
                        u.LastName.ToLower().Contains(lastName) ||
                        u.UserName.ToLower().Contains(firstName) ||
                        u.UserName.ToLower().Contains(lastName) ||
                        u.UserName.ToLower().Contains(searchTerm));
                }
            }

            return await query.CountAsync();
        }


        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
