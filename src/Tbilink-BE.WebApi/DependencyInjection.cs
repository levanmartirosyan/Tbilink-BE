using Microsoft.OpenApi.Models;
using Tbilink_BE.WebApi.signalR;

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

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
                c.SupportNonNullableReferenceTypes();
                //c.OperationFilter<SwaggerFileUploadOperationFilter>();
            });

            // Register UserTracker as singleton
            builder.Services.AddSingleton<UserTracker>();
        }
    }
}