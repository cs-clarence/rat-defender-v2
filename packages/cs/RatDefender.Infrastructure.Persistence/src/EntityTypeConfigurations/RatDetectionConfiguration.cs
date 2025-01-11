using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RatDefender.Domain;
using RatDefender.Domain.Entities;

namespace RatDefender.Infrastructure.Persistence.EntityTypeConfigurations;

public class RatDetectionConfiguration : IEntityTypeConfiguration<RatDetection>
{
    public void Configure(EntityTypeBuilder<RatDetection> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasAnnotation("Sqlite:Autoincrement", true);
        
        builder.Property(x => x.DetectedAt).IsRequired();
        builder.Property(x => x.Count).IsRequired();
    }
}