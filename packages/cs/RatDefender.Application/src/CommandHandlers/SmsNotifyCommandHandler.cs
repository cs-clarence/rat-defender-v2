using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class SmsNotifyCommandHandler(IDetectionNotifier notifier)
    : ICommandHandler<NotifyDetectionCommand>
{
    public async ValueTask<Unit> Handle(NotifyDetectionCommand detectionCommand,
        CancellationToken cancellationToken)
    {
        await notifier.NotifyAsync(detectionCommand.DetectionCount,
            DateTimeOffset.UtcNow, cancellationToken);

        return Unit.Value;
    }
}