//HintName: DarkLink.RoslynHelpers.GenerateAttributeAttribute.g.cs
using System;

namespace DarkLink.RoslynHelpers
{
    [AttributeUsage((AttributeTargets)4, AllowMultiple = false, Inherited = true)]
    public class GenerateAttributeAttribute : Attribute
    {
        public GenerateAttributeAttribute(System.AttributeTargets validOn) { }
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }
        public string? Namespace { get; set; }
        public string? Name { get; set; }
    }
}
