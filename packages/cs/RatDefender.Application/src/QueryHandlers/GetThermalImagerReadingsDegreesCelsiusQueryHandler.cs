using Mediator;
using RatDefender.Application.Dtos;
using RatDefender.Application.Queries;
using RatDefender.Domain.Services.Abstractions;
using UnitsNet;

namespace RatDefender.Application.QueryHandlers;

public class GetThermalImagerReadingsDegreesCelsiusQueryHandler(
    IThermalImager thermalImager) : IQueryHandler<
    GetThermalImagerReadingsDegreeCelsiusQuery, ThermalImageDto>
{
    private static float[,] ConvertToDegreesCelsius(Temperature[,] image)
    {
        var result = new float[image.GetLength(0), image.GetLength(1)];

        for (var x = 0; x < image.GetLength(0); x++)
        {
            for (var y = 0; y < image.GetLength(1); y++)
            {
                result[x, y] = (float)image[x, y].DegreesCelsius;
            }
        }

        return result;
    }

    public async ValueTask<ThermalImageDto> Handle(
        GetThermalImagerReadingsDegreeCelsiusQuery query,
        CancellationToken cancellationToken)
    {
        var reading = await thermalImager.ReadImageAsync(cancellationToken);

        return new ThermalImageDto(ConvertToDegreesCelsius(reading.Image),
            "DegreesCelsius");
    }
}