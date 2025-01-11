namespace Common.Application.Authorization.Abstractions;

public interface IAuthorizationHandler<TRule>
    : Microsoft.AspNetCore.Authorization.IAuthorizationHandler
    where TRule : IAuthorizationRequirement;
