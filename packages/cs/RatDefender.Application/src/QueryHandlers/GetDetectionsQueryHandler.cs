using Common.Application.Mappers;
using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Mediator;
using RatDefender.Application.Dtos;
using RatDefender.Application.Mappers;
using RatDefender.Application.Queries;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.QueryHandlers;

public class GetDetectionsQueryHandler(
    IRatDetectionRecordsService records,
    IUnitOfWork uow)
    : IQueryHandler<GetDetectionsQuery, ICollection<RatDetectionDto>>
{
    public async ValueTask<ICollection<RatDetectionDto>> Handle(
        GetDetectionsQuery query, CancellationToken cancellationToken
    ) => await uow.ExecNoTrackingAsync(async () =>
    {
        var response =
            await records.GetDetectionsAsync(
                query.From?.MapToDateTimeOffset(),
                query.To?.MapToDateTimeOffset(),
                cancellationToken);
        return response.MapToDto().ToList();
    }, cancellationToken);
}