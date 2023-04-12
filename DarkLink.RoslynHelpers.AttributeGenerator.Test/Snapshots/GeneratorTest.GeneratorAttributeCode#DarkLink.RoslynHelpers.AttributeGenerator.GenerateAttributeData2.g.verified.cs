﻿//HintName: DarkLink.RoslynHelpers.AttributeGenerator.GenerateAttributeData2.g.cs
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
    partial record GenerateAttributeData2
    {
        public const string ATTRIBUTE_NAME = "DarkLink.RoslynHelpers.GenerateAttributeAttribute";

        public static void AddTo(IncrementalGeneratorPostInitializationContext context)
        {
            const string hintName = "DarkLink.RoslynHelpers.GenerateAttributeAttribute.g.cs";
            const string code = @"using System;

namespace DarkLink.RoslynHelpers
{
    [AttributeUsage((AttributeTargets)4, AllowMultiple = false, Inherited = true)]
    internal class GenerateAttributeAttribute : Attribute
    {
        public GenerateAttributeAttribute(System.AttributeTargets validOn) { }
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }
        public string? Namespace { get; set; }
        public string? Name { get; set; }
    }
}
";
            var sourceText = SourceText.From(code, new UTF8Encoding(false));
            context.AddSource(hintName, sourceText);
        }

        public static GenerateAttributeData2 From(AttributeData data)
        {
            var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);
            var ___ValidOn = (System.AttributeTargets)data.ConstructorArguments[0].Value!;
            var ___AllowMultiple = GetNamedValueOrDefault<bool>("AllowMultiple", false);
            var ___Inherited = GetNamedValueOrDefault<bool>("Inherited", true);
            var ___Namespace = GetNamedValueOrDefault<string?>("Namespace", null);
            var ___Name = GetNamedValueOrDefault<string?>("Name", null);
            return new(ValidOn: ___ValidOn, AllowMultiple: ___AllowMultiple, Inherited: ___Inherited, Namespace: ___Namespace, Name: ___Name);
            T GetNamedValueOrDefault<T>(string name, T defaultValue) => namedArguments.TryGetValue(name, out var value) ? (T) value.Value! : defaultValue;
        }

        public static bool TryFrom(AttributeData data, [NotNullWhen(true)] out GenerateAttributeData2? parsedData)
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
            Func<GeneratorAttributeSyntaxContext, IReadOnlyList<(AttributeData AttributeData, GenerateAttributeData2 ParsedData)>, CancellationToken, T> transform)
            => syntaxValueProvider.ForAttributeWithMetadataName(
                ATTRIBUTE_NAME,
                predicate,
                (context, cancellationToken) =>
                {
                    var attributes = (IReadOnlyList<(AttributeData, GenerateAttributeData2)>) context.Attributes
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
            Func<GeneratorAttributeSyntaxContext, IReadOnlyList<GenerateAttributeData2>, CancellationToken, T> transform)
            => Find(
                syntaxValueProvider,
                predicate,
                (context, pairs, cancellationToken) => transform(context, pairs.Select(p => p.ParsedData).ToList(), cancellationToken));
    }
}
