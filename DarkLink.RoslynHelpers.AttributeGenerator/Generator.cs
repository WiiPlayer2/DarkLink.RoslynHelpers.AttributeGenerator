using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using DarkLink.RoslynHelpers.AttributeGenerator.Util;
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
            .Where(o => o is not null)
            .Select((o, _) => o!);

        context.RegisterSourceOutput(definitions, async (productionContext, definition) =>
        {
            var hintName = $"{definition.Type.ToDisplayString()}.g.cs";
            using var codeWriter = new StringWriter();

            WriteUsings(codeWriter);

            WriteSymbolStart(codeWriter, definition.Type);

            WriteAttributeGenerationCode(codeWriter, definition);

            WriteSymbolEnd(codeWriter, definition.Type);

            var code = codeWriter.ToString();
            productionContext.AddSource(hintName, SourceText.From(code, encoding));
        });
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

    private void WriteAttributeGenerationCode(TextWriter writer, AttributeDefinition definition)
    {
        writer.WriteLine("public static void AddTo(IncrementalGeneratorPostInitializationContext context)");
        writer.WriteLine("{");
        writer.WriteLine($"const string hintName = \"{definition.Type.ToDisplayString()}.g.cs\";");
        writer.WriteLine("const string code = @\"");
        writer.WriteLine("using System;");
        writer.WriteLine($"[AttributeUsage((AttributeTargets){(int) definition.Data.ValidOn}, AllowMultiple = {definition.Data.AllowMultiple}, Inherited = {definition.Data.Inherited})]");
        writer.WriteLine($"public class {definition.Type.Name} : Attribute");
        writer.WriteLine("{");
        writer.WriteLine($"public {definition.Type.Name}({string.Join(", ", GetConstructorParameters())}) {{ }}");
        foreach (var propertyParameter in GetPropertyParameters())
            writer.WriteLine(propertyParameter);
        writer.WriteLine("}");
        writer.WriteLine("\";");
        writer.WriteLine("var sourceText = SourceText.From(code, new Utf8Encoding(false));");
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

        string FormatConstructorParameter(IParameterSymbol parameter) => $"{parameter.Type.ToDisplayString()} {parameter.Name}";

        string FormatPropertyParameter(IParameterSymbol parameter) => $"public {parameter.Type.ToDisplayString()} {parameter.Name.Capitalize()} {{ get; set; }}";
    }

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
                writer.WriteLine($"partial class {typeSymbol.Name}"); // class might not be right here
                writer.WriteLine("{");
                break;
        }
    }

    private void WriteUsings(TextWriter writer)
    {
        writer.WriteLine(@"using System;
using Microsoft.CodeAnalysis;
");
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
