using System;
using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Util;

internal static class Extensions
{
    public static string Capitalize(this string value)
        => string.IsNullOrEmpty(value)
            ? string.Empty
            : $"{char.ToUpperInvariant(value[0])}{value[1..]}";

    public static IDisposable IndentScope(this IndentedTextWriter indentedTextWriter)
    {
        indentedTextWriter.Indent++;
        return Disposable.Create(() => indentedTextWriter.Indent--);
    }

    public static IDisposable ResetScope(this IndentedTextWriter indentedTextWriter)
    {
        var previousIndent = indentedTextWriter.Indent;
        indentedTextWriter.Indent = 0;
        return Disposable.Create(() => indentedTextWriter.Indent = previousIndent);
    }

    public static string SanitizeIdentifier(this string identifier)
        => SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
            ? $"@{identifier}"
            : identifier;

    public static string ToDefaultLiteral(this IParameterSymbol parameter)
    {
        if (parameter.Type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum)
            return $"(({parameter.Type.ToDisplayString()}){parameter.ExplicitDefaultValue.ToLiteral()})";

        return parameter.ExplicitDefaultValue.ToLiteral();
    }

    public static string ToLiteral(this object? value)
    {
        return Map().ToString();

        LiteralExpressionSyntax Map() => value switch
        {
            null => LiteralExpression(SyntaxKind.NullLiteralExpression),
            string stringValue => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(stringValue)),
            int intValue => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(intValue)),
            bool boolValue => LiteralExpression(boolValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
            _ => throw new NotImplementedException($"Not implemented for type {value.GetType().AssemblyQualifiedName}"),
        };
    }

    public static string Uncapitalize(this string value)
        => string.IsNullOrEmpty(value)
            ? string.Empty
            : $"{char.ToLowerInvariant(value[0])}{value[1..]}";
}
