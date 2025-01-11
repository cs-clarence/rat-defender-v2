using Common.Application.Mappers;
using Common.AspNetCore.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using RatDefender.Application.Services.Abstractions;

namespace RatDefender.AspNetCore.Api;

public static class RatDefenderApi
{
    public static IEndpointRouteBuilder MapRatDefenderApi(
        this IEndpointRouteBuilder routes)
    {
        var detectionsGroup = routes.MapGroup("/detections");

        detectionsGroup.MapPost("",
            async (IRatDetectionService svc, CancellationToken ct) =>
            {
                var response = await svc.RecordDetectionAsync(ct);
                return Responses.Data(response);
            });


        detectionsGroup.MapGet("",
            async (IRatDetectionService svc, string? from,
                string? to,
                string? day,
                CancellationToken ct) =>
            {
                if (day is not null)
                {
                    var date = day.MapToDateOnly();
                    return Responses.Data(await svc.GetDetectionsByDayAsync(
                        date,
                        ct));
                }

                return Responses.Data(await svc.GetDetectionsAsync(
                    from?.MapToDateTimeOffset(),
                    to?.MapToDateTimeOffset(),
                    ct));
            }
        );

        detectionsGroup.MapGet("/daily-summaries",
            async (IRatDetectionService svc, string? from,
                string? to,
                CancellationToken ct) => Responses.Data(
                await svc.GetDetectionsDailySummaryAsync(
                    from?.MapToDateTimeOffset(),
                    to?.MapToDateTimeOffset(),
                    ct)));


        return routes;
    }
}