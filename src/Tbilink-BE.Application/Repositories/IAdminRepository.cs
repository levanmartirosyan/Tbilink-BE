using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Domain.Entities;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Repositories
{
    public interface IAdminRepository
    {
        Task<List<User>> GetAllUsers(int pageNumber, int pageSize);
        Task<int> GetTotalUserCount();
        Task<int> GetTotalPostCount();
        Task<int> GetTotalCommentCount();

        Task<UserBan?> GetActiveBanAsync(int userId);
        Task<List<UserBan>> GetBanHistoryAsync(int userId);
        Task BanUserAsync(UserBan ban);
        Task UnbanUserAsync(int userId);

        Task<PaginatedResponse<UserBan>> GetAllBansPaginated(int pageNumber, int pageSize);
        Task<PaginatedResponse<AdminCommentDTO>> GetAllCommentsPaginated(int pageNumber, int pageSize);
    }
}
