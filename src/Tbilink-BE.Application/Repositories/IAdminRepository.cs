using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Repositories
{
    public interface IAdminRepository
    {
        Task<List<User>> GetAllUsers(int pageNumber, int pageSize);
        Task<int> GetTotalUserCount();
    }
}
