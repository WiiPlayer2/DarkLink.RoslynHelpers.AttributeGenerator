﻿//HintName: DarkLink.RoslynHelpers.AttributeGenerator.TypedData.g.cs
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

namespace DarkLink.RoslynHelpers.AttributeGenerator
{
    partial record TypedData
    {
        public const string ATTRIBUTE_NAME = "DarkLink.RoslynHelpers.AttributeGenerator.TypedData";

        public static void AddTo(IncrementalGeneratorPostInitializationContext context)
        {
            const string hintName = "DarkLink.RoslynHelpers.AttributeGenerator.TypedData.g.cs";
            const string code = @"using System;

namespace DarkLink.RoslynHelpers.AttributeGenerator
{
    [AttributeUsage((AttributeTargets)32767, AllowMultiple = false, Inherited = true)]
    public class TypedData : Attribute
    {
        public TypedData() { }
        public System.Type Argument { get; set; }
    }
}
";
            var sourceText = SourceText.From(code, new UTF8Encoding(false));
            context.AddSource(hintName, sourceText);
        }

        public static TypedData From(AttributeData data)
        {
            var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);
            var ___argument = GetNamedValueOrDefault<Microsoft.CodeAnalysis.INamedTypeSymbol>("Argument", null);
            return new(argument: ___argument);
            T GetNamedValueOrDefault<T>(string name, T defaultValue) => namedArguments.TryGetValue(name, out var value) ? (T) value.Value! : defaultValue;
        }

        public static bool TryFrom(AttributeData data, [NotNullWhen(true)] out TypedData? parsedData)
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
            Func<GeneratorAttributeSyntaxContext, IReadOnlyList<(AttributeData AttributeData, TypedData ParsedData)>, CancellationToken, T> transform)
            => syntaxValueProvider.ForAttributeWithMetadataName(
                ATTRIBUTE_NAME,
                predicate,
                (context, cancellationToken) =>
                {
                    var attributes = (IReadOnlyList<(AttributeData, TypedData)>) context.Attributes
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
            Func<GeneratorAttributeSyntaxContext, IReadOnlyList<TypedData>, CancellationToken, T> transform)
            => Find(
                syntaxValueProvider,
                predicate,
                (context, pairs, cancellationToken) => transform(context, pairs.Select(p => p.ParsedData).ToList(), cancellationToken));
    }
}