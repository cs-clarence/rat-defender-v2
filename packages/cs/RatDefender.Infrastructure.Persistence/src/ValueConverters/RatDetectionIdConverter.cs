using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RatDefender.Domain;
using RatDetectionId = RatDefender.Domain.Entities.RatDetectionId;

namespace RatDefender.Infrastructure.Persistence.ValueConverters;

public class RatDetectionIdConverter() : ValueConverter<RatDetectionId, int>(
    v => v.Value,
    v => new RatDetectionId(v)
);