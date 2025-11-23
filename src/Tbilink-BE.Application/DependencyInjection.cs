using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tbilink_BE.Application.Services.Implementations;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;
using Tbilink_BE.Services.Implementations;
using Tbilink_BE.Services.Interfaces;

namespace Tbilink_BE.Application
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<ITokenProvider, TokenProvider>();

        }
    }
}
