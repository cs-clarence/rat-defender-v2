using Mediator;
using RatDefender.Application.Dtos;
using RatDefender.Application.Queries;
using RatDefender.Domain.Services.Abstractions;
using UnitsNet;
using UnitsNet.Units;

namespace RatDefender.Application.QueryHandlers;

public class GetThermalImagerReadingsDegreeCelsiusQueryHandler(
    IThermalImager thermalImager) : IQueryHandler<
    GetThermalImagerReadingsDegreeCelsiusQuery, ThermalImageDto>
{
    private static float[][] ConvertToDegreesCelsius(Temperature[,] image)
    {
        var result = new float[image.GetLength(0)][];

        for (var y = 0; y < image.GetLength(1); y++)
        {
            result[y] = new float[image.GetLength(0)];
            for (var x = 0; x < image.GetLength(0); x++)
            {
                result[y][x] = (float)image[x, y].DegreesCelsius;
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
            TemperatureUnit.DegreeCelsius.ToString());
    }
}