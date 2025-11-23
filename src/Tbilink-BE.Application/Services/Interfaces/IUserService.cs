using System.Security.Claims;
using Tbilink_BE.Models;

namespace Tbilink_BE.Application.Services.Interfaces
{
    public interface IUserService
    {
        public Task<ServiceResponse<User>> GetUserInfoByEmail(ClaimsPrincipal userPrincipal);
    }
}
