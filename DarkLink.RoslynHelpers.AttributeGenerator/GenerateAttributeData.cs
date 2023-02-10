using System;

namespace DarkLink.RoslynHelpers.AttributeGenerator;

[GenerateAttribute(AttributeTargets.Class, Namespace = "DarkLink.RoslynHelpers", Name = "GenerateAttributeAttribute")]
internal partial class GenerateAttributeData
{
    public GenerateAttributeData(AttributeTargets validOn, bool allowMultiple = false, bool inherited = true, string? @namespace = default, string? name = default)
    {
        ValidOn = validOn;
        AllowMultiple = allowMultiple;
        Inherited = inherited;
        Namespace = @namespace;
        Name = name;
    }

    public bool AllowMultiple { get; }

    public bool Inherited { get; }

    public string? Name { get; }

    public string? Namespace { get; }

    public AttributeTargets ValidOn { get; }
}
