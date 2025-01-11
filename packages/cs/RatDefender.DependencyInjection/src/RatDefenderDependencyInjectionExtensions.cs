using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RatDefender.Infrastructure.Persistence.DbContexts;


namespace RatDefender.DependencyInjection;

public static class RatDefenderDependencyInjectionExtensions
{
    public static IServiceCollection AddRatDefender(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddRatDefenderInfrastructure(configuration)
            .AddRatDefenderDomain(configuration)
            .AddRatDefenderApplication(configuration);
    }

    public static IServiceCollection AddRatDefenderApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<
                Application.Services.Abstractions.
                IRatDetectionService,
                Application.Services.RatDetectionService>();
        return services;
    }

    public static IServiceCollection AddRatDefenderDomain(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<
            Domain.Repositories.Abstractions.IRatDetectionRepository,
            Infrastructure.Persistence.Repositories.RatDetectionRepository
        >();

        services.AddScoped<
            Domain.Services.Abstractions.IRatDetectionService,
            Domain.Services.RatDetectionService
        >();

        return services;
    }

    public static IServiceCollection AddRatDefenderInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<RatDefenderDbContext>(o =>
        {
            o.UseSqlite(configuration.GetConnectionString("RatDefender"));
        });

        return services;
    }
}