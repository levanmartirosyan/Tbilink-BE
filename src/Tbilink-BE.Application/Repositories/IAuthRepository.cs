using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Repositories
{
    public interface IAuthRepository
    {
        Task RegisterUser(User newUser);
        void RecoverPassword(User user, byte[] passwordHash, byte[] passwordSalt);
        void SaveRefreshToken(User user, string refreshToken, DateTime expirationDate);
        Task<EmailVerification?> GetEmailVerificationRecords(int userId);
        Task AddEmailVerificationRecord(EmailVerification record);
        void UpdateEmailVerificationRecord(EmailVerification record); 
        void RemoveEmailVerificationRecord(EmailVerification record);
        Task<bool> UserExistCheck(string userEmail);
        Task<bool> UsernameIsAvailable(string username);
        Task<bool> SaveChangesAsync();
    }
}
