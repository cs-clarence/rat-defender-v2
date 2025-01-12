using Mediator;
using RatDefender.Application.Dtos;

namespace RatDefender.Application.Queries;

public record GetDailySummariesQuery(
    string? From = null,
    string? To = null
) : IQuery<ICollection<RatDetectionDaySummaryDto>>;