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

    // private string AT_GetResponse(TimeSpan? timeout = null)
    // {
    //     string? result = null;
    //     timeout ??= TimeSpan.FromSeconds(5);
    //
    //     var sw = Stopwatch.StartNew();
    //
    //     do
    //     {
    //         result = _port.ReadExisting();
    //
    //         if (sw.Elapsed > timeout)
    //         {
    //             sw.Stop();
    //             throw new TimeoutException("Timeout when reading response");
    //         }
    //     } while (string.IsNullOrWhiteSpace(result));
    //
    //     return result;
    // }
    //
    //
    // private async Task<bool> AT_SetSmsTextMode(bool isSend = true, int mode = 1)
    // {
    //     bool ok = false;
    //     var command = $"{(isSend ? "AT+CMGF" : "AT+CMGR")}={mode}";
    //
    //     _port.WriteLine(command);
    //
    //     await Task.Delay(200);
    //
    //     var response = AT_GetResponse();
    //
    //     if (!response.Contains("OK"))
    //     {
    //         _logger.LogError("CMD {command} ERROR, Response: {response}",
    //             command, response);
    //         return false;
    //     }
    //
    //     _logger.LogDebug("CMD {command} OK, Response: {response}", command,
    //         response);
    //
    //     return true;
    // }
    //
    //
    // private async Task AT_SendSms(string phoneNumber, string message)
    // {
    //     var ok = await AT_SetSmsTextMode();
    //
    //     if (!ok)
    //     {
    //         _logger.LogError("Can't set to text mode");
    //         return;
    //     }
    //
    //     var command = $"AT+CMGS=\"{phoneNumber}\"";
    //     _port.WriteLine(command);
    //
    //     await Task.Delay(200);
    //
    //     var response = AT_GetResponse();
    //
    //     if (!response.Contains("OK"))
    //     {
    //         _logger?.LogError($"Can't set phone number, Response: {response}");
    //         return;
    //     }
    //
    //     _logger.LogDebug($"Set phone number, Response: {response}");
    //
    //     _port.WriteLine(message);
    //     await Task.Delay(200);
    //
    //     // CTRL+Z
    //     _port.Write([(char)26], 0, 1);
    //     await Task.Delay(200);
    //
    //     response = AT_GetResponse();
    //     if (!response.Contains("OK"))
    //     {
    //         _logger.LogError($"Can't send message, Response: {response}");
    //     }
    //
    //     _logger.LogDebug($"Send message, Response: {response}");
    // }

    private Task SendSms(ulong detectionCount, DateTimeOffset? detectedAt,
        CancellationToken cancellationToken = default)
    {
        SmsSender.Debug = true;
        var msg = _options.Value.MessageFormat
            .Replace("{{count}}", detectionCount.ToString())
            .Replace("{{time}}", detectedAt?.ToString("HH:mm:ss") ?? "");

        return _tq.EnqueueAsync(async () =>
        {
            foreach (var recipient in _options.Value.Recipients)
            {
                _logger.LogInformation("Sending SMS to {0}", recipient);
                using var smsSender = new SmsSender(
                    _options.Value.PortName,
                    recipient.CountryCode,
                    recipient.LocalNumber,
                    msg,
                    _logger
                );
                
                smsSender.Send();
                await Task.Delay(1000, cancellationToken);
            }
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