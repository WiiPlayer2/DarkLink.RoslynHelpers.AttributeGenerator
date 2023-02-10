//HintName: DarkLink.RoslynHelpers.AttributeGenerator.TypedData.g.cs
using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DarkLink.RoslynHelpers.AttributeGenerator
{
    partial record TypedData
    {
        public const string ATTRIBUTE_NAME = "DarkLink.RoslynHelpers.AttributeGenerator.TypedData";

        public static void AddTo(IncrementalGeneratorPostInitializationContext context)
        {
            const string hintName = "DarkLink.RoslynHelpers.AttributeGenerator.TypedData.g.cs";
            const string code = @"using System;

namespace DarkLink.RoslynHelpers.AttributeGenerator
{
    [AttributeUsage((AttributeTargets)32767, AllowMultiple = false, Inherited = true)]
    public class TypedData : Attribute
    {
        public TypedData() { }
        public int[] Argument { get; set; }
    }
}
";
            var sourceText = SourceText.From(code, new UTF8Encoding(false));
            context.AddSource(hintName, sourceText);
        }

        public static TypedData From(AttributeData data)
        {
            var namedArguments = data.NamedArguments.ToDictionary(o => o.Key, o => o.Value);
            var ___argument = GetNamedValueOrDefault<int[]>("Argument", null);
            return new(argument: ___argument);
            T GetNamedValueOrDefault<T>(string name, T defaultValue) => namedArguments.TryGetValue(name, out var value) ? (T) value.Value! : defaultValue;
        }
    }
}
