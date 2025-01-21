using System.IO.Ports;
using Common.HostedServices.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;
using RatDefender.Infrastructure.Iot.UartDrivers;

namespace RatDefender.Infrastructure.Iot;

public class DetectionUartSmsNotifier : IDetectionNotifier
{
    private readonly IOptions<DetectionUartSmsNotifierOptions> _options;
    private readonly ITaskQueueHandle _tq;
    private readonly ILogger<DetectionUartSmsNotifier> _logger;

    public DetectionUartSmsNotifier(IOptions<DetectionUartSmsNotifierOptions> options,
        ITaskQueueHandle tq,
        ILogger<DetectionUartSmsNotifier> logger)
    {
        _options = options;
        _tq = tq;
        _logger = logger;
        
        logger.LogInformation("{}", options.Value);
    }

    private Task SendSms(ulong detectionCount, DateTimeOffset? detectedAt,
        CancellationToken cancellationToken = default)
    {
        SmsSender.Debug = true;
        return _tq.EnqueueAsync(() =>
        {
            var msg = string.Format(_options.Value.MessageFormat,
                detectionCount,
                detectedAt?.ToString("HH:mm:ss") ?? "");

            foreach (var recipient in _options.Value.Recipients)
            {
                using var smsSender = new SmsSender(_options.Value.PortName,
                    recipient.CountryCode,
                    recipient.LocalNumber,
                    msg, _logger);

                smsSender.Send();
            }

            return ValueTask.CompletedTask;
        }, cancellationToken);
    }

    public Task NotifyAsync(ulong detectionCount = 1,
        DateTimeOffset? detectedAt = null,
        CancellationToken cancellationToken = default)
    {
        return SendSms(detectionCount, detectedAt, cancellationToken);
    }
}