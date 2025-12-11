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
        Task<bool> SaveChangesAsync();
    }
}
