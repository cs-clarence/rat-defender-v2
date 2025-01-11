using System.Security.Claims;

namespace Common.Application.Authentication.Abstractions;

public interface IUserAccessor
{
    ClaimsPrincipal User { get; }
}
