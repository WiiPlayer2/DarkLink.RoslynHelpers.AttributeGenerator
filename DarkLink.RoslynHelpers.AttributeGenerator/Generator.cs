﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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
                    var typeKind = FindTypeKind(type);
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
            using var codeWriter = new StringWriter();

            WriteUsings(codeWriter);

            WriteSymbolStart(codeWriter, definition.Type);

            WriteFullNameConstantCode(codeWriter, definition);
            WriteAttributeGenerationCode(codeWriter, definition);
            WriteAttributeParsingCode(codeWriter, definition);

            WriteSymbolEnd(codeWriter, definition.Type);

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

    private TypeKind FindTypeKind(ITypeSymbol type)
    {
        if (type.IsRecord)
            return TypeKind.Record;

        return TypeKind.Class;
    }

    private void PostInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        GenerateAttributeData.AddTo(context);

        var assembly = typeof(Generator).Assembly;
        var injectedCodeResources = assembly.GetManifestResourceNames()
            .Where(name => name.Contains("InjectedCode"));

        foreach (var resource in injectedCodeResources)
        {
            using var stream = assembly.GetManifestResourceStream(resource)!;
            context.AddSource(resource, SourceText.From(stream, encoding, canBeEmbedded: true));
        }
    }

    private void WriteAttributeGenerationCode(TextWriter writer, AttributeDefinition definition)
    {
        writer.WriteLine("public static void AddTo(IncrementalGeneratorPostInitializationContext context)");
        writer.WriteLine("{");
        writer.WriteLine($"const string hintName = \"{definition.FullName}.g.cs\";");
        writer.WriteLine("const string code = @\"");
        writer.WriteLine("using System;");

        if (definition.Namespace is not null)
        {
            writer.WriteLine($"namespace {definition.Namespace}");
            writer.WriteLine("{");
        }

        writer.WriteLine($"[AttributeUsage((AttributeTargets){(int) definition.Data.ValidOn}, AllowMultiple = {definition.Data.AllowMultiple.ToLiteral()}, Inherited = {definition.Data.Inherited.ToLiteral()})]");
        writer.WriteLine($"public class {definition.Name} : Attribute");
        writer.WriteLine("{");
        writer.WriteLine($"public {definition.Name}({string.Join(", ", GetConstructorParameters())}) {{ }}");
        foreach (var propertyParameter in GetPropertyParameters())
            writer.WriteLine(propertyParameter);
        writer.WriteLine("}");

        if (definition.Namespace is not null) writer.WriteLine("}");

        writer.WriteLine("\";");
        writer.WriteLine("var sourceText = SourceText.From(code, new UTF8Encoding(false));");
        writer.WriteLine("context.AddSource(hintName, sourceText);");
        writer.WriteLine("}");

        IEnumerable<string> GetConstructorParameters()
            => definition.Parameters
                .Where(p => !p.HasExplicitDefaultValue)
                .Select(FormatConstructorParameter);

        IEnumerable<string> GetPropertyParameters()
            => definition.Parameters
                .Where(p => p.HasExplicitDefaultValue)
                .Select(FormatPropertyParameter);

        string FormatConstructorParameter(IParameterSymbol parameter) => $"{parameter.Type.ToDisplayString()} {parameter.Name.Uncapitalize()}";

        string FormatPropertyParameter(IParameterSymbol parameter) => $"public {parameter.Type.ToDisplayString()} {parameter.Name.Capitalize()} {{ get; set; }}";
    }

    private void WriteAttributeParsingCode(TextWriter writer, AttributeDefinition definition)
    {
        writer.WriteLine($"public static {definition.Type.Name} From(AttributeData data)");
        writer.WriteLine("{");
        writer.WriteLine("var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);");

        var requiredParameters = GetRequiredParameters();
        for (var i = 0; i < requiredParameters.Count; i++)
        {
            var parameter = requiredParameters[i];
            writer.WriteLine($"var ___{parameter.Name} = ({parameter.Type.ToDisplayString()})data.ConstructorArguments[{i}].Value!;");
        }

        foreach (var parameter in GetOptionalParameters()) writer.WriteLine($"var ___{parameter.Name} = GetNamedValueOrDefault<{parameter.Type.ToDisplayString()}>(\"{parameter.Name.Capitalize()}\", {parameter.ExplicitDefaultValue.ToLiteral()});");

        writer.WriteLine($"return new({string.Join(", ", GetFormattedArguments())});");

        writer.WriteLine(@"T GetNamedValueOrDefault<T>(string name, T defaultValue)
                => namedArguments.TryGetValue(name, out var value)
                    ? (T) value.Value!
                    : defaultValue;");
        writer.WriteLine("}");

        IReadOnlyList<IParameterSymbol> GetRequiredParameters()
            => definition.Parameters
                .Where(p => !p.HasExplicitDefaultValue)
                .ToList();

        IReadOnlyList<IParameterSymbol> GetOptionalParameters()
            => definition.Parameters
                .Where(p => p.HasExplicitDefaultValue)
                .ToList();

        IEnumerable<string> GetFormattedArguments()
            => definition.Parameters
                .Select(FormatArgument);

        string FormatArgument(IParameterSymbol parameter) => $"{parameter.Name.SanitizeIdentifier()}: ___{parameter.Name}";
    }

    private void WriteFullNameConstantCode(TextWriter writer, AttributeDefinition definition)
        => writer.WriteLine($"public const string ATTRIBUTE_NAME = {definition.FullName.ToLiteral()};");

    private void WriteSymbolEnd(TextWriter writer, ISymbol currentSymbol)
    {
        if (currentSymbol.Kind == SymbolKind.NetModule)
            return;

        if (currentSymbol.ContainingSymbol is not null)
            WriteSymbolEnd(writer, currentSymbol.ContainingSymbol);

        if (currentSymbol is not INamespaceSymbol {IsGlobalNamespace: false,} and not ITypeSymbol)
            return;

        writer.WriteLine("}");
    }

    private void WriteSymbolStart(TextWriter writer, ISymbol currentSymbol)
    {
        if (currentSymbol.Kind == SymbolKind.NetModule)
            return;

        if (currentSymbol.ContainingSymbol is not null)
            WriteSymbolStart(writer, currentSymbol.ContainingSymbol);

        switch (currentSymbol)
        {
            case INamespaceSymbol {IsGlobalNamespace: false,} namespaceSymbol:
                writer.WriteLine($"namespace {namespaceSymbol.Name}");
                writer.WriteLine("{");
                break;

            case ITypeSymbol typeSymbol:
                var typeKind = FindTypeKind(typeSymbol);
                writer.WriteLine($"partial {typeKind.ToString().ToLowerInvariant()} {typeSymbol.Name}"); // class might not be right here
                writer.WriteLine("{");
                break;
        }
    }

    private void WriteUsings(TextWriter writer)
    {
        writer.WriteLine(@"using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
");
    }
}
