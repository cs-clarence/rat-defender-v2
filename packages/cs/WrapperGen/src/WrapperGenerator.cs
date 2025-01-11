using Microsoft.CodeAnalysis;
using WrapperGen.Analyzers;
using WrapperGen.Extensions;
using WrapperGen.Sources;

namespace WrapperGen;

[Generator(LanguageNames.CSharp)]
public class WrapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var providerNonGeneric = context.SyntaxProvider.ForAttributeWithMetadataName(
            WrapperAttributeSource.FullName,
            predicate: WrapperAnalyzer.Predicate,
            transform: WrapperAnalyzer.Transform
        );
        
        var providerGeneric = context.SyntaxProvider.ForAttributeWithMetadataName(
            GenericWrapperAttributeSource.FullName,
            predicate: WrapperAnalyzer.Predicate,
            transform: WrapperAnalyzer.Transform
        );

        context.RegisterSourceOutput(providerNonGeneric,
            static (context, source) =>
            {
                source.Switch(
                    value =>
                    {
                        var wrapperToGenerate = value;
                        context.AddWrapperSource(wrapperToGenerate);
                    },
                    diagnostics =>
                    {
                        foreach (var diagnostic in diagnostics)
                        {
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                );
            });
        
        context.RegisterSourceOutput(providerGeneric,
            static (context, source) =>
            {
                source.Switch(
                    value =>
                    {
                        var wrapperToGenerate = value;
                        context.AddWrapperSource(wrapperToGenerate);
                    },
                    diagnostics =>
                    {
                        foreach (var diagnostic in diagnostics)
                        {
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                );
            });

        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddWrapperAttributeSource();
            ctx.AddGenericWrapperAttributeSource();
        });
    }
}