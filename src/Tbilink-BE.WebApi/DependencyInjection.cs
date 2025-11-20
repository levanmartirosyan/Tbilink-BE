using Microsoft.OpenApi.Models;
using Tbilink_BE.Models;

namespace Tbilink_BE.WebApi
{
    public static class DependencyInjection
    {
        public static void AddWebApiServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddAuthentication();

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddHttpContextAccessor();

            var origins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AppCorsPolicy", policy =>
                {
                    policy
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddSwaggerGen(options =>
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "To authorize enter token like in this scheme: \" bearer {token} \"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                })
            );
        }
    }
}