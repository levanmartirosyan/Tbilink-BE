using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;
using Tbilink_BE.Services.Interfaces;

namespace Tbilink_BE.Services.Implementations;

internal sealed class TokenProvider : ITokenProvider
{
    private readonly IConfiguration _configuration;

    public TokenProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public (string, DateTime) CreateAccessToken(UserDTO userDTO)
    {
        string secretKey = _configuration["JwtSettings:SecretKey"]!;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpiryMinutes"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userDTO.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userDTO.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", userDTO.UserName ?? ""),
                new Claim(ClaimTypes.Role, userDTO.Role ?? ""),
                new Claim("photo_url", userDTO.ProfilePhotoUrl ?? ""),
                new Claim("email_verified", userDTO.IsEmailVerified.ToString()),
                new Claim("first_name", userDTO.FirstName ?? ""),
                new Claim("last_name", userDTO.LastName ?? ""),
            ]),
            Expires = expiresAt,
            SigningCredentials = credentials,
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
        };


        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return (token, expiresAt);
    }

    public (string , DateTime) CreateRefreshToken()
    {
        var randomBytes = new byte[32];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var refreshTokenString = WebEncoders.Base64UrlEncode(randomBytes);

        var expirationDate = DateTime.UtcNow.AddDays(15);

        return (refreshTokenString, expirationDate);
    }
}
