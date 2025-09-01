using System;

namespace DarkLink.RoslynHelpers.AttributeGenerator;

[GenerateAttribute(AttributeTargets.Class, Namespace = "DarkLink.RoslynHelpers", Name = "GenerateAttributeAttribute")]
internal partial record GenerateAttributeData(
    AttributeTargets ValidOn,
    bool AllowMultiple = false,
    bool Inherited = true,
    string? Namespace = default,
    string? Name = default,
    string? Docs = default);
