using Tbilink_BE.Application.DTOs;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IAdminService
    {
        public Task<ServiceResponse<bool>> IsUserAdmin(int userId);
        Task<ServiceResponse<PaginatedResponse<UserDTO>>> GetAllUsersPaginatedAsync(int userId, int pageNumber = 1, int pageSize = 20);

    }
}
