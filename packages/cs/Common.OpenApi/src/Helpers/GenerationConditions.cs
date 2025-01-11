using System.Reflection;

namespace Common.OpenApi.Helpers;

public static class GenerationConditions
{
    public static bool IsBuildTime =>
        Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

    public static void IfRuntime(Action action)
    {
        if (!IsBuildTime) action();
    }

    public static void IfBuildTime(Action action)
    {
        if (IsBuildTime) action();
    }
}