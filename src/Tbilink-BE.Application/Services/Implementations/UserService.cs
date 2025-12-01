using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ServiceResponse<User>> GetUserInfoByEmail(ClaimsPrincipal userPrincipal)
        {
            var email = userPrincipal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                        ?? userPrincipal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return ServiceResponse<User>.Fail(null, "Missing email claim in token.", 401);
            }
                
            var user = await _userRepository.GetUserByEmail(email);

            if (user == null)
            {
                return ServiceResponse<User>.Fail(null, "User not found.", 404);
            }

            return ServiceResponse<User>.Success(user, "User data retrieved successfully.");
        }

        public async Task<ServiceResponse<User>> GetUserInfoById(int userId)
        {
            var user = await _userRepository.GetUserById(userId);

            if (user == null)
            {
                return ServiceResponse<User>.Fail(null, "User not found.", 404);
            }

            return ServiceResponse<User>.Success(user, "User data retrieved successfully.");
        }
    }
}
 