using Common.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using RatDefender.Domain.Entities;
using RatDefender.Infrastructure.Persistence.EntityTypeConfigurations;
using RatDefender.Infrastructure.Persistence.ValueComparers;
using RatDefender.Infrastructure.Persistence.ValueConverters;

namespace RatDefender.Infrastructure.Persistence.DbContexts;

public class RatDefenderDbContext(
    DbContextOptions<RatDefenderDbContext> options)
    : BaseDbContexts(options)
{
    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<RatDetectionId>()
            .HaveConversion<RatDetectionIdConverter, RatDetectionIdComparer>();

        if (Database.ProviderName?.Contains(
                "Microsoft.EntityFrameworkCore.Sqlite") == true)
        {
        }

        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new RatDetectionConfiguration().Configure(modelBuilder
            .Entity<RatDetection>());
        base.OnModelCreating(modelBuilder);
    }
}