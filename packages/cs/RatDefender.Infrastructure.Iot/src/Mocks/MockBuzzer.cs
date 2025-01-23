using Common.HostedServices.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;

namespace RatDefender.Infrastructure.Iot.Mocks;

public class MockBuzzer(
    ILogger<MockBuzzer> logger,
    IOptions<BuzzerOptions> options,
    ITaskQueueHandle tq) : IBuzzer
{
    public Task BuzzAsync(uint tone, uint duration, uint delay,
        CancellationToken cancellationToken = default)
    {
        return tq.EnqueueAsync(async () =>
        {
            logger.LogInformation("Buzz Delay: {delay}", delay);
            logger.LogInformation("Buzzing {tone}", tone);
            await Task.Delay((int)duration, cancellationToken);
            logger.LogInformation("Buzzed for {duration}", duration);
        }, cancellationToken);
    }

    public Task BuzzAsync(uint tone, uint duration,
        CancellationToken cancellationToken = default)
    {
        return BuzzAsync(tone, duration, options.Value.BuzzDelayMs,
            cancellationToken);
    }

    public Task BuzzAsync(CancellationToken cancellationToken = default)
    {
        var opt = options.Value;
        return BuzzAsync(opt.BuzzTone, opt.BuzzDurationMs, cancellationToken);
    }
}