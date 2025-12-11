using System.Security.Claims;
using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IUserService
    {
        public Task<ServiceResponse<User>> GetUserInfoByEmail(ClaimsPrincipal userPrincipal);
        public Task<ServiceResponse<User>> GetUserInfoById(int userId);
        public Task<ServiceResponse<User>> GetUserInfoByUsername(string username);
        public Task<ServiceResponse<string>> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto); 
        public Task<ServiceResponse<string>> UpdateUserAsync(int currentUserId, int targetUserId, UpdateUserDTO updateUserDto);
    }
}
