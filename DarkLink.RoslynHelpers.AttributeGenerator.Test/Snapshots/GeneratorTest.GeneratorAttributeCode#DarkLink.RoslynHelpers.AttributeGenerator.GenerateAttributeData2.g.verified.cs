﻿//HintName: DarkLink.RoslynHelpers.AttributeGenerator.GenerateAttributeData2.g.cs
using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DarkLink
{
namespace RoslynHelpers
{
namespace AttributeGenerator
{
partial record GenerateAttributeData2
{
public const string ATTRIBUTE_NAME = "DarkLink.RoslynHelpers.GenerateAttributeAttribute";
public static void AddTo(IncrementalGeneratorPostInitializationContext context)
{
const string hintName = "DarkLink.RoslynHelpers.GenerateAttributeAttribute.g.cs";
const string code = @"
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
T GetNamedValueOrDefault<T>(string name, T defaultValue)
                => namedArguments.TryGetValue(name, out var value)
                    ? (T) value.Value!
                    : defaultValue;
}
}
}
}
}
