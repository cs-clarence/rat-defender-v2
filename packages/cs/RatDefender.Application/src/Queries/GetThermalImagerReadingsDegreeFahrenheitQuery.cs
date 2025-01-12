using Mediator;
using RatDefender.Application.Dtos;

namespace RatDefender.Application.Queries;

public record GetThermalImagerReadingsDegreeFahrenheitQuery()
    : IQuery<ThermalImageDto>
{
    public static GetThermalImagerReadingsDegreeFahrenheitQuery Instance
    {
        get;
    } = new();
}