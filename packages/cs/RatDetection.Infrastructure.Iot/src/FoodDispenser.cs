using System.Device.Pwm;
using Iot.Device.ServoMotor;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDetection.Infrastructure.Iot.Configurations;

namespace RatDetection.Infrastructure.Iot;

public class FoodDispenser : IFoodDispenser
{
    private readonly IOptions<FoodDispenserOptions> _options;
    private readonly ServoMotor _servoMotor;

    public FoodDispenser(IOptions<FoodDispenserOptions> options)
    {
        _options = options;
        var opt = options.Value;
        var channel = PwmChannel.Create(opt.ServoPwmChip, opt.ServoPwmChannel,
            opt.ServoPwmFrequency, opt.ServoPwmDutyPercent);
        _servoMotor = new ServoMotor(channel, opt.MinimumPulseWidthMicroseconds,
            opt.MaximumPulseWidthMicroseconds);
    }

    public async Task DispenseAsync(ulong count,
        CancellationToken cancellationToken = default)
    {
        var opt = _options.Value;

        _servoMotor.WriteAngle(0);
        await Task.Delay((int)opt.DispenseDelay, cancellationToken);
        
        var angle = opt.DispenseAngle;
        for (var i = 0u; i < count; i++)
        {
            _servoMotor.WriteAngle(angle);
            await Task.Delay((int)opt.RotationDelay, cancellationToken);
            _servoMotor.WriteAngle(0);
            await Task.Delay((int)opt.RotationDelay, cancellationToken);
        }
    }
}