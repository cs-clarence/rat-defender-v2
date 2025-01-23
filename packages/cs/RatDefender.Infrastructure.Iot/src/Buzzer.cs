using System.Device.Pwm;
using Common.HostedServices.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;
using IotBuzzer = Iot.Device.Buzzer.Buzzer;

namespace RatDefender.Infrastructure.Iot;

public class Buzzer : IBuzzer, IDisposable
{
    private readonly IotBuzzer _buzzer;

    private readonly IOptions<BuzzerOptions> _options;
    private readonly ITaskQueueHandle _taskQueue;

    public Buzzer(IOptions<BuzzerOptions> options, ITaskQueueHandle taskQueue,
        ILogger<Buzzer> logger)
    {
        _options = options;
        var opt = _options.Value;
        _taskQueue = taskQueue;
        _buzzer = new IotBuzzer(PwmChannel.Create(
            opt.PwmChipNumber, opt.PwmChannel,
            opt.PwmFrequency, opt.PwmDutyPercent));

        logger.LogInformation("{options}", options.Value);
    }


    public Task BuzzAsync(uint tone, uint duration, uint delay,
        CancellationToken cancellationToken = default)
    {
        return _taskQueue.EnqueueAsync(async () =>
        {
            await Task.Delay((int)delay, cancellationToken);
            await BuzzAsync(tone, duration, cancellationToken);
        }, cancellationToken);
    }

    public Task BuzzAsync(uint tone, uint duration,
        CancellationToken cancellationToken = default)
    {
        return BuzzAsync(tone, duration, _options.Value.BuzzDelayMs,
            cancellationToken);
    }

    public Task BuzzAsync(CancellationToken cancellationToken = default)
    {
        return BuzzAsync(_options.Value.BuzzTone,
            _options.Value.BuzzDurationMs, cancellationToken);
    }

    public void Dispose()
    {
        _buzzer.Dispose();
    }
}