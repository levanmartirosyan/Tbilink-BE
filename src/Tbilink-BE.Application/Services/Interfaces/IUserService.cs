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
        public Task<ServiceResponse<string>> RemoveUser(int currentUserId, int userId);

        Task<ServiceResponse<string>> ChangePasswordAsync(int userId, ChangePasswordDTO dto);

        public Task<ServiceResponse<string>> ToggleFollowUserAsync(int currentUserId, int targetUserId);
        public Task<ServiceResponse<List<UserFollowDTO>>> GetFollowersAsync(int userId, int? currentUserId = null);
        public Task<ServiceResponse<List<UserFollowDTO>>> GetFollowingAsync(int userId, int? currentUserId = null);
        public Task<ServiceResponse<List<UserFollowDTO>>> GetMutualFollowsAsync(int userId);
        public Task<ServiceResponse<FollowStatsDTO>> GetFollowStatsAsync(int userId, int? currentUserId = null);

        public Task<ServiceResponse<SearchResultDTO>> SearchAsync(SearchRequestDTO searchRequest, int? currentUserId = null);
    }
}
