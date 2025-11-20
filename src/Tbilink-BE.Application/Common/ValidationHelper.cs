using System.Text.RegularExpressions;

namespace Tbilink_BE.Services.Helpers
{
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PasswordRegex = new Regex(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            RegexOptions.Compiled);

        public static bool IsValidEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);

        public static bool IsValidPassword(string password) =>
            !string.IsNullOrWhiteSpace(password) && PasswordRegex.IsMatch(password);

        public static bool PasswordCompare(string password, string repPassword) =>
                !string.IsNullOrWhiteSpace(password) &&
                !string.IsNullOrWhiteSpace(repPassword) &&
                password == repPassword;
    }
}
