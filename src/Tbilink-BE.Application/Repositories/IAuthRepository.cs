using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Repositories
{
    public interface IAuthRepository
    {
        Task RegisterUser(User newUser);
        Task<User> GetUserByEmail(string email);
        Task RecoverPassword(User user, byte[] passwordHash, byte[] passwordSalt);
        Task SaveRefreshToken(User user, string refreshToken, DateTime expirationDate);
        Task<EmailVerification> GetEmailVerificationRecords(int userId);
        Task AddEmailVerificationRecords(EmailVerification record); 
        Task<bool> UserExistCheck(string userEmail);
        Task<bool> UsernameIsAvailable(string username);
        Task<bool> SaveChangesAsync();
    }
}
