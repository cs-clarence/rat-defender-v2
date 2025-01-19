using System.Runtime.CompilerServices;
using Common.AspNetCore.Dtos.Responses;
using Common.AspNetCore.Responses;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RatDefender.Application.Commands;
using RatDefender.Application.Queries;
using RatDefender.Application.Dtos;
using RatDefender.AspNetCore.Results;

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

        routes.MapPost("/food-dispenser/dispense", DispenseFood)
            .WithName(nameof(DispenseFood)).WithTags("FoodDispenser");

        routes.MapPost("/buzzer/buzz", Buzz)
            .WithName(nameof(Buzz)).WithTags("Buzzer");

        routes.MapPost("/notifier/notify-detection", NotifyDetection)
            .WithName(nameof(NotifyDetection)).WithTags("Notifier");

        routes.MapGet("/images/current.jpeg", GetCurrentImageJpeg)
            .WithName(nameof(GetCurrentImageJpeg)).WithTags("Images");

        routes.MapGet("/images/current.mjpeg", GetCurrentImageMotionJpeg)
            .WithName(nameof(GetCurrentImageMotionJpeg)).WithTags("Images");

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

    public static async Task<SuccessResponse> DispenseFood(
        this IMediator mediator,
        DispenseCommand command
    )
    {
        await mediator.Send(command);

        return Responses.Success();
    }

    public static async Task<SuccessResponse> Buzz(
        this IMediator mediator,
        BuzzCommand command
    )
    {
        await mediator.Send(command);

        return Responses.Success();
    }

    public static async Task<SuccessResponse> NotifyDetection(
        this IMediator mediator,
        NotifyDetectionCommand command
    )
    {
        await mediator.Send(command);

        return Responses.Success();
    }

    public static async Task<FileContentHttpResult> GetCurrentImageJpeg(
        this IMediator mediator
    )
    {
        var img = await mediator.Send(GetCurrentImageQuery.Instance);
        return TypedResults.File(img.Buffer, "image/jpeg");
    }

    private static async IAsyncEnumerable<byte[]> MapCurrentImageStream(
        IAsyncEnumerable<ImageDto> stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in stream.WithCancellation(cancellationToken))
        {
            yield return item.Buffer;
            if (cancellationToken.IsCancellationRequested) break;
        }
    }

    public static Task<MotionJpegResult> GetCurrentImageMotionJpeg(
        this IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        var stream = mediator.CreateStream(GetCurrentImageStreamQuery.Instance,
            cancellationToken);
        return Task.FromResult(
            new MotionJpegResult(
                MapCurrentImageStream(stream, cancellationToken),
                cancellationToken));
    }
}