using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task<ServiceResponse<bool>> SendEmail(string toEmail, string subject, string verificationCode);
    }
}
