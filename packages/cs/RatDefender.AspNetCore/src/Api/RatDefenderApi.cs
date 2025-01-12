using Common.AspNetCore.Dtos.Responses;
using Common.AspNetCore.Responses;
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

        detectionsGroup.MapDelete("/{id}", DeleteDetection)
            .WithName(nameof(DeleteDetection));

        detectionsGroup.MapDelete("", DeleteAllDetections)
            .WithName(nameof(DeleteAllDetections));

        detectionsGroup.MapPost("/simulations", SimulateDetection)
            .WithName(nameof(SimulateDetection));

        detectionsGroup.MapGet("", GetDetections)
            .WithName(nameof(GetDetections));

        detectionsGroup.MapGet("/daily-summaries", GetDailySummaries)
            .WithName(nameof(GetDailySummaries));

        var readings = routes.MapGroup("/thermal-image-readings")
            .WithTags("ThermalImageReadings");

        readings.MapGet("/degrees-celsius",
                GetThermalImagerReadingsDegreesCelsius)
            .WithName(nameof(GetThermalImagerReadingsDegreesCelsius));

        readings.MapGet("/degrees-fahrenheit",
                GetThermalImagerReadingsDegreesFahrenheit)
            .WithName(nameof(GetThermalImagerReadingsDegreesFahrenheit));

        return routes;
    }

    public static async
        Task<DataResponse<ICollection<RatDetectionDaySummaryDto>>>
        GetDailySummaries(
            this IMediator mediator,
            string? from = null,
            string? to = null
        )
    {
        return Responses.Data(
            await mediator.Send(new GetDailySummariesQuery(from, to)));
    }

    public static async Task<DataResponse<RatDetectionDto>> RecordDetection(
        this IMediator mediator
    )
    {
        return await mediator.Send(RecordDetectionCommand.Instance);
    }

    public static async Task<DataResponse<ICollection<RatDetectionDto>>>
        GetDetections(
            this IMediator mediator,
            string? from = null,
            string? to = null
        )
    {
        return Responses.Data(
            await mediator.Send(new GetDetectionsQuery(from, to)));
    }

    public static async Task<SuccessResponse> SimulateDetection(
        this IMediator mediator
    )
    {
        await mediator.Send(SimulateDetectionCommand.Instance);

        return Responses.Success();
    }

    public static async Task<SuccessResponse> DeleteDetection(
        this IMediator mediator,
        string id)
    {
        await mediator.Send(new DeleteDetectionCommand(id));
        return Responses.Success();
    }

    public static async Task<SuccessResponse> DeleteAllDetections(
        this IMediator mediator
    )
    {
        await mediator.Send(DeleteAllDetectionsCommand.Instance);
        return Responses.Success();
    }

    public static async Task<DataResponse<ThermalImageDto>>
        GetThermalImagerReadingsDegreesCelsius(
            this IMediator mediator
        )
    {
        return await mediator.Send(
            GetThermalImagerReadingsDegreeCelsiusQuery
                .Instance);
    }

    public static async Task<DataResponse<ThermalImageDto>>
        GetThermalImagerReadingsDegreesFahrenheit(
            this IMediator mediator
        )
    {
        return await mediator.Send(
            GetThermalImagerReadingsDegreeFahrenheitQuery
                .Instance);
    }
}