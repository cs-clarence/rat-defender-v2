using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RatDefender.Application.Commands;
using RatDefender.Application.Queries;
using RatDefender.Application.Dtos;

namespace RatDefender.AspNetCore.Api;

public static class RatDefenderApi
{
    public static IEndpointRouteBuilder MapRatDefenderApi(
        this IEndpointRouteBuilder routes)
    {
        var detectionsGroup = routes.MapGroup("/detections")
            .WithTags("Detections");

        detectionsGroup.MapPost("", RecordDetection)
            .WithName(nameof(RecordDetection));

        detectionsGroup.MapGet("", GetDetections)
            .WithName(nameof(GetDetections));

        detectionsGroup.MapGet("/daily-summaries", GetDailySummaries)
            .WithName(nameof(GetDailySummaries));

        return routes;
    }

    public static async Task<ICollection<RatDetectionDaySummaryDto>>
        GetDailySummaries(
            this IMediator mediator,
            string? from = null,
            string? to = null
        )
    {
        return await mediator.Send(new GetDailySummariesQuery(from, to));
    }

    public static async Task<RatDetectionDto> RecordDetection(
        this IMediator mediator
    )
    {
        return await mediator.Send(RecordDetectionCommand.Instance);
    }

    public static async Task<ICollection<RatDetectionDto>> GetDetections(
        this IMediator mediator,
        string? from = null,
        string? to = null
    )
    {
        return await mediator.Send(new GetDetectionsQuery(from, to));
    }
}