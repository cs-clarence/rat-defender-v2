using System.Device.Pwm;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDetection.Infrastructure.Iot.Configurations;
using IotBuzzer = Iot.Device.Buzzer.Buzzer;

namespace RatDetection.Infrastructure.Iot;

public class Buzzer : IBuzzer
{
    private readonly IOptions<BuzzerOptions> _options;
    private readonly IotBuzzer _buzzer;

    public Buzzer(IOptions<BuzzerOptions> options)
    {
        _options = options;
        var channel = PwmChannel.Create(_options.Value.BuzzerPwmChip,
            _options.Value.BuzzerPwmChannel, _options.Value.BuzzerPwmFrequency,
            _options.Value.BuzzerPwmDutyPercent);

        _buzzer = new IotBuzzer(channel);
    }

    public async Task BuzzAsync(ushort tone, ushort duration,
        CancellationToken cancellationToken = default)
    {
        _buzzer.StartPlaying(tone);
        await Task.Delay(duration, cancellationToken);
        _buzzer.StopPlaying();
    }

    public async Task BuzzAsync(CancellationToken cancellationToken = default)
    {
        var opt = _options.Value;
        await BuzzAsync(opt.BuzzerTone, opt.BuzzerDuration, cancellationToken);
    }
}