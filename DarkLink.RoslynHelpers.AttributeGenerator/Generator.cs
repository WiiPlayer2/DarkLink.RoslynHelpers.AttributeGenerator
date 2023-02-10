using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DarkLink.RoslynHelpers.AttributeGenerator.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DarkLink.RoslynHelpers.AttributeGenerator;

[Generator]
public class Generator : IIncrementalGenerator
{
    private static readonly Encoding encoding = new UTF8Encoding(false);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialize);

        var definitions = context.SyntaxProvider.ForAttributeWithMetadataName(
                GenerateAttributeData.ATTRIBUTE_NAME,
                (node, _) => true,
                (syntaxContext, _) =>
                {
                    var type = (INamedTypeSymbol) syntaxContext.TargetSymbol;
                    var typeKind = type.FindTypeKind();
                    var parameters = FindParameters(type, typeKind);
                    if (parameters is null)
                        return default;

                    var data = GenerateAttributeData.From(syntaxContext.Attributes.First());

                    return new AttributeDefinition(data, type, typeKind, parameters);
                })
            .Where(o => o is not null)
            .Select((o, _) => o!);

        context.RegisterSourceOutput(definitions, (productionContext, definition) =>
        {
            var hintName = $"{definition.Type.ToDisplayString()}.g.cs";
            using var codeWriter = new AttributeCodeWriter();
            codeWriter.WriteDefinition(definition);
            var code = codeWriter.ToString();
            productionContext.AddSource(hintName, SourceText.From(code, encoding));
        });
    }

    private ImmutableArray<IParameterSymbol>? FindParameters(INamedTypeSymbol type, TypeKind typeKind)
    {
        var isRecord = typeKind is TypeKind.Record;

        if ((isRecord && type.Constructors.Length > 2) || (!isRecord && type.Constructors.Length > 1))
            return null;

        if (isRecord)
            return type.Constructors.Length == 1
                ? ImmutableArray<IParameterSymbol>.Empty
                : type.Constructors.First().Parameters;

        return type.Constructors.Length == 0
            ? ImmutableArray<IParameterSymbol>.Empty
            : type.Constructors.First().Parameters;
    }

    private void PostInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        GenerateAttributeData.AddTo(context);
    }
}
