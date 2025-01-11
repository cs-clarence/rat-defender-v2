using Common.Application.FileHandling.Abstractions;
using Common.S3.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.S3;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddS3(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<S3FileOperationUrlProviderOptions>()
            .Bind(
                configuration.GetRequiredSection(
                    S3FileOperationUrlProviderOptions.DefaultKey
                )
            )
            .ValidateOnStart();

        services.AddSingleton<
            IFileOperationUrlProvider,
            S3FileOperationUrlProvider
        >();
        return services;
    }
}
