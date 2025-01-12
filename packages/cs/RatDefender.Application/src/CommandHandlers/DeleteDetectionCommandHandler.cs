using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Domain.Entities;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class DeleteDetectionCommandHandler(IRatDetectionRecordsService records, IUnitOfWork uow) : ICommandHandler<DeleteDetectionCommand>
{
    public async ValueTask<Unit> Handle(DeleteDetectionCommand command,
        CancellationToken cancellationToken) => await uow.ExecNoTrackingAsync(async () =>
    {
        var integer = int.Parse(command.Id);
        var ratDetectionId = new RatDetectionId(integer);
        await records.DeleteDetectionAsync(ratDetectionId, cancellationToken);
        
        return Unit.Value;
    }, cancellationToken);
}