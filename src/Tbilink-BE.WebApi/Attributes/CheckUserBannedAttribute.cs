using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Tbilink_BE.Application.Repositories;

namespace Tbilink_BE.WebApi.Attributes
{
    public class CheckUserBannedAttribute : TypeFilterAttribute
    {
        public CheckUserBannedAttribute() : base(typeof(CheckUserBannedFilter))
        {
        }

        private class CheckUserBannedFilter : IAsyncActionFilter
        {
            private readonly IUserRepository _userRepository;

            public CheckUserBannedFilter(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var user = context.HttpContext.User;
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var dbUser = await _userRepository.GetUserById(userId);
                if (dbUser == null)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                if (dbUser.IsBanned)
                {
                    context.Result = new ObjectResult(new { message = "User is banned." })
                    {
                        StatusCode = 403
                    };
                    return;
                }

                await next();
            }
        }
    }
}
