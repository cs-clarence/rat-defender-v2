using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using WrapperGen.Outputs;
using WrapperGen.Utilities;

namespace WrapperGen.Sources;

public record struct WrapperSourceOutput
{
    public string FileName { get; set; }
    public string Code { get; set; }
}

public static class WrapperSource
{
    private static WrapperSourceOutput GetSource(WrapperToGenerate data)
    {
        var a = new SourceBuilder();

        a.EnableNullable()
            .Line($"namespace {data.Namespace};")
            .Line()
            .GeneratedCode()
            .Line(
                data.DeclarationType switch
                {
                    DeclarationType.Class =>
                        $"public partial class {data.Name}",
                    DeclarationType.Record =>
                        $"public partial record {data.Name}",
                    DeclarationType.RecordClass =>
                        $"public partial record class {data.Name}",
                    DeclarationType.Struct =>
                        $"public partial struct {data.Name}",
                    DeclarationType.RefStruct =>
                        $"public ref partial struct {data.Name}",
                    DeclarationType.ReadOnlyStruct =>
                        $"public readonly partial struct {data.Name}",
                    DeclarationType.ReadOnlyRefStruct =>
                        $"public readonly ref partial struct {data.Name}",
                    DeclarationType.RecordStruct =>
                        $"public partial record struct {data.Name}",
                    DeclarationType.ReadOnlyRecordStruct =>
                        $"public readonly partial record struct {data.Name}",
                    _ => throw new ArgumentOutOfRangeException()
                }
            );


        var valueParamName = data.ValuePropertyName.Camelize();
        a.CurlyBracket((b) =>
        {
            if (data.GenerateValueProperty)
            {
                b.Line(
                    $"public {data.ValuePropertyType} {data.ValuePropertyName} {{ get; }}");
            }

            if (data.GenerateConstructor)
            {
                // Constructor
                b.Line(
                        $"public {data.Name}({data.ValuePropertyType} {valueParamName}) : this()")
                    .CurlyBracket(c =>
                    {
                        if (data.AssertValue is not null)
                        {
                            c.Line(
                                $"{data.AssertValue}({valueParamName});");
                        }

                        c.Line(
                            $"this.{data.ValuePropertyName} = {valueParamName};");
                    });
            }


            // Explicit Conversion
            if (data.FromValueConversion)
            {
                b.Line()
                    .Line(
                        $"public static explicit operator {data.Name}({data.ValuePropertyType} {valueParamName}) => new {data.Name}({valueParamName});");
            }

            // Implicit Conversion
            if (data.ToValueConversion)
            {
                b.Line()
                    .Line(
                        $"public static implicit operator {data.ValuePropertyType}({data.Name} {data.Name}) => {data.Name}.{data.ValuePropertyName};");
            }
        });

        a.DisableNullable();

        return new WrapperSourceOutput()
        {
            FileName = $"{data.Name}.g.cs",
            Code = a.ToString()
        };
    }

    public static void AddWrapperSource(this SourceProductionContext context,
        WrapperToGenerate data)
    {
        var generated = GetSource(data);
        context.AddSource(generated.FileName,
            SourceText.From(generated.Code, Encoding.UTF8));
    }
}