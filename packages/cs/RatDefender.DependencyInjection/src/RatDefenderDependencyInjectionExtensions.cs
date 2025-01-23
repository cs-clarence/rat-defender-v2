using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Configurations;
using RatDefender.Domain.Repositories.Abstractions;
using RatDefender.Domain.Services;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.HostedServices;
using RatDefender.Infrastructure.Iot;
using RatDefender.Infrastructure.Iot.Configurations;
using RatDefender.Infrastructure.Iot.HttpClients;
using RatDefender.Infrastructure.Iot.Mocks;
using RatDefender.Infrastructure.ObjectDetection;
using RatDefender.Infrastructure.ObjectDetection.Configurations;
using RatDefender.Infrastructure.ObjectDetection.Mocks;
using RatDefender.Infrastructure.Persistence.DbContexts;
using RatDefender.Infrastructure.Persistence.Repositories;


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
            IRatDetectionRepository,
            RatDetectionRepository
        >();

        services.AddScoped<
            IRatDetectionRecordsService,
            RatDetectionRecordsRecordsService
        >();

        services.AddSingleton<
            IRatDetector,
            RatDetector
        >();

        services.AddScoped<
            IRatDetectionResultHandler,
            RatDetectionResultHandler
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

        services
            .AddSingleton<IValidateOptions<RatDetectionImageProcessorOptions>,
                RatImageDetectionImageProcessorOptionsValidator>();

        services.AddOptions<ThermalImagerOptions>()
            .BindConfiguration(ThermalImagerOptions.DefaultKey)
            .ValidateOnStart();

        services
            .AddSingleton<IValidateOptions<DetectionUartSmsNotifierOptions>,
                DetectionSmsNotifierOptionsValidator>();

        services.AddOptions<DetectionUartSmsNotifierOptions>()
            .BindConfiguration(DetectionUartSmsNotifierOptions.DefaultKey)
            .ValidateOnStart();

        services.AddOptions<RatDetectionImageProcessorOptions>()
            .BindConfiguration(RatDetectionImageProcessorOptions.DefaultKey)
            .ValidateOnStart();

        services
            .AddSingleton<IValidateOptions<BuzzerOptions>,
                BuzzerOptionsValidator>();
        services.AddOptions<BuzzerOptions>()
            .BindConfiguration(BuzzerOptions.DefaultKey)
            .ValidateOnStart();

        services
            .AddSingleton<IValidateOptions<FoodDispenserOptions>,
                FoodDispenserOptionsValidator>();
        services.AddOptions<FoodDispenserOptions>()
            .BindConfiguration(FoodDispenserOptions.DefaultKey)
            .ValidateOnStart();

        var mock = configuration.GetSection(ServiceRegistrationOptions.DefaultKey)
            .Get<ServiceRegistrationOptions>() ?? new ServiceRegistrationOptions();

        if (mock.MockThermalImager)
        {
            services.AddSingleton<IThermalImager, MockThermalImager>();
        }
        else
        {
            services.AddSingleton<IThermalImager, ThermalImager>();
        }

        if (mock.MockFoodDispenser)
        {
            services.AddSingleton<IFoodDispenser, MockFoodDispenser>();
        }
        else
        {
            services.AddSingleton<IFoodDispenser, FoodDispenser>();
        }

        if (mock.MockBuzzer)
        {
            services.AddSingleton<IBuzzer, MockBuzzer>();
        }
        else
        {
            services.AddSingleton<IBuzzer, Buzzer>();
        }

        if (mock.MockRatDetectionImageProcessor)
        {
            services.AddSingleton<IRatDetectionImageProcessor,
                MockRatDetectionImageProcessor>();
        }
        else
        {
            services.AddSingleton<IRatDetectionImageProcessor,
                RatDetectionImageProcessor>();
        }

        if (mock.MockDetectionNotifier)
        {
            services
                .AddSingleton<IDetectionNotifier, MockDetectionNotifier>();
        }
        else
        {
            services
                .AddSingleton<IDetectionNotifier, DetectionUartSmsNotifier>();
        }

        services
            .AddSingleton<IValidateOptions<SerialAdapterOptions>,
                SerialAdapterOptionsValidator>();
        services.AddOptions<SerialAdapterOptions>()
            .Bind(configuration.GetSection(SerialAdapterOptions.DefaultKey));
        services.AddSingleton<SerialAdapter>();
        services.AddSingleton<IBuzzer>(p =>
            p.GetRequiredService<SerialAdapter>());
        services.AddSingleton<IFoodDispenser>(p =>
            p.GetRequiredService<SerialAdapter>());
        services.AddSingleton<ImageHolder>();
        services.AddSingleton<IImageRetriever>(p =>
            p.GetRequiredService<ImageHolder>());

        return services;
    }
}