using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDetection.Infrastructure.Iot.Configurations;

namespace RatDetection.Infrastructure.Iot.Mocks;

public class MockBuzzer(
    ILogger<MockBuzzer> logger,
    IOptions<BuzzerOptions> options) : IBuzzer
{
    public async Task BuzzAsync(ushort tone, ushort duration,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Buzzing {tone}", tone);
        await Task.Delay(duration, cancellationToken);
        logger.LogInformation("Buzzed for {duration}", duration);
    }

    public Task BuzzAsync(CancellationToken cancellationToken = default)
    {
        var opt = options.Value;
        return BuzzAsync(opt.BuzzerTone, opt.BuzzerDuration, cancellationToken);
    }
}