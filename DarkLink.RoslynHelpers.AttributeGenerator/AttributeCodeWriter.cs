using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DarkLink.RoslynHelpers.AttributeGenerator.Util;
using Microsoft.CodeAnalysis;

namespace DarkLink.RoslynHelpers.AttributeGenerator;

internal class AttributeCodeWriter : IDisposable
{
    private readonly StringWriter codeWriter;

    private readonly IndentedTextWriter writer;

    public AttributeCodeWriter()
    {
        codeWriter = new StringWriter();
        writer = new IndentedTextWriter(codeWriter, "    ");
    }

    public void Dispose()
    {
        writer.Dispose();
        codeWriter.Dispose();
    }

    public override string ToString() => codeWriter.ToString();

    private void WriteAttributeGenerationCode(AttributeDefinition definition)
    {
        writer.WriteLine("public static void AddTo(IncrementalGeneratorPostInitializationContext context)");
        writer.WriteLine("{");

        using (writer.IndentScope())
        {
            writer.WriteLine($"const string hintName = \"{definition.FullName}.g.cs\";");
            writer.WriteLine("const string code = @\"using System;");

            using (writer.ResetScope())
            {
                writer.WriteLine();

                if (definition.Namespace is not null)
                {
                    writer.WriteLine($"namespace {definition.Namespace}");
                    writer.WriteLine("{");
                    writer.Indent++;
                }

                writer.WriteLine($"[AttributeUsage((AttributeTargets){(int) definition.Data.ValidOn}, AllowMultiple = {definition.Data.AllowMultiple.ToLiteral()}, Inherited = {definition.Data.Inherited.ToLiteral()})]");
                writer.WriteLine($"public class {definition.Name} : Attribute");
                writer.WriteLine("{");

                using (writer.IndentScope())
                {
                    writer.WriteLine($"public {definition.Name}({string.Join(", ", GetConstructorParameters())}) {{ }}");
                    foreach (var propertyParameter in GetPropertyParameters())
                        writer.WriteLine(propertyParameter);
                }

                writer.WriteLine("}");

                if (definition.Namespace is not null)
                {
                    writer.Indent--;
                    writer.WriteLine("}");
                }

                writer.WriteLine("\";");
            }

            writer.WriteLine("var sourceText = SourceText.From(code, new UTF8Encoding(false));");
            writer.WriteLine("context.AddSource(hintName, sourceText);");
        }

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

    private void WriteAttributeParsingCode(AttributeDefinition definition)
    {
        writer.WriteLine($"public static {definition.Type.Name} From(AttributeData data)");
        writer.WriteLine("{");

        using (writer.IndentScope())
        {
            writer.WriteLine("var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);");

            var requiredParameters = GetRequiredParameters();
            for (var i = 0; i < requiredParameters.Count; i++)
            {
                var parameter = requiredParameters[i];
                writer.WriteLine($"var ___{parameter.Name} = ({parameter.Type.ToDisplayString()})data.ConstructorArguments[{i}].Value!;");
            }

            foreach (var parameter in GetOptionalParameters()) writer.WriteLine($"var ___{parameter.Name} = GetNamedValueOrDefault<{parameter.Type.ToDisplayString()}>(\"{parameter.Name.Capitalize()}\", {parameter.ToDefaultLiteral()});");

            writer.WriteLine($"return new({string.Join(", ", GetFormattedArguments())});");

            writer.WriteLine("T GetNamedValueOrDefault<T>(string name, T defaultValue) => namedArguments.TryGetValue(name, out var value) ? (T) value.Value! : defaultValue;");
        }

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

    public void WriteDefinition(AttributeDefinition definition)
    {
        WriteHeader();

        WriteSymbolStart(definition.Type);

        WriteFullNameConstantCode(definition);
        writer.WriteLineNoTabs(string.Empty);
        WriteAttributeGenerationCode(definition);
        writer.WriteLineNoTabs(string.Empty);
        WriteAttributeParsingCode(definition);

        WriteSymbolEnd(definition.Type);
    }

    private void WriteFullNameConstantCode(AttributeDefinition definition)
        => writer.WriteLine($"public const string ATTRIBUTE_NAME = {definition.FullName.ToLiteral()};");

    private void WriteHeader()
    {
        writer.WriteLine(@"// <auto-generated />
#nullable enable

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
");
    }

    private void WriteSymbolEnd(ISymbol currentSymbol)
    {
        if (currentSymbol.Kind == SymbolKind.NetModule)
            return;

        if (currentSymbol.ContainingSymbol is not null && currentSymbol is not INamespaceSymbol)
            WriteSymbolEnd(currentSymbol.ContainingSymbol);

        if (currentSymbol is not INamespaceSymbol {IsGlobalNamespace: false,} and not ITypeSymbol)
            return;

        writer.Indent--;
        writer.WriteLine("}");
    }

    private void WriteSymbolStart(ISymbol currentSymbol)
    {
        if (currentSymbol.Kind == SymbolKind.NetModule)
            return;

        if (currentSymbol.ContainingSymbol is not null && currentSymbol is not INamespaceSymbol)
            WriteSymbolStart(currentSymbol.ContainingSymbol);

        switch (currentSymbol)
        {
            case INamespaceSymbol {IsGlobalNamespace: false,} namespaceSymbol:
                writer.WriteLine($"namespace {namespaceSymbol.ToDisplayString()}");
                writer.WriteLine("{");
                writer.Indent++;
                break;

            case ITypeSymbol typeSymbol:
                var typeKind = typeSymbol.FindTypeKind();
                writer.WriteLine($"partial {typeKind.ToString().ToLowerInvariant()} {typeSymbol.Name}");
                writer.WriteLine("{");
                writer.Indent++;
                break;
        }
    }
}
