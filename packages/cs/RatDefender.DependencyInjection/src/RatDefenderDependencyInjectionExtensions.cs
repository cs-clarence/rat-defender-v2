using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Configurations;
using RatDefender.Domain.Services;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.HostedServices;
using RatDefender.Infrastructure.ImageRecognition;
using RatDefender.Infrastructure.ImageRecognition.Configurations;
using RatDefender.Infrastructure.ImageRecognition.Mocks;
using RatDefender.Infrastructure.Iot;
using RatDefender.Infrastructure.Iot.Configurations;
using RatDefender.Infrastructure.Iot.HttpClients;
using RatDefender.Infrastructure.Iot.Mocks;
using RatDefender.Infrastructure.Persistence.DbContexts;


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

        services
            .AddSingleton<IValidateOptions<DetectionSmsNotifierOptions>,
                DetectionSmsNotifierOptionsValidator>();

        services.AddOptions<DetectionSmsNotifierOptions>()
            .BindConfiguration(DetectionSmsNotifierOptions.DefaultKey)
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

        if (mock.DetectionNotifier)
        {
            services
                .AddSingleton<IDetectionNotifier, MockDetectionSmsNotifier>();
        }
        else
        {
            services.AddHttpClient<ItexmoClient>(
                o => { o.BaseAddress = new Uri("https://api.itexmo.com"); });
            services.AddSingleton<IDetectionNotifier, DetectionSmsNotifier>();
        }

        services.AddSingleton<ImageHolder>();

        return services;
    }
}