using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        var definitions = context.SyntaxProvider.ForAttributeWithMetadataName(
                ATTRIBUTE_NAME,
                (node, _) => node is ClassDeclarationSyntax,
                (syntaxContext, _) =>
                {
                    var type = (INamedTypeSymbol) syntaxContext.TargetSymbol;
                    if (type.Constructors.Length > 1)
                        return default;

                    var data = GenerateAttributeData.FromAttribute(syntaxContext.Attributes.First());
                    var parameters = type.Constructors.FirstOrDefault()?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;

                    return new AttributeDefinition(data, type, parameters);
                })
            .Where(o => o is not null);

        context.RegisterSourceOutput(definitions, (productionContext, definition) => { });
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

    private record AttributeDefinition(GenerateAttributeData Data, INamedTypeSymbol Type, IReadOnlyList<IParameterSymbol> Parameters);

    private record GenerateAttributeData(AttributeTargets ValidOn, bool AllowMultiple, bool Inherited)
    {
        public static GenerateAttributeData FromAttribute(AttributeData data)
        {
            var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);

            var validOn = (AttributeTargets) data.ConstructorArguments[0].Value!;
            var allowMultiple = GetNamedValueOrDefault(nameof(AllowMultiple), false);
            var inherited = GetNamedValueOrDefault(nameof(Inherited), true);
            return new GenerateAttributeData(validOn, allowMultiple, inherited);

            T GetNamedValueOrDefault<T>(string name, T defaultValue)
                => namedArguments.TryGetValue(name, out var value)
                    ? (T) value.Value!
                    : defaultValue;
        }
    }
}
