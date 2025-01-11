using Common.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Common.AspNetCore.Extensions;

public static class MiddlewareExtension
{
    public static IApplicationBuilder UseDomainExceptionHandler(
        this IApplicationBuilder builder
    )
    {
        return builder.UseMiddleware<DomainExceptionHandlerMiddleware>();
    }
}
