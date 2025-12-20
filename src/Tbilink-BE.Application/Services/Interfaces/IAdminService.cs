using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IAdminService
    {
        public Task<ServiceResponse<bool>> IsUserAdmin(int userId);
        Task<ServiceResponse<PaginatedResponse<AdminUserDTO>>> GetAllUsersPaginatedAsync(int userId, int pageNumber = 1, int pageSize = 20);
        Task<ServiceResponse<AdminStatsDTO>> GetAdminStatsAsync();

        Task<ServiceResponse<string>> BanUserAsync(int adminId, int userId, string reason, DateTime? expiresAt = null);
        Task<ServiceResponse<string>> UnbanUserAsync(int adminId, int userId);
        Task<ServiceResponse<List<UserBan>>> GetBanHistoryAsync(int adminId, int userId);

        Task<ServiceResponse<PaginatedResponse<UserBanDTO>>> GetAllBansPaginatedAsync(int adminId, int pageNumber = 1, int pageSize = 20);
        Task<ServiceResponse<PaginatedResponse<AdminCommentDTO>>> GetAllCommentsPaginatedAsync(int adminId, int pageNumber = 1, int pageSize = 20);
    }
}
