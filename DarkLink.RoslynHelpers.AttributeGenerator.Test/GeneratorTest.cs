namespace DarkLink.RoslynHelpers.AttributeGenerator.Test;

[TestClass]
public class GeneratorTest : VerifySourceGenerator
{
    public static IEnumerable<object[]> TypedOptionalArgumentsData => Types.All.Select(t => new[] {t.TypeName, t.ExampleValue});

    public static IEnumerable<object[]> TypedRequiredArgumentsData => Types.All.Select(t => new[] {t.TypeName});

    public static IEnumerable<object[]> TypedRequiredScalarArgumentsData => Types.Scalars.Select(t => new[] {t.TypeName});

    [TestMethod]
    public async Task AttributeWithConstructorAndOtherNameAndNamespace()
    {
        var source =
            /*lang=csharp*/
            """
            using System;
            using DarkLink.RoslynHelpers;

            [GenerateAttribute(AttributeTargets.All, Name = "OtherNameAttribute", Namespace = "OtherNamespace.Here")]
            public partial class TestAttribute
            {
                public TestAttribute(string requiredArgument, int optionalArgument = 42) { }
            }
            """;

        await Verify(source);
    }

    [TestMethod]
    public async Task ComplexAttribute()
    {
        var source =
            /*lang=csharp*/
            """
            using System;
            using DarkLink.RoslynHelpers;

            [GenerateAttribute(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
            public partial class TestAttribute
            {
            }
            """;

        await Verify(source);
    }

    [TestMethod]
    public async Task DefaultAttribute()
    {
        var source =
            /*lang=csharp*/
            """
            using System;
            using DarkLink.RoslynHelpers;

            [GenerateAttribute(AttributeTargets.All)]
            public partial class TestAttribute
            {
            }
            """;

        await Verify(source);
    }

    [TestMethod]
    public async Task DefaultAttributeWithConstructor()
    {
        var source =
            /*lang=csharp*/
            """
            using System;
            using DarkLink.RoslynHelpers;

            [GenerateAttribute(AttributeTargets.All)]
            public partial class TestAttribute
            {
                public TestAttribute(string requiredArgument, int optionalArgument = 42) { }
            }
            """;

        await Verify(source);
    }

    [TestMethod]
    public async Task Empty()
    {
        var source = string.Empty;

        await Verify(source);
    }

    [TestMethod]
    public async Task GeneratorAttributeCode()
    {
        var source =
            /*lang=csharp*/
            """
            using System;

            namespace DarkLink.RoslynHelpers.AttributeGenerator;

            [GenerateAttribute(AttributeTargets.Class, Namespace = "DarkLink.RoslynHelpers", Name = "GenerateAttributeAttribute")]
            internal partial record GenerateAttributeData2(
                AttributeTargets ValidOn,
                bool AllowMultiple = false,
                bool Inherited = true,
                string? Namespace = default,
                string? Name = default);
            """;

        await Verify(source);
    }

    [TestMethod]
    public async Task ParameterlessRecord()
    {
        var source =
            /*lang=csharp*/
            """
            using System;

            namespace DarkLink.RoslynHelpers.AttributeGenerator;

            [GenerateAttribute(AttributeTargets.All)]
            internal partial record Marker();
            """;

        await Verify(source);
    }

    [TestMethod]
    public async Task ConstructorlessRecord()
    {
        var source =
            /*lang=csharp*/
            """
            using System;

            namespace DarkLink.RoslynHelpers.AttributeGenerator;

            [GenerateAttribute(AttributeTargets.All)]
            internal partial record Marker;
            """;

        await Verify(source);
    }

    [DynamicData(nameof(TypedOptionalArgumentsData))]
    [DataTestMethod]
    public async Task TypedOptionalArgument(string typeName, string defaultValue)
    {
        var source =
            /*lang=csharp*/
            $"""
             using System;

             namespace DarkLink.RoslynHelpers.AttributeGenerator;

             [GenerateAttribute(AttributeTargets.All)]
             internal partial record TypedData({typeName} argument = {defaultValue});
             """;

        await Verify(source, task => task.UseParameters(typeName));
    }

    [DynamicData(nameof(TypedRequiredArgumentsData))]
    [DataTestMethod]
    public async Task TypedRequiredArgument(string typeName)
    {
        var source =
            /*lang=csharp*/
            $"""
             using System;

             namespace DarkLink.RoslynHelpers.AttributeGenerator;

             [GenerateAttribute(AttributeTargets.All)]
             internal partial record TypedData({typeName} argument);
             """;

        await Verify(source, task => task.UseParameters(typeName));
    }

    [TestMethod]
    [DynamicData(nameof(TypedRequiredScalarArgumentsData))]
    public async Task ParamsParameter(string typeName)
    {
        var source =
            /*lang=csharp*/
            $"""
             using System;

             namespace DarkLink.RoslynHelpers.AttributeGenerator;

             [GenerateAttribute(AttributeTargets.All)]
             internal partial record ParamsParameter(params {typeName}[] argument);
             """;

        await Verify(source, task => task.UseParameters(typeName));
    }

    public static class Types
    {
        public static IEnumerable<TypeInfo> All => Scalars.Concat(Arrays);

        public static IEnumerable<TypeInfo> Arrays => Scalars.Select(t => new TypeInfo($"{t.TypeName}[]", "null"));

        public static IEnumerable<TypeInfo> Enums => new TypeInfo[]
        {
            new("System.AttributeTargets", "System.AttributeTargets.All"),
        };

        public static IEnumerable<TypeInfo> Primitives => new TypeInfo[]
        {
            new("bool", "true"),
            new("byte", "0xFF"),
            new("char", "'?'"),
            new("double", "13.37"),
            new("float", "13.37f"),
            new("int", "-42069"),
            new("long", "-42069L"),
            new("sbyte", "-0x1F"),
            new("short", "-69"),
            new("string", "\"henlo dere\""),
            new("uint", "42069U"),
            new("ulong", "42069UL"),
            new("ushort", "69"),
        };

        public static IEnumerable<TypeInfo> Scalars => Primitives.Concat(Specials).Concat(Enums);

        public static IEnumerable<TypeInfo> Specials => new TypeInfo[]
        {
            new("object", "null"),
            new("Microsoft.CodeAnalysis.INamedTypeSymbol", "null"),
        };

        public record TypeInfo(string TypeName, string ExampleValue);
    }
}
