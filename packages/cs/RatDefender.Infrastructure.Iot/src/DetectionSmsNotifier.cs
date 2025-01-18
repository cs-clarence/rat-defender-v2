using Common.HostedServices.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;
using RatDefender.Infrastructure.Iot.HttpClients;

namespace RatDefender.Infrastructure.Iot;

public class DetectionSmsNotifier(
    ItexmoClient client,
    IOptions<DetectionSmsNotifierOptions> options,
    ITaskQueueHandle tq,
    ILogger<DetectionSmsNotifier> logger) : IDetectionNotifier
{
    private Task SendSms(ulong detectionCount, DateTimeOffset? detectedAt,
        CancellationToken cancellationToken = default)
    {
        return tq.EnqueueAsync(async () =>
        {
            detectedAt ??= DateTimeOffset.UtcNow;
            var tz =
                TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            detectedAt = TimeZoneInfo.ConvertTime(detectedAt.Value, tz);
            var opt = options.Value;
            var message = opt.MessageFormat
                .Replace("{count}", detectionCount.ToString())
                .Replace("{time}", detectedAt.Value.ToString("O"));
            try
            {
                await client.SendSmsAsync(new Sms
                {
                    ApiCode = opt.ApiCode,
                    Email = opt.Email,
                    Message = message,
                    Recipients = opt.Recipients,
                    Password = opt.Password
                }, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while sending sms");
            }
        }, cancellationToken);
    }

    public Task NotifyAsync(ulong detectionCount = 1,
        DateTimeOffset? detectedAt = null,
        CancellationToken cancellationToken = default)
    {
        return SendSms(detectionCount, detectedAt, cancellationToken);
    }
}