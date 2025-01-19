using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Domain.Services;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class SimulateDetectionCommandHandler(
    IUnitOfWork uow,
    IRatDetectionResultHandler handler
) : ICommandHandler<SimulateDetectionCommand>
{
    public async ValueTask<Unit> Handle(SimulateDetectionCommand command,
        CancellationToken cancellationToken) => await uow.ExecNoTrackingAsync(
        async () =>
        {
            await handler.HandleAsync(
                new DetectionResult(1, true, true, DateTimeOffset.UtcNow),
                cancellationToken
            );
            return Unit.Value;
        }, cancellationToken);
}