using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;

        public AdminService(IUserRepository userRepository, IAdminRepository adminRepository)
        {
            _userRepository = userRepository;
            _adminRepository = adminRepository;
        }

        #region Admin Methods
        public async Task<ServiceResponse<bool>> IsUserAdmin(int userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return ServiceResponse<bool>.Fail(false, "User not found.", 404);
            }

            bool isAdmin = user.Role == "Admin" || user.Role == "Owner";

            return ServiceResponse<bool>.Success(isAdmin, "User role retrieved successfully.");
        }

        public async Task<ServiceResponse<PaginatedResponse<UserDTO>>> GetAllUsersPaginatedAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            if (userId < 0)
            {
                return ServiceResponse<PaginatedResponse<UserDTO>>.Fail(null, "Invalid user ID.", 400);
            }

            var requestingUser = await _userRepository.GetUserById(userId);

            if (requestingUser == null)
            {
                return ServiceResponse<PaginatedResponse<UserDTO>>.Fail(null, "Requesting user not found.", 404);
            }

            if (requestingUser.Role != "Admin" && requestingUser.Role != "Owner")
            {
                return ServiceResponse<PaginatedResponse<UserDTO>>.Fail(null, "Unauthorized access. Admin privileges required.", 403);
            }

            var users = await _adminRepository.GetAllUsers(pageNumber, pageSize);
            var totalCount = await _adminRepository.GetTotalUserCount();

            var userDtos = users.Select(u => new UserDTO
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Email = u.Email,
                ProfilePhotoUrl = u.ProfilePhotoUrl,
                Role = u.Role,
                IsEmailVerified = u.IsEmailVerified
            }).ToList();

            var paginated = new PaginatedResponse<UserDTO>
            {
                Data = userDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return ServiceResponse<PaginatedResponse<UserDTO>>.Success(paginated, "Users retrieved successfully.");
        }
        #endregion
    }
}
