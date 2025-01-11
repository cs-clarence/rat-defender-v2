using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;

namespace Server.Middleware;

public record StaticSpaOptions
{
    public string ServePath { get; set; } = "/app";
    public bool PrefixRequestPathToFilePaths { get; set; } = true;
    public string RequestPath { get; set; } = "/app";
    public string Directory { get; set; } = "./spa";
    public bool Serve { get; set; } = true;
    public IEnumerable<string> IgnorePaths { get; set; } = new List<string>();
}

abstract class StaticSpaMiddleware;

public static class StaticSpaExtensions
{
    public static IApplicationBuilder UseStaticSpa(
        this IApplicationBuilder app,
        StaticSpaOptions options
    )
    {
        var spaRequestPath = options.RequestPath;
        var prefixPath = options.PrefixRequestPathToFilePaths;
        var logger = app.ApplicationServices.GetRequiredService<
            ILogger<StaticSpaMiddleware>
        >();

        logger.LogInformation("Serving SPA on {path}", spaRequestPath);

        app.UseRewriter(
            new RewriteOptions().Add(ctx =>
            {
                foreach (var path in options.IgnorePaths)
                {
                    if (ctx.HttpContext.Request.Path.StartsWithSegments(path))
                        return;
                }
                
                var endsWithAnyExtension = Regex.IsMatch(
                    ctx.HttpContext.Request.Path,
                    @"\.\w*$"
                );

                if (!endsWithAnyExtension)
                {
                    ctx.HttpContext.Request.Path = "/index.html";
                }

                if (prefixPath)
                {
                    var spaBase = new PathString(spaRequestPath);

                    ctx.HttpContext.Request.Path = spaBase.Add(
                        ctx.HttpContext.Request.Path
                    );
                }

                ctx.Result = RuleResult.SkipRemainingRules;
            })
        );

        var spaDirectory = options.Directory;

        var absSpaDirectory = Path.GetFullPath(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                string.IsNullOrEmpty(spaDirectory) ? "./spa" : spaDirectory
            )
        );

        logger.LogInformation("SPA Directory: {dir}", absSpaDirectory);

        var directoryExists = Directory.Exists(absSpaDirectory);

        if (!directoryExists)
        {
            logger.LogWarning(
                "Trying to serve SPA but Directory does not exist: {dir}",
                absSpaDirectory
            );
        }

        var defaultFilesOptions = new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(absSpaDirectory),
        };

        if (string.IsNullOrEmpty(spaRequestPath))
        {
            defaultFilesOptions.RequestPath = spaRequestPath;
        }

        app.UseDefaultFiles(defaultFilesOptions);

        var staticFilesOptions = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(absSpaDirectory),
            ServeUnknownFileTypes = true,
            DefaultContentType = "text/html",
            HttpsCompression = HttpsCompressionMode.Compress,
        };

        if (prefixPath)
        {
            staticFilesOptions.RequestPath = spaRequestPath;
        }

        app.UseStaticFiles(staticFilesOptions);

        return app;
    }
}