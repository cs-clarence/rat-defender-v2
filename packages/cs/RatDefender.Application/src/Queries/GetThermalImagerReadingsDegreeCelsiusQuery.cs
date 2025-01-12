using Mediator;
using RatDefender.Application.Dtos;

namespace RatDefender.Application.Queries;

public record GetThermalImagerReadingsDegreeCelsiusQuery() : IQuery<ThermalImageDto>
{
    public static GetThermalImagerReadingsDegreeCelsiusQuery Instance { get; } = new();
}