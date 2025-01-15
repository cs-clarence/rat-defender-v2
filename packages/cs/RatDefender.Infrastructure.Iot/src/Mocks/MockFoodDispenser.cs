using Microsoft.Extensions.Logging;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Infrastructure.Iot.Mocks;

public class MockFoodDispenser(ILogger<MockFoodDispenser> logger)
    : IFoodDispenser
{
    public async Task DispenseAsync(ulong servings = 0,
        CancellationToken cancellationToken = default)
    {
        while (servings > 0)
        {
            logger.LogInformation("Dispensing food");
            servings--;
            await Task.Delay(1000, cancellationToken);
        }
    }
}