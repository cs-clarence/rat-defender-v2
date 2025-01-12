using Microsoft.Extensions.Logging;
using RatDefender.Domain.Services.Abstractions;
using UnitsNet;
using UnitsNet.Units;

namespace RatDefender.Infrastructure.Iot.Mocks;

public class MockThermalImager(ILogger<MockThermalImager> logger)
    : IThermalImager
{
    private static Temperature CreateRandomTemperature()
    {
        var rand = new Random();
        var min = rand.Next(20, 50);

        return new Temperature(min, TemperatureUnit.DegreeCelsius);
    }

    private static Temperature[,] CreateRandomImage()
    {
        var image = new Temperature[8, 8];

        for (var x = 0; x < 8; x++)
        {
            for (var y = 0; y < 8; y++)
            {
                image[x, y] = CreateRandomTemperature();
            }
        }

        return image;
    }

    public async Task<ThermalImagerReading> ReadImageAsync(
        CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        logger.LogInformation("Reading thermal image");
        return new ThermalImagerReading(CreateRandomImage());
    }
}