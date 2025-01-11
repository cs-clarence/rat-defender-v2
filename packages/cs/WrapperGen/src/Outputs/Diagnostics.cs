using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace WrapperGen.Outputs;

public record Diagnostics
{
    public ImmutableArray<Diagnostic> List { get; private set; } =
        ImmutableArray<Diagnostic>.Empty;

    public void Add(Diagnostic diagnostic)
    {
        List = List.Add(diagnostic);
    }

    public ImmutableArray<Diagnostic>.Enumerator GetEnumerator()
    {
        return List.GetEnumerator();
    }
}