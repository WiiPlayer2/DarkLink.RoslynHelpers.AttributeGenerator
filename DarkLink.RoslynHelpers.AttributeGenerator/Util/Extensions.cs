using System;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Util;

internal static class Extensions
{
    public static string Capitalize(this string value)
        => string.IsNullOrEmpty(value)
            ? string.Empty
            : $"{char.ToUpperInvariant(value[0])}{value[1..]}";

    public static string ToLiteral(this object? value)
    {
        return Map().ToString();

        SyntaxToken Map() => value switch
        {
            int intValue => Literal(intValue),
            _ => throw new NotImplementedException(),
        };
    }
}
