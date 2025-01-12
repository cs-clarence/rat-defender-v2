using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class DeleteAllDetectionsCommandHandler(
    IRatDetectionRecordsService records,
    IUnitOfWork uow) : ICommandHandler<DeleteAllDetectionsCommand>
{
    public async ValueTask<Unit> Handle(DeleteAllDetectionsCommand command,
        CancellationToken cancellationToken)
        => await uow.ExecNoTrackingAsync(async () =>
        {
            await records.DeleteAllDetectionsAsync(cancellationToken);
            return Unit.Value;
        }, cancellationToken);
}