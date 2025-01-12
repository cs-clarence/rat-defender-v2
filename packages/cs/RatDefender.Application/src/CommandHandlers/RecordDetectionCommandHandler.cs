using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Mediator;
using RatDefender.Application.Commands;
using RatDefender.Application.Dtos;
using RatDefender.Application.Mappers;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.CommandHandlers;

public class RecordDetectionCommandHandler(
    IRatDetectionRecordsService records,
    IUnitOfWork uow) : ICommandHandler<RecordDetectionCommand, RatDetectionDto>
{
    public async ValueTask<RatDetectionDto> Handle(
        RecordDetectionCommand command,
        CancellationToken cancellationToken
    ) => await uow.ExecNoTrackingAsync(async () =>
    {
        
        var response =
            await records.RecordDetectionAsync(cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return response.MapToDto();
        
    }, cancellationToken);
}