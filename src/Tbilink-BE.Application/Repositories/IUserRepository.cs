using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserById(int userId);
        Task<User?> GetUserByUsername(string username);
        Task CreateUser(User user);
        void UpdateUser(User user);
        void RemoveUser(User user);

        Task<UserFollow?> GetFollowAsync(int followerId, int followedId);
        Task<List<UserFollow>> GetUserFollowersAsync(int userId);
        Task<List<UserFollow>> GetUserFollowingAsync(int userId);
        Task<List<UserFollow>> GetMutualFollowsAsync(int userId);
        Task<bool> IsFollowingAsync(int followerId, int followedId);
        Task AddFollowAsync(UserFollow userFollow);
        Task RemoveFollowAsync(UserFollow userFollow);
        Task<int> GetFollowersCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);

        Task<List<User>> SearchUsersAsync(string? keyword, int page, int pageSize);
        Task<int> GetSearchUsersCountAsync(string? keyword);

        Task<bool> SaveChangesAsync();
    }
}
