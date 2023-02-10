//HintName: DarkLink.RoslynHelpers.AttributeGenerator.InjectedCode.Attribute.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkLink.RoslynHelpers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class GenerateAttributeAttribute : Attribute
    {
    }
}
