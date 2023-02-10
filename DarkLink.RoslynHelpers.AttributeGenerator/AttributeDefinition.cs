using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DarkLink.RoslynHelpers.AttributeGenerator;

internal record AttributeDefinition(GenerateAttributeData Data, INamedTypeSymbol Type, TypeKind TypeKind, IReadOnlyList<IParameterSymbol> Parameters)
{
    public string FullName
        => Namespace is not null
            ? $"{Namespace}.{Name}"
            : Name;

    public string Name => Data.Name ?? Type.Name;

    public string? Namespace
        => Data.Namespace
           ?? (Type.ContainingNamespace is {IsGlobalNamespace: false,}
               ? Type.ContainingNamespace.ToDisplayString()
               : default);
}
