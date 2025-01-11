using Common.Application.Authentication.Abstractions;
using Common.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Common.DependencyInjection.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        return services.Any(x => x.ServiceType == typeof(IUserAccessor))
            ? services
            : services.AddScoped<IUserAccessor, HttpContextUserAccessor>();
    }
}
