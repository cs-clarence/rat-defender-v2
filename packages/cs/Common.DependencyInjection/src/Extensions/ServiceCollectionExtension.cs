using System.Threading.Channels;
using Common.Application.Authentication.Abstractions;
using Common.AspNetCore.Authentication;
using Common.HostedServices;
using Common.HostedServices.Abstractions;
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

    public static IServiceCollection AddTaskQueueBackgroundService(
        this IServiceCollection services)
    {
        return services.AddSingleton<TaskQueueChannel>(_ =>
                new TaskQueueChannel(
                    Channel.CreateUnbounded<TaskQueueItem>()))
            .AddSingleton<ITaskQueueHandle, TaskQueueHandle>()
            .AddHostedService<TaskQueueBackgroundService>();
    }
}