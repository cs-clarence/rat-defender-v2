using System.Text.RegularExpressions;
using Humanizer;
using Microsoft.AspNetCore.Routing;

namespace Common.AspNetCore.Parameters.Transformers;

internal static partial class Pattern
{
    [GeneratedRegex(@"([a-z])([A-Z])", RegexOptions.Compiled, 1000)]
    internal static partial Regex Slugify();
}

public class RestifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is string str)
        {
            return Restify(str);
        }

        return null;
    }

    private static string Restify(string str)
    {
        return Pattern
            .Slugify()
            .Replace(str, "$1-$2")
            .ToLowerInvariant()
            .Pluralize();
    }
}
