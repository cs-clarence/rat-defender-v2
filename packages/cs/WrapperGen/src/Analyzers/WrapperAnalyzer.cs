using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using WrapperGen.Exceptions;
using WrapperGen.Outputs;
using WrapperGen.Sources;
using WrapperGen.Utilities;

namespace WrapperGen.Analyzers;

public static class WrapperAnalyzer
{
    private static DeclarationType GetDeclarationType(
        RecordDeclarationSyntax node)
    {
        var isClass = node.ClassOrStructKeyword.Text == "class";
        var isStruct = node.ClassOrStructKeyword.Text == "struct";
        var isReadonly = node.Modifiers.Any(m => m.Text == "readonly");

        if (isClass)
        {
            return DeclarationType.RecordClass;
        }

        if (isStruct && isReadonly)
        {
            return DeclarationType.ReadOnlyRecordStruct;
        }

        if (isStruct)
        {
            return DeclarationType.RecordStruct;
        }

        return DeclarationType.Record;
    }

    private static DeclarationType GetDeclarationType(
        StructDeclarationSyntax node)
    {
        var isReadonly = node.Modifiers.Any(m => m.Text == "readonly");
        var isRef = node.Modifiers.Any(m => m.Text == "ref");

        if (isReadonly && isRef)
        {
            return DeclarationType.ReadOnlyRefStruct;
        }

        if (isReadonly)
        {
            return DeclarationType.ReadOnlyStruct;
        }

        if (isRef)
        {
            return DeclarationType.RefStruct;
        }

        return DeclarationType.Struct;
    }

    private static DeclarationType GetDeclarationType(SyntaxNode node)
        => node switch
        {
            ClassDeclarationSyntax => DeclarationType.Class,
            RecordDeclarationSyntax r => GetDeclarationType(r),
            StructDeclarationSyntax s => GetDeclarationType(s),
            _ => throw new Exception("Unknown declaration type")
        };

    record AnalysisFlags
    {
        public bool GenerateValueProperty { get; set; } = true;
        public bool GenerateConstructor { get; set; } = true;
        public bool AutoDiscoverAssertValue { get; set; } = true;
        public bool AutoDiscoverValue { get; set; } = true;
        public string? AssertValue { get; set; }
        public string? ValuePropertyType { get; set; }
        public string? ValuePropertyName { get; set; }
        public bool FromValueConversion { get; set; } = true;
        public bool ToValueConversion { get; set; } = true;
    };

    private static string GetFullNamespace(INamedTypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return "";
        }

        var typeInfo = typeSymbol.ContainingNamespace;
        var ns = new List<string>();

        while (typeInfo is not null)
        {
            if (typeInfo.IsGlobalNamespace)
            {
                break;
            }
            
            ns.Add(typeInfo.Name);
            typeInfo = typeInfo.ContainingNamespace;
        }

        ns.Reverse();

