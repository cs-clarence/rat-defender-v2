using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Common.AspNetCore.StartupFilters;

public class CommonStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(
        Action<IApplicationBuilder> next
    )
    {
        return next;
    }
}
