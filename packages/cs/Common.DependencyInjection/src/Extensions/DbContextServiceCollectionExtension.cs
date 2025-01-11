using System.Data.Common;
using Common.Application.UnitOfWork.Abstractions;
using Common.Infrastructure.Persistence.Connections;
using Common.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.DependencyInjection.Extensions;

internal static partial class DbLoggingExtensions
{
    [LoggerMessage(EventId = 1001, Message = "{sql}")]
    public static partial void LogSql(
        this ILogger logger,
        string sql,
        LogLevel logLevel = LogLevel.Information
    );
}

public static class DbContextServiceCollectionExtension
{
    public static IServiceCollection AddDbContextPoolWithSharedConnection<
        TContext>(
        this IServiceCollection services,
        string connectionString,
        Func<IServiceProvider, string, DbConnection> createConnection,
        Action<DbConnection, DbContextOptionsBuilder<TContext>>? action = null
    )
        where TContext : DbContext
    {
        services.AddScoped<DbContextOptions<TContext>>(provider =>
        {
            var connectionSource =
                provider.GetRequiredService<DbConnectionSource>();
            var env = provider.GetRequiredService<IHostEnvironment>();
            var connection = connectionSource[connectionString];
            var config = provider.GetRequiredService<IConfiguration>();
            var allowDevelopmentLogging = config.GetValue(
                "Logging:Database:LogQueries",
                false
            );
            var sensitiveDataLoggingEnabled = config.GetValue(
                "Logging:Database:LogWithSensitiveData",
                false
            );

            if (connection == null)
            {
                connection = createConnection(provider, connectionString);
                connectionSource[connectionString] = connection;
            }

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();

            if (env.IsDevelopment() && allowDevelopmentLogging)
            {
                var logger = provider.GetRequiredService<ILogger<TContext>>();
                optionsBuilder.LogTo((log) => logger.LogSql(log));
                if (sensitiveDataLoggingEnabled)
                {
                    optionsBuilder.EnableSensitiveDataLogging();
                }
            }

            if (action is not null)
                action(connection, optionsBuilder);

            return optionsBuilder.Options;
        });

        services.AddScoped<TContext>();
        return services;
    }

    public static IServiceCollection AddUnitOfWork(
        this IServiceCollection services
    )
    {
        return services
            .AddScoped<ActiveDbContextCollection>()
            .AddScoped<IUnitOfWork, EfUnitOfWork>();
    }

    public static IServiceCollection AddDbConnectionSource(
        this IServiceCollection services
    )
    {
        return services.Any(x => x.ServiceType == typeof(DbConnectionSource))
            ? services
            : services.AddScoped<DbConnectionSource>();
    }
}