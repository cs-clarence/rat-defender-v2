using Microsoft.Extensions.Logging;
using RatDefender.Domain.Services.Abstractions;

namespace RatDetection.Infrastructure.Iot.Mocks;

public class MockFoodDispenser(ILogger<MockFoodDispenser> logger) : IFoodDispenser
{
    public async Task DispenseAsync(ulong count,
        CancellationToken cancellationToken = default)
    {
        while (count > 0)
        {
            logger.LogInformation("Dispensing food");
            count--;
            await Task.Delay(1000, cancellationToken);
        }
    }
}