using System.Security.Claims;

namespace Common.Application.Authorization;

public class AuthorizationHandlerContext
{
    public ClaimsPrincipal? User { get; set; }
    public object? Resource { get; set; }
}
