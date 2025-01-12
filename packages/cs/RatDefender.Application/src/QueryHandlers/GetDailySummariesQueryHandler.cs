using Common.Application.Mappers;
using Common.Application.UnitOfWork.Abstractions;
using Common.Application.UnitOfWork.Extensions;
using Mediator;
using RatDefender.Application.Dtos;
using RatDefender.Application.Mappers;
using RatDefender.Application.Queries;
using RatDefender.Domain.Services.Abstractions;

namespace RatDefender.Application.QueryHandlers;

public class GetDailySummariesQueryHandler(
    IRatDetectionRecordsService records,
    IUnitOfWork uow)
    : IQueryHandler<GetDailySummariesQuery,
        ICollection<RatDetectionDaySummaryDto>>
{
    public async ValueTask<ICollection<RatDetectionDaySummaryDto>> Handle(
        GetDailySummariesQuery query,
        CancellationToken cancellationToken
    ) => await uow.ExecNoTrackingAsync(async () =>
    {
        
        var response =
            await records.GetDetectionsDailySummaryAsync(
                query.From?.MapToDateTimeOffset(),
                query.To?.MapToDateTimeOffset(),
                cancellationToken);
        return response.MapToDto().ToList();
        
    }, cancellationToken);
}