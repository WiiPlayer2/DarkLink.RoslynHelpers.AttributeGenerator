﻿//HintName: TestAttribute.g.cs
// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

partial class TestAttribute
{
    public const string ATTRIBUTE_NAME = "TestAttribute";

    public static void AddTo(IncrementalGeneratorPostInitializationContext context)
    {
        const string hintName = "TestAttribute.g.cs";
        const string code = @"using System;

[AttributeUsage((AttributeTargets)32767, AllowMultiple = false, Inherited = true)]
internal class TestAttribute : Attribute
{
    public TestAttribute() { }
}
";
        var sourceText = SourceText.From(code, new UTF8Encoding(false));
        context.AddSource(hintName, sourceText);
    }

    public static TestAttribute From(AttributeData data)
    {
        var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);
        return new();
        T GetNamedValueOrDefault<T>(string name, T defaultValue) => namedArguments.TryGetValue(name, out var value) ? (T) value.Value! : defaultValue;
    }

    public static bool TryFrom(AttributeData data, [NotNullWhen(true)] out TestAttribute? parsedData)
    {
        try
        {
            parsedData = From(data);
            return true;
        }
        catch
        {
            parsedData = default;
            return false;
        }
    }

    public static IncrementalValuesProvider<T> Find<T>(
        SyntaxValueProvider syntaxValueProvider,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, IReadOnlyList<(AttributeData AttributeData, TestAttribute ParsedData)>, CancellationToken, T> transform)
        => syntaxValueProvider.ForAttributeWithMetadataName(
            ATTRIBUTE_NAME,
            predicate,
            (context, cancellationToken) =>
            {
                var attributes = (IReadOnlyList<(AttributeData, TestAttribute)>) context.Attributes
                    .Select(data =>
                    {
                        var result = TryFrom(data, out var attribute);
                        return (result, attribute: (data, attribute));
                    })
                    .Where(pair => pair.result)
                    .Select(pair => pair.attribute!)
                    .ToList();
                return (context, attributes);
            })
            .Where(pair => pair.attributes.Any())
            .Select((pair, ct) => transform(pair.context, pair.attributes, ct));

    public static IncrementalValuesProvider<T> Find<T>(
        SyntaxValueProvider syntaxValueProvider,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, IReadOnlyList<TestAttribute>, CancellationToken, T> transform)
        => Find(
            syntaxValueProvider,
            predicate,
            (context, pairs, cancellationToken) => transform(context, pairs.Select(p => p.ParsedData).ToList(), cancellationToken));
}
