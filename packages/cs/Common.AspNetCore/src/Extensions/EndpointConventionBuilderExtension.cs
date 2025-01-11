using System.Net.Mime;
using Common.AspNetCore.Dtos.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Common.AspNetCore.Extensions;

public static class EndpointConventionBuilderExtension
{
    private static TBuilder Produces<TBuilder, TType>(
        this TBuilder builder,
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null,
        params string[] contentTypes
    )
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithMetadata(
            new ProducesResponseTypeMetadata(
                statusCode,
                typeof(TType),
                [
                    contentType ?? MediaTypeNames.Application.Json,
                    .. contentTypes,
                ]
            )
        );
    }

    public static RouteGroupBuilder Produces<T>(
        this RouteGroupBuilder builder,
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null,
        params string[] contentTypes
    )
    {
        return Produces<RouteGroupBuilder, T>(
            builder,
            statusCode,
            contentType,
            contentTypes
        );
    }

    public static RouteHandlerBuilder Produces<T>(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null,
        params string[] contentTypes
    )
    {
        return Produces<RouteHandlerBuilder, T>(
            builder,
            statusCode,
            contentType,
            contentTypes
        );
    }

    private static TBuilder ProducesJson<TBuilder, TType>(
        this TBuilder builder,
        int statusCode = StatusCodes.Status200OK
    )
        where TBuilder : IEndpointConventionBuilder
    {
        return Produces<TBuilder, TType>(
            builder,
            statusCode,
            MediaTypeNames.Application.Json
        );
    }

    public static RouteGroupBuilder ProducesJson<T>(
        this RouteGroupBuilder builder,
        int statusCode = StatusCodes.Status200OK
    )
    {
        return ProducesJson<RouteGroupBuilder, T>(builder, statusCode);
    }

    public static RouteHandlerBuilder ProducesJson<T>(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status200OK
    )
    {
        return ProducesJson<RouteHandlerBuilder, T>(builder, statusCode);
    }

    private static TBuilder ProducesProblemJson<TBuilder, TType>(
        this TBuilder builder,
        int statusCode = StatusCodes.Status200OK
    )
        where TBuilder : IEndpointConventionBuilder
        where TType : ProblemDetails
    {
        return Produces<TBuilder, TType>(
            builder,
            statusCode,
            MediaTypeNames.Application.ProblemJson
        );
    }

    public static RouteGroupBuilder ProducesProblemJson<T>(
        this RouteGroupBuilder builder,
        int statusCode = StatusCodes.Status200OK
    )
        where T : ProblemDetails
    {
        return ProducesProblemJson<RouteGroupBuilder, T>(builder, statusCode);
    }

    public static RouteHandlerBuilder ProducesProblemJson<T>(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status200OK
    )
        where T : ProblemDetails
    {
        return ProducesProblemJson<RouteHandlerBuilder, T>(builder, statusCode);
    }

    public static RouteGroupBuilder ProducesDomainProblemJson(
        this RouteGroupBuilder builder,
        int statusCode = StatusCodes.Status400BadRequest
    )
    {
        return ProducesProblemJson<DomainProblemDetails>(builder, statusCode);
    }

    public static RouteHandlerBuilder ProducesDomainProblemJson(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status400BadRequest
    )
    {
        return ProducesProblemJson<DomainProblemDetails>(builder, statusCode);
    }

    public static RouteHandlerBuilder ProducesProblemJson(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status500InternalServerError
    )
    {
        return ProducesProblemJson<ProblemDetails>(builder, statusCode);
    }

    public static RouteGroupBuilder ProducesProblemJson(
        this RouteGroupBuilder builder,
        int statusCode = StatusCodes.Status500InternalServerError
    )
    {
        return ProducesProblemJson<ProblemDetails>(builder, statusCode);
    }

    private static string TrimStart(this string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString))
            return target;

        var result = target;
        while (result.StartsWith(trimString))
        {
            result = result.Substring(trimString.Length);
        }

        return result;
    }

    public static TBuilder WithMapName<TBuilder>(
        this TBuilder builder,
        string mapName
    )
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithName(mapName.TrimStart("Map"));
    }
}
