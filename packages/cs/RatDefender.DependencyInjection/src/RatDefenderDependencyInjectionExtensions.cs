using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Configurations;
using RatDefender.Domain.Services;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.HostedServices;
using RatDefender.Infrastructure.ImageRecognition.ImageProcessing;
using RatDefender.Infrastructure.ImageRecognition.ImageProcessing.Mocks;
using RatDefender.Infrastructure.Persistence.DbContexts;
using RatDetection.Infrastructure.Iot;
using RatDetection.Infrastructure.Iot.Configurations;
using RatDetection.Infrastructure.Iot.Mocks;


namespace RatDefender.DependencyInjection;

public static class RatDefenderDependencyInjectionExtensions
{
    public static IServiceCollection AddRatDefender(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddRatDefenderHostedServices(configuration)
            .AddRatDefenderInfrastructure(configuration)
            .AddRatDefenderDomain(configuration)
            .AddRatDefenderApplication(configuration);
    }

    public static IServiceCollection AddRatDefenderApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public static IServiceCollection AddRatDefenderDomain(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<IValidateOptions<RatDetectorOptions>,
                RatDetectorOptionsValidator>();
        services
            .AddOptions<RatDetectorOptions>()
            .BindConfiguration(RatDetectorOptions.DefaultKey)
            .ValidateOnStart();

        services.AddScoped<
            Domain.Repositories.Abstractions.IRatDetectionRepository,
            Infrastructure.Persistence.Repositories.RatDetectionRepository
        >();

        services.AddScoped<
            IRatDetectionRecordsService,
            RatDetectionRecordsRecordsService
        >();

        services.AddScoped<
            IRatDetector,
            RatDetector
        >();


        return services;
    }

    public static IServiceCollection AddRatDefenderHostedServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<RatDetectionBackgroundService>();

        return services;
    }

    public static IServiceCollection AddRatDefenderInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<RatDefenderDbContext>(
            o =>
            {
                o.UseSqlite(configuration.GetConnectionString("RatDefender"));
            });

        services
            .AddSingleton<IValidateOptions<ThermalImagerOptions>,
                ThermalImagerOptionsValidator>();
        services.AddOptions<ThermalImagerOptions>()
            .BindConfiguration(ThermalImagerOptions.DefaultKey)
            .ValidateOnStart();

        var mock = configuration.GetSection(MockOptions.DefaultKey)
            .Get<MockOptions>() ?? new MockOptions();
        
        if (mock.ThermalImager)
        {
            services.AddSingleton<IThermalImager, MockThermalImager>();
        }
        else
        {
            services.AddSingleton<IThermalImager, ThermalImager>();
        }

        if (mock.FoodDispenser)
        {
            services.AddSingleton<IFoodDispenser, MockFoodDispenser>();
        }
        else
        {
            services.AddSingleton<IFoodDispenser, FoodDispenser>();
        }

        if (mock.Buzzer)
        {
            services.AddSingleton<IBuzzer, MockBuzzer>();
        }
        else
        {
            services.AddSingleton<IBuzzer, Buzzer>();
        }

        if (mock.RatDetectionImageProcessor)
        {
            services.AddSingleton<IRatDetectionImageProcessor,
                MockRatDetectionImageProcessor>();
        }
        else
        {
            services.AddSingleton<IRatDetectionImageProcessor,
                RatDetectionImageProcessor>();
        }
        services.AddSingleton<ImageHolder>();

        return services;
    }
}