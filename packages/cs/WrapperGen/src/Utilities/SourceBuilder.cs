using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace WrapperGen.Utilities;

public class SourceBuilder
{
    private readonly StringBuilder _builder;
    private readonly IndentedTextWriter _writer;

    public SourceBuilder()
    {
        _builder = new StringBuilder();
        _writer = new IndentedTextWriter(new StringWriter(_builder));
    }

    public SourceBuilder Line(string line)
    {
        _writer.WriteLine(line);

        return this;
    }

    public SourceBuilder Line()
    {
        _writer.WriteLine();

        return this;
    }

    public SourceBuilder Char(char c)
    {
        _writer.Write(c);
        return this;
    }

    public SourceBuilder String(string value)
    {
        _writer.Write(value);
        return this;
    }

    public SourceBuilder Attribute(string attribute)
    {
        Line($"[{attribute}]");

        return this;
    }

    public SourceBuilder AttributeUsage(AttributeTargets targets,
        bool inherit = false)
    {
        var usages = string.Join(" | ",
            targets.ToString().Split(',')
                .Select(x => $"AttributeTargets.{x.Trim()}"));
        Attribute(
            $"AttributeUsage({usages}, Inherited = {inherit.ToString().ToLower()})");
        return this;
    }

    public SourceBuilder EnableNullable()
    {
        Line("#nullable enable");
        return this;
    }

    public SourceBuilder DisableNullable()
    {
        Line("#nullable disable");
        return this;
    }

    public SourceBuilder Indent(Action<SourceBuilder> action)
    {
        _writer.Indent++;
        action(this);
        _writer.Indent--;

        return this;
    }

    public SourceBuilder Indent(char leading, char trailing,
        Action<SourceBuilder> action)
    {
        _writer.Write(leading);
        Indent(action);
        _writer.Write(trailing);

        return this;
    }

    public SourceBuilder Indent(string leading, string trailing,
        Action<SourceBuilder> action)
    {
        _writer.WriteLine(leading);
        Indent(action);
        _writer.WriteLine(trailing);

        return this;
    }

    public SourceBuilder CurlyBracket(Action<SourceBuilder> action)
    {
        Indent("{", "}", action);
        return this;
    }

    public SourceBuilder GeneratedCode(string tool, string version)
    {
        Line(
            $"[global::System.CodeDom.Compiler.GeneratedCode(\"{tool}\", \"{version}\")]");

        return this;
    }

    public SourceBuilder GeneratedCode()
    {
        var asm = Assembly.GetCallingAssembly()!;
        GeneratedCode(
            asm.GetName().Name!,
            asm.GetName().Version!.ToString()
        );

        return this;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}