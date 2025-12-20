using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Application.Repositories;
using Tbilink_BE.Application.Services.Interfaces;
using Tbilink_BE.Domain.Entities;
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

        public async Task<ServiceResponse<PaginatedResponse<AdminUserDTO>>> GetAllUsersPaginatedAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            if (userId < 0)
                return ServiceResponse<PaginatedResponse<AdminUserDTO>>.Fail(null, "Invalid user ID.", 400);

            var requestingUser = await _userRepository.GetUserById(userId);
            if (requestingUser == null)
                return ServiceResponse<PaginatedResponse<AdminUserDTO>>.Fail(null, "Requesting user not found.", 404);

            if (requestingUser.Role != "Admin" && requestingUser.Role != "Owner")
                return ServiceResponse<PaginatedResponse<AdminUserDTO>>.Fail(null, "Unauthorized access. Admin privileges required.", 403);

            var users = await _adminRepository.GetAllUsers(pageNumber, pageSize);
            var totalCount = await _adminRepository.GetTotalUserCount();

            var userDtos = users.Select(u => new AdminUserDTO
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Email = u.Email,
                Country = u.Country,
                City = u.City,
                PhoneNumber = u.PhoneNumber,
                RegisterDate = u.RegisterDate,
                IsEmailVerified = u.IsEmailVerified,
                IsRegistrationCompleted = u.IsRegistrationCompleted,
                Role = u.Role,
                ProfilePhotoUrl = u.ProfilePhotoUrl,
                CoverPhotoUrl = u.CoverPhotoUrl,
                Description = u.Description,
                IsPublicProfile = u.IsPublicProfile,
                ShowEmail = u.ShowEmail,
                ShowPhone = u.ShowPhone,
                AllowTagging = u.AllowTagging,
                EmailNotifications = u.EmailNotifications,
                PushNotifications = u.PushNotifications,
                SmsNotifications = u.SmsNotifications,
                MarketingEmails = u.MarketingEmails,
                Language = u.Language,
                TimeZone = u.TimeZone,
                IsOnline = u.IsOnline,
                LastActive = u.LastActive,
                FollowersCount = u.FollowersCount,
                FollowingCount = u.FollowingCount,
                PostCount = u.PostCount,
                IsBanned = u.IsBanned
            }).ToList();

            var paginated = new PaginatedResponse<AdminUserDTO>
            {
                Data = userDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return ServiceResponse<PaginatedResponse<AdminUserDTO>>.Success(paginated, "Users retrieved successfully.");
        }

        public async Task<ServiceResponse<AdminStatsDTO>> GetAdminStatsAsync()
        {
            var userCount = await _adminRepository.GetTotalUserCount();
            var postCount = await _adminRepository.GetTotalPostCount();
            var commentCount = await _adminRepository.GetTotalCommentCount();

            var stats = new AdminStatsDTO
            {
                UserCount = userCount,
                PostCount = postCount,
                CommentCount = commentCount
            };

            return ServiceResponse<AdminStatsDTO>.Success(stats, "Stats retrieved successfully.");
        }
        #endregion

        #region Ban Methods
        public async Task<ServiceResponse<string>> BanUserAsync(int adminId, int userId, string reason, DateTime? expiresAt = null)
        {
            var admin = await _userRepository.GetUserById(adminId);
            if (admin == null || (admin.Role != "Admin" && admin.Role != "Owner"))
                return ServiceResponse<string>.Fail(null, "Unauthorized", 403);

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
                return ServiceResponse<string>.Fail(null, "User not found", 404);

            var activeBan = await _adminRepository.GetActiveBanAsync(userId);
            if (activeBan != null && activeBan.IsActive)
                return ServiceResponse<string>.Fail(null, "User is already banned", 409);

            var ban = new UserBan
            {
                UserId = userId,
                Reason = reason,
                BannedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                BannedById = adminId,
                IsActive = true
            };
            await _adminRepository.BanUserAsync(ban);
            return ServiceResponse<string>.Success(null, expiresAt == null ? "User permanently banned." : $"User banned until {expiresAt}.");
        }

        public async Task<ServiceResponse<string>> UnbanUserAsync(int adminId, int userId)
        {
            var admin = await _userRepository.GetUserById(adminId);
            if (admin == null || (admin.Role != "Admin" && admin.Role != "Owner"))
                return ServiceResponse<string>.Fail(null, "Unauthorized", 403);

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
                return ServiceResponse<string>.Fail(null, "User not found", 404);

            await _adminRepository.UnbanUserAsync(userId);
            return ServiceResponse<string>.Success(null, "User unbanned.");
        }

        public async Task<ServiceResponse<List<UserBan>>> GetBanHistoryAsync(int adminId, int userId)
        {
            var admin = await _userRepository.GetUserById(adminId);
            if (admin == null || (admin.Role != "Admin" && admin.Role != "Owner"))
                return ServiceResponse<List<UserBan>>.Fail(null, "Unauthorized", 403);

            var bans = await _adminRepository.GetBanHistoryAsync(userId);
            return ServiceResponse<List<UserBan>>.Success(bans, "Ban history retrieved.");
        }

        public async Task<ServiceResponse<PaginatedResponse<UserBanDTO>>> GetAllBansPaginatedAsync(int adminId, int pageNumber = 1, int pageSize = 20)
        {
            var admin = await _userRepository.GetUserById(adminId);
            if (admin == null || (admin.Role != "Admin" && admin.Role != "Owner"))
                return ServiceResponse<PaginatedResponse<UserBanDTO>>.Fail(null, "Unauthorized", 403);

            var paginated = await _adminRepository.GetAllBansPaginated(pageNumber, pageSize);

            var dto = new PaginatedResponse<UserBanDTO>
            {
                Data = paginated.Data.Select(b => new UserBanDTO
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    UserName = b.User?.UserName,
                    Reason = b.Reason,
                    BannedAt = b.BannedAt,
                    ExpiresAt = b.ExpiresAt,
                    BannedById = b.BannedById,
                    BannedByUserName = b.BannedBy?.UserName,
                    IsActive = b.IsActive
                }).ToList(),
                PageNumber = paginated.PageNumber,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount,
                TotalPages = paginated.TotalPages
            };

            return ServiceResponse<PaginatedResponse<UserBanDTO>>.Success(dto, "Bans retrieved successfully.");
        }

        public async Task<ServiceResponse<PaginatedResponse<AdminCommentDTO>>> GetAllCommentsPaginatedAsync(int adminId, int pageNumber = 1, int pageSize = 20)
        {
            var admin = await _userRepository.GetUserById(adminId);
            if (admin == null || (admin.Role != "Admin" && admin.Role != "Owner"))
                return ServiceResponse<PaginatedResponse<AdminCommentDTO>>.Fail(null, "Unauthorized", 403);

            var paginated = await _adminRepository.GetAllCommentsPaginated(pageNumber, pageSize);
            return ServiceResponse<PaginatedResponse<AdminCommentDTO>>.Success(paginated, "Comments retrieved successfully.");
        }
        #endregion
    }
}