        return string.Join(".", ns);
    }


    private static AnalysisFlags GetAnalysisFlags(
        GeneratorAttributeSyntaxContext context)
    {
        var flags = new AnalysisFlags();
        var attribute = context.TargetSymbol
            .GetAttributes()
            .FirstOrDefault(data =>
                data.AttributeClass is not null
                && (data.AttributeClass.Name ==
                    WrapperAttributeSource.Name
                    || data.AttributeClass.Name ==
                    GenericWrapperAttributeSource
                        .GenericName
                )
                && GetFullNamespace(data.AttributeClass) ==
                WrapperAttributeSource.Namespace
            );


        if (attribute is null)
        {
            var diagnostics = new Diagnostics();

            diagnostics.Add(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "WRG0001",
                    "WrapperGen",
                    $"The {WrapperAttributeSource.Name} attribute is not found on the target",
                    "WrapperGen",
                    DiagnosticSeverity.Error,
                    true
                ),
                GetLocation(context)
            ));

            throw new CompileError(
                $"The {WrapperAttributeSource.FullName} attribute is not found on the target",
                diagnostics
            );
        }
        
        var attributeClass = attribute.AttributeClass;

        if (attributeClass is { IsGenericType: true, TypeArguments.Length: 1 })
        {
            var typeArgument = attributeClass.TypeArguments[0];
            flags.ValuePropertyType =
                $"{typeArgument.ContainingNamespace}.{typeArgument.Name}";
        }

        foreach (var arg in attribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case GenericWrapperAttributeSource.GenerateValueProperty:
                    flags.GenerateValueProperty = arg.Value.Value is null
                                                  || (arg.Value
                                                      .Value as bool? ?? true);
                    break;
                case GenericWrapperAttributeSource.GenerateConstructor:
                    flags.GenerateConstructor = arg.Value.Value is null
                                                || (arg.Value.Value as bool? ??
                                                    true);
                    break;
                case GenericWrapperAttributeSource
                    .AutoDiscoverValueAssertionMethod:
                    flags.AutoDiscoverAssertValue = arg.Value.Value is null
                                                    || (arg.Value
                                                            .Value as bool? ??
                                                        true);
                    break;
                case GenericWrapperAttributeSource.AutoDiscoverValueProperty:
                    flags.AutoDiscoverValue = arg.Value.Value is null
                                              || (arg.Value.Value as bool? ??
                                                  true);
                    break;
                case GenericWrapperAttributeSource.ValueAssertionMethodName:
                    flags.AssertValue ??= arg.Value.Value?.ToString();
                    break;
                case GenericWrapperAttributeSource.ValuePropertyType:
                    var typedSymbol = (INamedTypeSymbol)arg.Value.Value!;
                    flags.ValuePropertyType =
                        $"{typedSymbol.ContainingNamespace}.{typedSymbol.Name}";
                    break;
                case GenericWrapperAttributeSource.ValuePropertyName:
                    flags.ValuePropertyName =
                        arg.Value.Value as string ?? "Value";
                    break;
                case GenericWrapperAttributeSource.FromValueConversion:
                    flags.FromValueConversion = arg.Value.Value is null
                                                || (arg.Value.Value as bool? ??
                                                    true);
                    break;
                case GenericWrapperAttributeSource.ToValueConversion:
                    flags.ToValueConversion = arg.Value.Value is null
                                              || (arg.Value.Value as bool? ??
                                                  true);
                    break;
            }
        }

        return flags;
    }

    private static SyntaxList<MemberDeclarationSyntax> GetMembers(
        this GeneratorAttributeSyntaxContext context)
    {
        return context.TargetNode switch
        {
            ClassDeclarationSyntax classNode => classNode.Members,
            StructDeclarationSyntax structNode => structNode.Members,
            RecordDeclarationSyntax recordNode => recordNode.Members,
            _ => throw new Exception("Unknown declaration type")
        };
    }

    private static SeparatedSyntaxList<ParameterSyntax>? GetParameters(
        this GeneratorAttributeSyntaxContext context)
    {
        return context.TargetNode switch
        {
            ClassDeclarationSyntax classNode => classNode.ParameterList
                ?.Parameters,
            StructDeclarationSyntax structNode => structNode.ParameterList
                ?.Parameters,
            RecordDeclarationSyntax recordNode => recordNode.ParameterList
                ?.Parameters,
            _ => null
        };
    }

    private static string GetFullTypeName(TypeSyntax typeSyntax,
        GeneratorAttributeSyntaxContext context)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(typeSyntax);

        if (typeInfo.Type is null)
        {
            var diagnostics = new Diagnostics();

            diagnostics.Add(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "WRG002",
                    "WrapperGen",
                    "Type cannot be null",
                    "WrapperGen",
                    DiagnosticSeverity.Error,
                    true
                ),
                GetLocation(context)
            ));

            throw new CompileError(
                "Type cannot be inferred",
                diagnostics
            );
        }

        if (typeSyntax is TupleTypeSyntax tup)
        {
            var tf = string.Join(", ",
                tup.Elements.Select(x => GetFullTypeName(x.Type, context)));

            return $"({tf})";
        }


        var typeFullName =
            $"{typeInfo.Type?.ContainingNamespace.Name}.{typeInfo.Type?.Name}";

        return typeFullName;
    }

    private static bool TryDiscoverValue(
        GeneratorAttributeSyntaxContext context,
        AnalysisFlags flags,
        out string? valueName,
        out string? valueType,
        out bool generateConstructor
    )
    {
        valueName = flags.ValuePropertyName ?? "Value";
        valueType = flags.ValuePropertyType;

        var members = context.GetMembers();

        var parameters = context.GetParameters();

        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Type is null) continue;
                if (parameter.Identifier.Text != valueName) continue;

                var typeFullName = GetFullTypeName(parameter.Type, context);

                if (valueType is not null && valueType != typeFullName)
                    continue;

                valueType = typeFullName;
                generateConstructor = false;
                return true;
            }
        }


        foreach (var member in members)
        {
            if (member is not PropertyDeclarationSyntax
                and not FieldDeclarationSyntax) continue;

            var memberName = member switch
            {
                PropertyDeclarationSyntax propertyNode => propertyNode
                    .Identifier.Text,
                FieldDeclarationSyntax fieldNode => fieldNode.Declaration
                    .Variables.First().Identifier.Text,
                _ => throw new Exception("Unknown member type")
            };

            if (memberName != valueName) continue;

            var typeSyntax = member switch
            {
                PropertyDeclarationSyntax propertyNode => propertyNode.Type,
                FieldDeclarationSyntax fieldNode => fieldNode.Declaration.Type,
                _ => throw new Exception("Unknown member type")
            };

            var typeFullName = GetFullTypeName(typeSyntax, context);

            if (valueType is not null && valueType != typeFullName) continue;

            valueType = typeFullName;
            generateConstructor = true;
            return true;
        }

        valueName = null;
        valueType = null;
        generateConstructor = false;
        return false;
    }

    private static bool TryDiscoverAssertValue(
        GeneratorAttributeSyntaxContext context,
        AnalysisFlags flags,
        out string? assertValue)
    {
        var members = context.GetMembers();

        foreach (var member in members)
        {
            if (member is not MethodDeclarationSyntax methodNode) continue;

            if (methodNode.Identifier.Text != "AssertValue") continue;

            if (methodNode.ReturnType is not PredefinedTypeSyntax predefined)
                continue;

            if (predefined.Keyword.Text.Trim() != "void") continue;

            var parameters = methodNode.ParameterList.Parameters;

            if (parameters.Count != 1) continue;

            var parameter = parameters[0];
            if (parameter.Type is null) continue;
            var typeInfo = context.SemanticModel.GetTypeInfo(parameter.Type);

            var typeFullName =
                $"{typeInfo.Type?.ContainingNamespace.Name}.{typeInfo.Type?.Name}";
            if (typeFullName != flags.ValuePropertyType) continue;


            assertValue = "AssertValue";
            return true;
        }

        assertValue = null;
        return false;
    }

    private static Location GetLocation(GeneratorAttributeSyntaxContext context)
    {
        return context.TargetNode.GetLocation();
    }

    private static void AssertInvariants(
        GeneratorAttributeSyntaxContext context)
    {
        var wrappedValueCount = 0;

        var diagnostics = new Diagnostics();


        foreach (var parameter in context.GetParameters() ?? [])
        {
            wrappedValueCount++;
            if (wrappedValueCount > 1)
            {
                diagnostics.Add(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "WRG001",
                        "WrapperGen",
                        "Only one primary constructor parameter/property for the value is allowed",
                        "WrapperGen",
                        DiagnosticSeverity.Error,
                        true
                    ),
                    parameter.GetLocation()
                ));
                break;
            }
        }


        foreach (var member in context.GetMembers())
        {
            if (member is not PropertyDeclarationSyntax
                and not FieldDeclarationSyntax) continue;

            var location = member switch
            {
                PropertyDeclarationSyntax propertyNode => propertyNode
                    .Identifier
                    .GetLocation(),
                FieldDeclarationSyntax fieldNode => fieldNode.Declaration
                    .Variables.First().Identifier.GetLocation(),
                _ => throw new Exception("Unknown member type")
            };

            wrappedValueCount++;

            if (wrappedValueCount > 1)
            {
                diagnostics.Add(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "WRG001",
                        "WrapperGen",
                        "Only one primary constructor parameter/property for the value is allowed",
                        "WrapperGen",
                        DiagnosticSeverity.Error,
                        true
                    ),
                    location
                ));
                break;
            }
        }


        if (diagnostics.List.Any())
        {
            throw new CompileError(
                "Invariants are not met",
                diagnostics
            );
        }
    }

    public static TransformResult<WrapperToGenerate, Diagnostics> Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        try
        {
            AssertInvariants(context);

            cancellationToken.ThrowIfCancellationRequested();
            var className = context.TargetSymbol.Name;
            var namespaceName =
                context.TargetSymbol.ContainingNamespace.ToDisplayString();


            var flags = GetAnalysisFlags(context);

            if (flags.AutoDiscoverValue && TryDiscoverValue(context,
                    flags,
                    out var valueName, out var valueType,
                    out var generateConstructor) &&
                valueName is not null && valueType is not null)
            {
                flags.ValuePropertyName = valueName;
                flags.ValuePropertyType = valueType;
                flags.GenerateValueProperty = false;
                flags.GenerateConstructor = generateConstructor;
            }
            else
            {
                flags.ValuePropertyType ??= "System.String";
                flags.ValuePropertyName ??= "Value";
            }


            if (flags.AutoDiscoverAssertValue &&
                TryDiscoverAssertValue(context, flags,
                    out var assertValue) &&
                assertValue is not null)
            {
                flags.AssertValue = assertValue;
            }

            return TransformResult.Success<WrapperToGenerate, Diagnostics>(
                new WrapperToGenerate
                {
                    Name = className,
                    Namespace = namespaceName,
                    DeclarationType = GetDeclarationType(context.TargetNode),
                    ValuePropertyName = flags.ValuePropertyName,
                    ValuePropertyType =
                        flags.ValuePropertyType,
                    AssertValue = flags.AssertValue,
                    GenerateValueProperty = flags.GenerateValueProperty,
                    GenerateConstructor = flags.GenerateConstructor,
                    FromValueConversion = flags.FromValueConversion,
                    ToValueConversion = flags.ToValueConversion,
                });
        }
        catch (CompileError e)
        {
            return TransformResult.Failure<WrapperToGenerate, Diagnostics>(
                e.Diagnostics);
        }
        catch (Exception e)
        {
            var diagnostics = new Diagnostics();

            diagnostics.Add(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "WRG0000",
                    "WrapperGen",
                    $"An error occurred while generating the wrapper: {e.Message}, StackTrace: {e.StackTrace}",
                    "WrapperGen",
                    DiagnosticSeverity.Error,
                    true
                ),
                Location.None
            ));

            return TransformResult.Failure<WrapperToGenerate, Diagnostics>(
                diagnostics);
        }
    }

    public static bool Predicate(SyntaxNode node,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return node is ClassDeclarationSyntax
            or RecordDeclarationSyntax
            or StructDeclarationSyntax;
    }
}