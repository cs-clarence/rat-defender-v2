using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class SimulateDetectionCommandHandler(
    IUnitOfWork uow,
    IRatDetectionRecordsService records,
    IFoodDispenser foodDispenser,
    IDetectionNotifier notifier,
    IBuzzer buzzer) : ICommandHandler<SimulateDetectionCommand>
{
    public async ValueTask<Unit> Handle(SimulateDetectionCommand command,
        CancellationToken cancellationToken) => await uow.ExecNoTrackingAsync(
        async () =>
        {
            var count =
                await records.GetDetectionsAsync(null, null, cancellationToken);
            await buzzer.BuzzAsync(cancellationToken);
            await foodDispenser.DispenseAsync(1, cancellationToken);
            await notifier.NotifyAsync(1, DateTimeOffset.UtcNow,
                cancellationToken);
            return Unit.Value;
        }, cancellationToken);
}