//HintName: TestAttribute.g.cs
using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

partial class TestAttribute
{
    public const string ATTRIBUTE_NAME = "TestAttribute";

    public static void AddTo(IncrementalGeneratorPostInitializationContext context)
    {
        const string hintName = "TestAttribute.g.cs";
        const string code = @"using System;

[AttributeUsage((AttributeTargets)32767, AllowMultiple = false, Inherited = true)]
public class TestAttribute : Attribute
{
    public TestAttribute(string requiredArgument) { }
    public int OptionalArgument { get; set; }
}
";
        var sourceText = SourceText.From(code, new UTF8Encoding(false));
        context.AddSource(hintName, sourceText);
    }

    public static TestAttribute From(AttributeData data)
    {
        var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);
        var ___requiredArgument = (string)data.ConstructorArguments[0].Value!;
        var ___optionalArgument = GetNamedValueOrDefault<int>("OptionalArgument", 42);
        return new(requiredArgument: ___requiredArgument, optionalArgument: ___optionalArgument);
        T GetNamedValueOrDefault<T>(string name, T defaultValue) => namedArguments.TryGetValue(name, out var value) ? (T) value.Value! : defaultValue;
    }
}
