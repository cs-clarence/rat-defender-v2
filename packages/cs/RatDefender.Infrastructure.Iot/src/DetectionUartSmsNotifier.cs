using System.Globalization;
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

    public DetectionUartSmsNotifier(
        IOptions<DetectionUartSmsNotifierOptions> options,
        ITaskQueueHandle tq,
        ILogger<DetectionUartSmsNotifier> logger)
    {
        _options = options;
        _tq = tq;
        _logger = logger;

        logger.LogInformation("{}", options.Value);
    }

    private static string GetResponse(SerialPort port)
    {
        string result;

        do
        {
            result = port.ReadExisting();
        } while (string.IsNullOrWhiteSpace(result));

        return result;
    }

    private async Task SendSmsAsync(string msg, string phoneNumber,
        SerialPort port)
    {
        port.Write($"AT+CMGF=1\r\n");
        await Task.Delay(200);
        var response = GetResponse(port);

        if (response.IndexOf("OK", StringComparison.OrdinalIgnoreCase) == -1)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "Got error response '{0}'.", response));
        }

        port.Write($"AT+CMGS=\"{phoneNumber}\"\r\n");

        response = GetResponse(port);
        if (response.IndexOf(">", StringComparison.OrdinalIgnoreCase) == -1)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "Got error response '{0}'.", response));
        }

        await Task.Delay(200);
        port.Write($"{msg}");
        await Task.Delay(200);
        port.Write(['\x1A'], 0, 1);

        const int timeoutMs = 30000;
        response = GetResponse(port);

        var start = DateTimeOffset.UtcNow;

        while (!(response.Contains("OK", StringComparison.OrdinalIgnoreCase) ||
                 response.Contains("+CMGS",
                     StringComparison.OrdinalIgnoreCase)) &&
               DateTimeOffset.UtcNow.Subtract(start).TotalMilliseconds <
               timeoutMs)
        {
            response = GetResponse(port);
            await Task.Delay(100);
        }

        if (DateTimeOffset.UtcNow.Subtract(start).TotalMilliseconds >=
            timeoutMs)
        {
            // Let's just log this and assume it the SMS was sent
            _logger.LogError("Timeout waiting for SMS to be sent");
        }
    }

    private Task SendSms(ulong detectionCount, DateTimeOffset? detectedAt,
        CancellationToken cancellationToken = default)
    {
        SmsSender.Debug = true;
        var msg = _options.Value.MessageFormat
            .Replace("{{count}}", detectionCount.ToString());

        if (detectedAt is not null)
        {
            // Convert time to local time
            msg = msg.Replace("{{time}}",
                detectedAt.Value.DateTime.ToLocalTime()
                    .ToString(CultureInfo.CurrentCulture));
        }

        return _tq.EnqueueAsync(async () =>
        {
            using var port = new SerialPort(_options.Value.PortName,
                (int)_options.Value.BaudRate);
            port.Open();
            foreach (var recipient in _options.Value.Recipients)
            {
                _logger.LogInformation("Sending SMS to {0}",
                    recipient.Formatted);
                // using (
                //     var smsSender = new SmsSender(
                //         _options.Value.PortName,
                //         recipient.CountryCode,
                //         recipient.LocalNumber,
                //         msg,
                //         _logger
                //     )
                // )
                // {
                //     smsSender.Send();
                // }
                try
                {
                    await SendSmsAsync(msg, recipient.Formatted, port);
                    _logger.LogInformation("SMS sent to {}",
                        recipient.Formatted);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error sending SMS");
                }

                await Task.Delay(1000, cancellationToken);
            }

            port.Close();
        }, cancellationToken);
    }

    public Task NotifyAsync(ulong detectionCount = 1,
        DateTimeOffset? detectedAt = null,
        CancellationToken cancellationToken = default)
    {
        return SendSms(detectionCount, detectedAt, cancellationToken);
    }

    // public void Dispose()
    // {
    //     _port.Close();
    // }
}