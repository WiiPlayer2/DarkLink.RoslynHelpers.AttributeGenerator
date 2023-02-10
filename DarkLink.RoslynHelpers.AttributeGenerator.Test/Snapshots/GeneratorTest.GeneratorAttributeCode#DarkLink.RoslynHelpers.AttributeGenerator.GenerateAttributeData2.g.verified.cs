//HintName: DarkLink.RoslynHelpers.AttributeGenerator.GenerateAttributeData2.g.cs
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
partial class GenerateAttributeData2
{
public const string ATTRIBUTE_NAME = "DarkLink.RoslynHelpers.GenerateAttributeAttribute";
public static void AddTo(IncrementalGeneratorPostInitializationContext context)
{
const string hintName = "DarkLink.RoslynHelpers.AttributeGenerator.GenerateAttributeData2.g.cs";
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
var ___validOn = (System.AttributeTargets)data.ConstructorArguments[0].Value!;
var ___allowMultiple = GetNamedValueOrDefault<bool>("AllowMultiple", false);
var ___inherited = GetNamedValueOrDefault<bool>("Inherited", true);
var ___namespace = GetNamedValueOrDefault<string?>("Namespace", null);
var ___name = GetNamedValueOrDefault<string?>("Name", null);
return new(validOn: ___validOn, allowMultiple: ___allowMultiple, inherited: ___inherited, @namespace: ___namespace, name: ___name);
T GetNamedValueOrDefault<T>(string name, T defaultValue)
                => namedArguments.TryGetValue(name, out var value)
                    ? (T) value.Value!
                    : defaultValue;
}
}
}
}
}
