using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
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

        var definitions = GenerateAttributeData.Find(
                context.SyntaxProvider,
                (_, _) => true,
                TransformDefinition)
            .Where(o => o is not null)
            .Select((o, _) => o!);

        context.RegisterSourceOutput(definitions, GenerateAttributeCode);
    }

    private static ImmutableArray<IParameterSymbol>? FindParameters(INamedTypeSymbol type, TypeKind typeKind)
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

    private static void GenerateAttributeCode(SourceProductionContext productionContext, AttributeDefinition definition)
    {
        var hintName = $"{definition.Type.ToDisplayString()}.g.cs";
        using var codeWriter = new AttributeCodeWriter(definition);
        codeWriter.WriteDefinition();
        var code = codeWriter.ToString();
        productionContext.AddSource(hintName, SourceText.From(code, encoding));
    }

    private void PostInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        GenerateAttributeData.AddTo(context);
    }

    private static AttributeDefinition? TransformDefinition(
        (GeneratorAttributeSyntaxContext syntaxContext, IReadOnlyList<GenerateAttributeData> attributes) pair,
        CancellationToken cancellationToken)
    {
        var (syntaxContext, attributes) = pair;

        var type = (INamedTypeSymbol) syntaxContext.TargetSymbol;
        var typeKind = type.FindTypeKind();
        var parameters = FindParameters(type, typeKind);
        if (parameters is null)
            return default;

        return new AttributeDefinition(attributes.First(), type, typeKind, parameters);
    }
}
