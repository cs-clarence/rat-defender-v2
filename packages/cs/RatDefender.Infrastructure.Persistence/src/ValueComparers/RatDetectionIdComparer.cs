using Microsoft.EntityFrameworkCore.ChangeTracking;
using RatDefender.Domain;
using RatDetectionId = RatDefender.Domain.Entities.RatDetectionId;

namespace RatDefender.Infrastructure.Persistence.ValueComparers;

public class RatDetectionIdComparer() : ValueComparer<RatDetectionId>(
    (v1, v2) => v1.Value == v2.Value,
    v => v.Value
);