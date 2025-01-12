using System.Device.Pwm;
using Common.HostedServices.Abstractions;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;
using IotBuzzer = Iot.Device.Buzzer.Buzzer;

namespace RatDefender.Infrastructure.Iot;

public class Buzzer : IBuzzer
{
    private readonly IOptions<BuzzerOptions> _options;
    private readonly IotBuzzer _buzzer;
    private readonly ITaskQueueHandle _taskQueue;

    public Buzzer(IOptions<BuzzerOptions> options, ITaskQueueHandle taskQueue)
    {
        _options = options;
        var channel = PwmChannel.Create(_options.Value.BuzzerPwmChip,
            _options.Value.BuzzerPwmChannel, _options.Value.BuzzerPwmFrequency,
            _options.Value.BuzzerPwmDutyPercent);

        _buzzer = new IotBuzzer(channel);
        _taskQueue = taskQueue;
    }

    private async Task BuzzAsync(ushort tone, ushort duration, ulong delayMs,
        CancellationToken cancellationToken = default)
    {
        if (delayMs > 100)
        {
            await _taskQueue.EnqueueAsync(async () =>
            {
                await Task.Delay((int)delayMs, cancellationToken);
                await BuzzAsync(tone, duration, cancellationToken);
            }, cancellationToken);
        }
        else
        {
            await Task.Delay((int)delayMs, cancellationToken);
            await BuzzAsync(tone, duration, cancellationToken);
        }
    }

    public Task BuzzAsync(ushort tone, ushort duration,
        CancellationToken cancellationToken = default)
    {
        return BuzzAsync(tone, duration, _options.Value.BuzzerDelayMs,
            cancellationToken);
    }

    public Task BuzzAsync(CancellationToken cancellationToken = default)
    {
        return BuzzAsync(_options.Value.BuzzerTone,
            _options.Value.BuzzerDurationMs, cancellationToken);
    }
}