using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DarkLink.RoslynHelpers.AttributeGenerator;

[Generator]
public class Generator : IIncrementalGenerator
{
    private const string ATTRIBUTE_NAME = "DarkLink.RoslynHelpers.GenerateAttributeAttribute";

    private static readonly Encoding encoding = new UTF8Encoding(false);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialize);

        context.SyntaxProvider.ForAttributeWithMetadataName(
            ATTRIBUTE_NAME,
            (node, _) => node is ClassDeclarationSyntax,
            (syntaxContext, _) => { return new AttributeDefinition(); });
    }

    private void PostInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        var assembly = typeof(Generator).Assembly;
        var injectedCodeResources = assembly.GetManifestResourceNames()
            .Where(name => name.Contains("InjectedCode"));

        foreach (var resource in injectedCodeResources)
        {
            using var stream = assembly.GetManifestResourceStream(resource)!;
            context.AddSource(resource, SourceText.From(stream, encoding, canBeEmbedded: true));
        }
    }

    private record AttributeDefinition;
}
