using Microsoft.EntityFrameworkCore;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Models;


namespace Tbilink_BE.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _db;

        public AuthRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RegisterUser(User newUser)
        {
            await _db.Users.AddAsync(newUser);
        }

        public void RecoverPassword(User user, byte[] passwordHash, byte[] passwordSalt)
        {
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _db.Users.Update(user);
        }

        public void SaveRefreshToken(User user, string refreshToken, DateTime expirationDate)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpires = expirationDate;

            _db.Users.Update(user);
        }

        public async Task<EmailVerification?> GetEmailVerificationRecords(int userId)
        {
            return await _db.EmailVerifications.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task AddEmailVerificationRecord(EmailVerification record)
        {
            await _db.EmailVerifications.AddAsync(record);
        }

        public void UpdateEmailVerificationRecord(EmailVerification record)
        {
            _db.EmailVerifications.Update(record);
        }

        public void RemoveEmailVerificationRecord(EmailVerification record)
        {
            _db.EmailVerifications.Remove(record);
        }

        public async Task<bool> UserExistCheck(string userEmail)
        {
            return await _db.Users.AnyAsync(u => u.Email.ToLower() == userEmail.ToLower()); ;
        }

        public async Task<bool> UsernameIsAvailable(string username)
        {
            return await _db.Users.AnyAsync(u => u.UserName.ToLower() == username.ToLower()); ;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
