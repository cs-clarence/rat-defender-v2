using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatDefender.Domain.Services.Abstractions;
using RatDefender.Infrastructure.Iot.Configurations;
using RatDefender.Infrastructure.Iot.HttpClients;

namespace RatDefender.Infrastructure.Iot;

public class DetectionSmsNotifier(
    ItexmoClient client,
    IOptions<DetectionSmsNotifierOptions> options,
    ILogger<DetectionSmsNotifier> logger) : IDetectionNotifier
{
    private async Task SendSms(ulong detectionCount, DateTimeOffset? detectedAt, CancellationToken cancellationToken = default)
    {
        detectedAt ??= DateTimeOffset.UtcNow;
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
    }

    public Task NotifyAsync(ulong detectionCount = 1, DateTimeOffset? detectedAt = null,
        CancellationToken cancellationToken = default)
    {
        return SendSms(detectionCount, detectedAt, cancellationToken);
    }
}