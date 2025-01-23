using System.IO.Ports;
using Common.HostedServices.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;

namespace RatDefender.Infrastructure.Iot;

public class SerialAdapter : IBuzzer, IFoodDispenser, IDisposable
{
    private readonly SerialPort _serialPort;

    private readonly IOptions<BuzzerOptions> _buzzerOptions;
    private readonly IOptions<FoodDispenserOptions> _dispenserOptions;
    private readonly ITaskQueueHandle _taskQueue;
    private readonly ILogger<SerialAdapter> _logger;

    public SerialAdapter(IOptions<SerialAdapterOptions> options,
        IOptions<BuzzerOptions> buzzerOptions,
        IOptions<FoodDispenserOptions> dispenserOptions,
        ILogger<SerialAdapter> logger, ITaskQueueHandle taskQueue)
    {
        _buzzerOptions = buzzerOptions;
        _dispenserOptions = dispenserOptions;
        _taskQueue = taskQueue;
        _serialPort = new SerialPort(options.Value.PortName,
            options.Value.BaudRate ?? 115200);
        _logger = logger;

        logger.LogInformation("{options}", options.Value);
    }

    private void OpenPort()
    {
        if (_serialPort.IsOpen)
        {
            return;
        }

        try
        {
            _serialPort.Open();
        }
        catch (Exception e)
        {
            throw new Exception("Unable to open serial port", e);
        }
    }

    private void ClosePort()
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }

    private void SendCommand(string command, string args)
    {
        _logger.LogInformation("SendCommand: {command}:{args}", command, args);
        OpenPort();
        _serialPort.WriteLine($"{command}:{args}");
        ClosePort();
    }

    public Task BuzzAsync(uint tone, uint duration, uint delay,
        CancellationToken cancellationToken = default)
    {
        return _taskQueue.EnqueueAsync(async () =>
        {
            await Task.Delay((int)delay, cancellationToken);
            SendCommand("BUZZ", $"{tone},{duration}");
        }, cancellationToken);
    }

    public Task BuzzAsync(uint tone, uint duration,
        CancellationToken cancellationToken = default)
    {
        return BuzzAsync(tone, duration, _buzzerOptions.Value.BuzzDelayMs,
            cancellationToken);
    }

    public Task BuzzAsync(CancellationToken cancellationToken = default)
    {
        var opt = _buzzerOptions.Value;
        return BuzzAsync(opt.BuzzTone,
            opt.BuzzDurationMs, opt.BuzzDelayMs, cancellationToken);
    }

    public Task DispenseAsync(ulong servings = 1,
        CancellationToken cancellationToken = default)
    {
        var opt = _dispenserOptions.Value;
        SendCommand("DISPENSE",
            $"{servings},{opt.ServoDispenseAngle},{opt.ServoRotationDelay},{opt.DispenseDelay}");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _serialPort.Dispose();
    }
}