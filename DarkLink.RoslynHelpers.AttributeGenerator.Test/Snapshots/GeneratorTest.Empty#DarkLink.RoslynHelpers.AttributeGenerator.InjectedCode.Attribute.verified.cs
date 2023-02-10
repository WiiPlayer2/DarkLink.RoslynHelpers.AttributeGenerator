//HintName: DarkLink.RoslynHelpers.AttributeGenerator.InjectedCode.Attribute.cs
using System;

namespace DarkLink.RoslynHelpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class GenerateAttributeAttribute : Attribute
{
    public GenerateAttributeAttribute(AttributeTargets validOn) { }

    public bool AllowMultiple { get; set; }

    public bool Inherited { get; set; }
}
