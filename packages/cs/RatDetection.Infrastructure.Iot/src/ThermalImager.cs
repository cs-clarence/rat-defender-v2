using System.Device.I2c;
using Iot.Device.Amg88xx;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDetection.Infrastructure.Iot.Configurations;

namespace RatDetection.Infrastructure.Iot;

public class ThermalImager : IThermalImager
{
    private readonly Amg88xx _thermalSensor;

    public ThermalImager(IOptions<ThermalImagerOptions> options)
    {
        var val = options.Value;
        var i2C =
            I2cDevice.Create(
                new I2cConnectionSettings(val.BusId,
                    val.DeviceAddress ?? Amg88xx.DefaultI2cAddress));
        _thermalSensor = new Amg88xx(i2C);
    }

    public Task<ThermalImagerReading> ReadImageAsync(
        CancellationToken ct = default)
    {
        return Task.FromResult(
            new ThermalImagerReading(_thermalSensor.TemperatureImage));
    }
}