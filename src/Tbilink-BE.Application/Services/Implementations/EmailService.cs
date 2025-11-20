using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<ServiceResponse<bool>> SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.User, _settings.Password),
                    EnableSsl = _settings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.User, "Tbilink"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

                return ServiceResponse<bool>.Success(true, $"Verification code send to {toEmail}");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Fail($"Email sending failed: {ex.Message}");
            }

        }
    }
}
