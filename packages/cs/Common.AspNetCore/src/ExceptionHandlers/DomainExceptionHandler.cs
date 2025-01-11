using System.Net.Mime;
using Common.AspNetCore.Dtos.Responses;
using Common.Domain.Exceptions.Abstractions;
using Humanizer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Common.AspNetCore.ExceptionHandlers;

public class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not DomainException domainException)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        if (
            httpContext.Request.Headers.Accept.Any(i =>
                i == MediaTypeNames.Application.Json
            )
        )
        {
            var domain = domainException.Domain.Kebaberize();
            var typename = domainException
                .GetType()
                .Name.Replace("Exception", "")
                .Kebaberize();
            var link = $"/errors/{domain}/{typename}";
            httpContext.Response.ContentType = MediaTypeNames
                .Application
                .ProblemJson;

            await problemDetailsService.WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = domainException,
                    ProblemDetails = new DomainProblemDetails
                    {
                        Domain = domain,
                        Title = domainException.Message,
                        Status = StatusCodes.Status400BadRequest,
                        Detail = domainException.Message,
                        Type = link,
                    },
                }
            );
            return true;
        }

        return false;
    }
}
