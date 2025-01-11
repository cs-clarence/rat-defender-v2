using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using WrapperGen.Utilities;

namespace WrapperGen.Sources;

internal class GenericWrapperAttributeSource
{
    public const string Name = "WrapperAttribute";

    public static readonly string Namespace = "WrapperGen";

    public static readonly string FullName = $"{Namespace}.{Name}`1";

    public const string ValuePropertyName =
        nameof(ValuePropertyName);

    public const string ValuePropertyType =
        nameof(ValuePropertyType);

    public const string FromValueConversion =
        nameof(FromValueConversion);

    public const string ToValueConversion =
        nameof(ToValueConversion);

    public const string GenerateValueProperty =
        nameof(GenerateValueProperty);

    public const string GenerateConstructor =
        nameof(GenerateConstructor);

    public const string ValueAssertionMethodName =
        nameof(ValueAssertionMethodName);

    public const string AutoDiscoverValueAssertionMethod =
        nameof(AutoDiscoverValueAssertionMethod);

    public const string AutoDiscoverValueProperty =
        nameof(AutoDiscoverValueProperty);

    public static string GetSource()
    {
        var a = new SourceBuilder();

        a.EnableNullable()
            .Line($"namespace {Namespace};")
            .Line()
            .GeneratedCode()
            .AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
                inherit: false)
            .Line($"public class {Name}<TValue>(")
            .Indent((b) =>
            {
                b.Line(
                    $"string {ValuePropertyName.Camelize()} = \"Value\",");
                b.Line(
                    $"string? {ValueAssertionMethodName.Camelize()} = null,");
                b.Line($"bool {FromValueConversion.Camelize()} = true,");
                b.Line($"bool {ToValueConversion.Camelize()} = true,");
                b.Line(
                    $"bool {GenerateValueProperty.Camelize()} = false,");
                b.Line(
                    $"bool {GenerateConstructor.Camelize()} = false,");
                b.Line(
                    $"bool {AutoDiscoverValueAssertionMethod.Camelize()} = true,");
                b.Line(
                    $"bool {AutoDiscoverValueProperty.Camelize()} = true");
            })
            .Line(") : WrapperAttribute(")
            .Indent((b) =>
            {
                b.Line("typeof(TValue),");
                b.Line(
                    $"{ValuePropertyName.Camelize()},");
                b.Line(
                    $"{ValueAssertionMethodName.Camelize()},");
                b.Line(
                    $"{FromValueConversion.Camelize()},");
                b.Line(
                    $"{ToValueConversion.Camelize()},");
                b.Line(
                    $"{GenerateValueProperty.Camelize()},");
                b.Line(
                    $"{GenerateConstructor.Camelize()},");
                b.Line(
                    $"{AutoDiscoverValueAssertionMethod.Camelize()},");
                b.Line(
                    $"{AutoDiscoverValueProperty.Camelize()}");
            })
            .Line(");")
            .DisableNullable();
        return a.ToString();
    }

    public static void AddGenericWrapperAttributeSource(
        IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(
            "GenericWrapperAttribute.g.cs",
            SourceText.From(GetSource(), Encoding.UTF8)
        );
    }
}