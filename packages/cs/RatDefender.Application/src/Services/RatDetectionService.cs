using Common.Application.UnitOfWork.Abstractions;
using RatDefender.Application.Dtos;
using RatDefender.Application.Mappers;
using RatDefender.Domain.Entities;
using IDomainRatDefenderService =
    RatDefender.Domain.Services.Abstractions.IRatDetectionService;
using IRatDetectionService =
    RatDefender.Application.Services.Abstractions.IRatDetectionService;

namespace RatDefender.Application.Services;

public class RatDetectionService(
    IDomainRatDefenderService domain,
    IUnitOfWork uow)
    : IRatDetectionService
{
    public async Task<RatDetectionDto> RecordDetectionAsync(
        CancellationToken cancellationToken = default)
        => await uow.ExecNoTrackingAsync(async () =>
        {
            var response = await domain.RecordDetectionAsync(cancellationToken);
            
            await uow.SaveChangesAsync(cancellationToken);
            
            return response.MapToDto();
        }, cancellationToken);

    public async Task<ICollection<RatDetectionDto>> GetDetectionsAsync(
        DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
        => await uow.ExecNoTrackingAsync(async () =>
        {
            var response =
                await domain.GetDetectionsAsync(from, to, cancellationToken);
            return response.MapToDto().ToList();
        }, cancellationToken);

    public async Task<ICollection<RatDetectionDaySummaryDto>>
        GetDetectionsDailySummaryAsync(DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            CancellationToken cancellationToken = default)
        => await uow.ExecNoTrackingAsync(async () =>
        {
            var response =
                await domain.GetDetectionsDailySummaryAsync(from, to,
                    cancellationToken);
            return response.MapToDto().ToList();
        }, cancellationToken);

    public async Task<ICollection<RatDetectionDto>> GetDetectionsByDayAsync(
        DateOnly day,
        CancellationToken cancellationToken = default)
        => await uow.ExecNoTrackingAsync(async () =>
            (await domain.GetDetectionsByDayAsync(day, cancellationToken))
            .MapToDto().ToList(), cancellationToken);

    public async Task<RatDetectionDto> GetDetectionByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await uow.ExecNoTrackingAsync(async () =>
            (await domain.GetDetectionByIdAsync(new RatDetectionId(id),
                cancellationToken))
            .MapToDto(), cancellationToken);

    public async Task<RatDetectionDto?> FindDetectionByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await uow.ExecNoTrackingAsync(async () =>
        {
            var resp = await domain.FindDetectionByIdAsync(
                new RatDetectionId(id),
                cancellationToken);

            return resp?.MapToDto();
        }, cancellationToken);
}