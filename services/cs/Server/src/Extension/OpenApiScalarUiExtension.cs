using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server.Extension;

public static class OpenApiScalarUiExtension
{
    public static IEndpointConventionBuilder MapScalarApiReference(
        this IEndpointRouteBuilder endpoints
    )
    {
        return endpoints.MapScalarApiReference(_ => { });
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

    public static IEndpointConventionBuilder MapScalarApiReference(
        this IEndpointRouteBuilder endpoints,
        Action<ScalarOptions> configureOptions
    )
    {
        var options = new ScalarOptions();
        configureOptions(options);

        var configurationJson = JsonSerializer.Serialize(
            options,
            JsonSerializerOptions
        );

        return endpoints
            .MapGet(
                options.EndpointPathPrefix + "/{documentName}",
                (string documentName) =>
                {
                    var documentPath =
                        options.OpenApiDocumentRouteTemplate.Replace(
                            "{documentName}",
                            documentName
                        );
                    var title =
                        options.Title
                        ?? $"Scalar API Reference -- {documentName}";
                    return Results.Content(
                        $$"""
                        <!doctype html>
                        <html>
                        <head>
                            <title>{{title}}</title>
                            <meta charset="utf-8" />
                            <meta name="viewport" content="width=device-width, initial-scale=1" />
                        </head>
                        <body>
                            <script id="api-reference" data-url="{{documentPath}}"></script>
                            <script>
                            var configuration = {{configurationJson}}

                            document.getElementById('api-reference').dataset.configuration =
                                JSON.stringify(configuration)
                            </script>
                            <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
                        </body>
                        </html>
                        """,
                        "text/html"
                    );
                }
            )
            .ExcludeFromDescription();
    }
}

public class ScalarOptions
{
    [JsonIgnore]
    public string EndpointPathPrefix { get; set; } = "/scalar";

    [JsonIgnore]
    public string OpenApiDocumentRouteTemplate { get; set; } =
        "/openapi/{documentName}.json";

    [JsonIgnore]
    public string? Title { get; set; }

    public string Theme { get; set; } = "purple";

    public bool? DarkMode { get; set; }
    public bool? HideDownloadButton { get; set; }
    public bool? ShowSideBar { get; set; }

    public bool? WithDefaultFonts { get; set; }

    public string? Layout { get; set; }

    public string? CustomCss { get; set; }

    public string? SearchHotkey { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }

    public ScalarAuthenticationOptions Authentication { get; set; } =
        new ScalarAuthenticationOptions();
}

public class ScalarAuthenticationOptions
{
    public string? PreferredSecurityScheme { get; set; }

    public ScalarAuthenticationApiKey? ApiKey { get; set; }
}

public class ScalarAuthenticationAuth2
{
    public string? ClientId { get; set; }

    public List<string>? Scopes { get; set; }
}

public class ScalarAuthenticationApiKey
{
    public string? Token { get; set; }
}
