using Microsoft.CodeAnalysis;
using WrapperGen.Sources;

namespace WrapperGen.Extensions;

internal static class ContextExtensions
{
    public static void AddWrapperAttributeSource(
        this IncrementalGeneratorPostInitializationContext context)
    {
        WrapperAttributeSource.AddWrapperAttributeSource(context);
    }
    
    public static void AddGenericWrapperAttributeSource(
        this IncrementalGeneratorPostInitializationContext context)
    {
        GenericWrapperAttributeSource.AddGenericWrapperAttributeSource(context);
    }
}