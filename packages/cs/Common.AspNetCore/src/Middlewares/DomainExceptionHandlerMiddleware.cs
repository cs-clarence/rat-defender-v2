using System.Net.Mime;
using Common.AspNetCore.Dtos.Responses;
using Common.Domain.Exceptions.Abstractions;
using Humanizer;
using Microsoft.AspNetCore.Http;

namespace Common.AspNetCore.Middlewares;

/// <summary>
///     Middleware that converts <see cref="DomainException" /> to <see cref="DomainProblemDetails" /> responses.
///     This is conditionally executed if it Accepts the "Application/Json" content type.
///     If the <see cref="IProblemDetailsService" /> is not injected, it will rethrow the exception.
/// </summary>
/// <param name="next">The next middleware in the pipeline.</param>
public class DomainExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task Invoke(
        HttpContext context,
        IProblemDetailsService? problemDetailsService = null
    )
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception) when (!context.Response.HasStarted)
        {
            var response = context.Response;
            var request = context.Request;

            response.StatusCode = exception switch
            {
                IEntityNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest,
            };

            if (
                problemDetailsService != null
                && request.Headers.Accept.Any(i =>
                    i == MediaTypeNames.Application.Json
                )
            )
            {
                response.ContentType = MediaTypeNames.Application.ProblemJson;
                var domain = exception.Domain.Kebaberize();
                var typename = exception
                    .GetType()
                    .Name.Replace("Exception", "")
                    .Kebaberize();

                var link = $"/errors/{domain}/{typename}";

                var pd = new DomainProblemDetails
                {
                    Domain = domain,
                    Title = exception.Title,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = exception.Message,
                    Type = link,
                };
                switch (exception)
                {
                    case IEntityNotFoundException e:
                        pd.Extensions.Add("entity", e.Entity);
                        break;
                }

                await problemDetailsService.WriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = context,
                        Exception = exception,
                        ProblemDetails = pd,
                    }
                );
            }
            else
            {
                await response.WriteAsync(exception.Message);
            }
        }
    }
}
