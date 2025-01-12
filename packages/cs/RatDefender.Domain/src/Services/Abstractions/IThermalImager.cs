using UnitsNet;

namespace RatDefender.Domain.Services.Abstractions;

public record ThermalImagerReading(Temperature[,] Image);

public interface IThermalImager
{
    public Task<ThermalImagerReading> ReadImageAsync(
        CancellationToken ct = default);
}