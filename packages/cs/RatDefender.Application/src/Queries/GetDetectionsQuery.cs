using Mediator;
using RatDefender.Application.Dtos;

namespace RatDefender.Application.Queries;

public record GetDetectionsQuery(string? From, string? To)
    : IQuery<ICollection<RatDetectionDto>>;