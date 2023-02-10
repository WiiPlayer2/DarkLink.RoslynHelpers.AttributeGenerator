using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Util;

internal static class Extensions
{
    public static string Capitalize(this string value)
        => string.IsNullOrEmpty(value)
            ? string.Empty
            : $"{char.ToUpperInvariant(value[0])}{value[1..]}";

    public static string SanitizeIdentifier(this string identifier)
        => SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
            ? $"@{identifier}"
            : identifier;

    public static string ToLiteral(this object? value)
    {
        return value is null
            ? "null"
            : Map().ToString();

        SyntaxToken Map() => value switch
        {
            string stringValue => Literal(stringValue),
            int intValue => Literal(intValue),
            _ => throw new NotImplementedException($"Not implemented for type {value.GetType().AssemblyQualifiedName}"),
        };
    }
}
