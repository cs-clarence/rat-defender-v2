using System.Device.I2c;
using Iot.Device.Amg88xx;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;

namespace RatDefender.Infrastructure.Iot;

public class ThermalImager : IThermalImager
{
    private readonly Amg88xx _thermalSensor;
    private readonly ILogger<ThermalImager> _logger;

    public ThermalImager(IOptions<ThermalImagerOptions> options,
        ILogger<ThermalImager> logger)
    {
        var val = options.Value;
        var addr = val.DeviceAddress ?? Amg88xx.AlternativeI2cAddress;
        var i2C =
            I2cDevice.Create(
                new I2cConnectionSettings(val.BusId,
                    addr));
        logger.LogInformation(
            "ThermalImager: I2C Address: {address}, BusId: {busId}", addr,
            val.BusId);

        _logger = logger;
        _thermalSensor = new Amg88xx(i2C);
        _thermalSensor.FrameRate = FrameRate.Rate10FramesPerSecond;
    }

    public Task<ThermalImagerReading> ReadImageAsync(
        CancellationToken ct = default)
    {
        _thermalSensor.ReadImage();
        return Task.FromResult(
            new ThermalImagerReading(_thermalSensor.TemperatureImage));
    }
}