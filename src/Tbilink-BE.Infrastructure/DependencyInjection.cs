using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tbilink_BE.Application.Common;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Data;
using Tbilink_BE.Infrastructure.Repositories;

namespace Tbilink_BE.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastuctureServices(this IHostApplicationBuilder builder)
        {
            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Tbilink-BE.Infrastructure")
                );
            });

            //builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // MediatR
            //builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();

            builder.Services.AddSignalR();

            var jwtSettings = builder.Configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings section not found in configuration");

            if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JwtSettings.SecretKey is missing or empty. Please configure a valid JWT secret key in appsettings.json or user secrets.");
            }

            if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            {
                throw new InvalidOperationException("JwtSettings.Issuer is missing or empty.");
            }

            if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            {
                throw new InvalidOperationException("JwtSettings.Audience is missing or empty.");
            }

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.ClaimsIssuer = jwtSettings.Issuer;
                    options.Audience = jwtSettings.Audience;
                    options.MapInboundClaims = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hubs")))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }

}
