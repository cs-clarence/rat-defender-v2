using System.Security.Claims;
using Common.Application.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Common.AspNetCore.Authentication;

public class HttpContextUserAccessor(IHttpContextAccessor accessor)
    : IUserAccessor
{
    public ClaimsPrincipal User
    {
        get
        {
            var httpContext = accessor.HttpContext;
            ArgumentNullException.ThrowIfNull(httpContext);

            var user = httpContext.User;
            ArgumentNullException.ThrowIfNull(user);

            return user;
        }
    }
}
