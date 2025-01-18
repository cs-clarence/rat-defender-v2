using System.Device.Pwm;
using Common.HostedServices.Abstractions;
using Iot.Device.ServoMotor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;

namespace RatDefender.Infrastructure.Iot;

public class FoodDispenser : IFoodDispenser, IDisposable
{
    private readonly IOptions<FoodDispenserOptions> _options;
    private readonly ServoMotor _servoMotor;
    private readonly ITaskQueueHandle _tq;

    public FoodDispenser(IOptions<FoodDispenserOptions> options,
        ILogger<FoodDispenser> logger, ITaskQueueHandle tq)
    {
        _tq = tq;
        _options = options;
        var opt = options.Value;
        var channel = PwmChannel.Create(opt.PwmChipNumber, opt.PwmChannel,
            opt.PwmFrequency, opt.PwmDutyPercent);
        _servoMotor = new ServoMotor(channel, opt.ServoMaximumAngle,
            opt.ServoMinimumPulseWidthMicroseconds,
            opt.ServoMaximumPulseWidthMicroseconds);

        logger.LogInformation("{options}", opt);
        _servoMotor.Start();
        _servoMotor.WriteAngle(0);
    }

    public async Task DispenseAsync(ulong servings = 1,
        CancellationToken cancellationToken = default)
    {
        var opt = _options.Value;
        await _tq.EnqueueAsync(async () =>
        {
            _servoMotor.WriteAngle(0);
            await Task.Delay((int)opt.DispenseDelay, cancellationToken);

            var angle = opt.ServoDispenseAngle;
            for (var i = 0u; i < servings; i++)
            {
                _servoMotor.WriteAngle(angle);
                await Task.Delay((int)opt.ServoRotationDelay,
                    cancellationToken);
                _servoMotor.WriteAngle(0);
                await Task.Delay((int)opt.ServoRotationDelay,
                    cancellationToken);
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        _servoMotor.Stop();
        _servoMotor.Dispose();
    }
}