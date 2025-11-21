using System.Security.Cryptography;

namespace Tbilink_BE.Application.Common
{
    public static class OtpHelper
    {
        public static string GenerateCode(int length = 6)
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            int number = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            return (number % (int)Math.Pow(10, length)).ToString($"D{length}");
        }

        public static string HashCode(string code)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyHash(string plainText, string storedHash)
        {
            var newHash = HashCode(plainText); 
                                              
            return newHash.Equals(storedHash, StringComparison.Ordinal);
        }

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }
    }
}
