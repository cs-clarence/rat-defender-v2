using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using WrapperGen.Utilities;

namespace WrapperGen.Sources;

internal class WrapperAttributeSource : GenericWrapperAttributeSource
{
    public new const string Name = GenericWrapperAttributeSource.Name;
    public new const string Namespace = GenericWrapperAttributeSource.Namespace;
    public new static readonly string FullName = $"{Namespace}.{Name}";

    public new static string GetSource()
    {
        var a = new SourceBuilder();

        a.EnableNullable()
            .Line($"namespace {Namespace};")
            .Line()
            .GeneratedCode()
            .AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
                inherit: false)
            .Line($"public class {Name}(")
            .Indent((b) =>
            {
                b.Line($"Type? {ValuePropertyType.Camelize()} = null,");
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
            .String(") : Attribute")
            .CurlyBracket((b) =>
            {
                b.Line(
                    $"public string {ValuePropertyName} {{ get; set; }} = {ValuePropertyName.Camelize()};");
                b.Line(
                    $"public Type {ValuePropertyType} {{ get; set; }} = {ValuePropertyType.Camelize()} ?? typeof(string);");
                b.Line(
                    $"public bool {FromValueConversion} {{ get; set; }} = {FromValueConversion.Camelize()};");
                b.Line(
                    $"public bool {ToValueConversion} {{ get; set; }} = {ToValueConversion.Camelize()};");
                b.Line(
                    $"public bool {GenerateValueProperty} {{ get; set; }} = {GenerateValueProperty.Camelize()};");
                b.Line(
                    $"public bool {GenerateConstructor} {{ get; set; }} = {GenerateConstructor.Camelize()};");
                b.Line(
                    $"public string? {ValueAssertionMethodName} {{ get; set; }} = {ValueAssertionMethodName.Camelize()};");
                b.Line(
                    $"public bool {AutoDiscoverValueAssertionMethod} {{ get; set; }} = {AutoDiscoverValueAssertionMethod.Camelize()};");
                b.Line(
                    $"public bool {AutoDiscoverValueProperty} {{ get; set; }} = {AutoDiscoverValueProperty.Camelize()};");
            })
            .DisableNullable();
        return a.ToString();
    }

    public static void AddWrapperAttributeSource(
        IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(
            "WrapperAttribute.g.cs",
            SourceText.From(GetSource(), Encoding.UTF8)
        );
    }
}