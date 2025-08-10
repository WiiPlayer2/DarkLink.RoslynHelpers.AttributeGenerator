using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Consumer.Test;

[TestClass]
public class GeneratorTest
{
    [TestMethod]
    public void ParamsParameterSingleArgument()
    {
        // Arrange
        var source =
            /*lang=csharp*/
            """
            using DarkLink.RoslynHelpers.AttributeGenerator.Consumer.Test;
            
            [ParamsTest("hi")]
            internal class My;
            """;
        var compilation = Compile(source, context => context.RegisterPostInitializationOutput(ParamsTestAttribute.AddTo));
        var myType = compilation.GetSemanticModel(compilation.SyntaxTrees.First()).LookupNamespacesAndTypes(0, name: "My").First();
        var attribute = myType.GetAttributes().First();

        // Act
        var result = ParamsTestAttribute.From(attribute);

        // Assert
        result.Should().BeEquivalentTo(new ParamsTestAttribute("hi"));
    }
    
    [TestMethod]
    public void ParamsParameterMultipleArguments()
    {
        // Arrange
        var source =
            /*lang=csharp*/
            """
            using DarkLink.RoslynHelpers.AttributeGenerator.Consumer.Test;

            [ParamsTest("henlo", "dere")]
            internal class My;
            """;
        var compilation = Compile(source, context => context.RegisterPostInitializationOutput(ParamsTestAttribute.AddTo));
        var myType = compilation.GetSemanticModel(compilation.SyntaxTrees.First()).LookupNamespacesAndTypes(0, name: "My").First();
        var attribute = myType.GetAttributes().First();

        // Act
        var result = ParamsTestAttribute.From(attribute);

        // Assert
        result.Should().BeEquivalentTo(new ParamsTestAttribute("henlo", "dere"));
    }
    
    [TestMethod]
    public void ParamsParameterNoArguments()
    {
        // Arrange
        var source =
            /*lang=csharp*/
            """
            using DarkLink.RoslynHelpers.AttributeGenerator.Consumer.Test;

            [ParamsTest]
            internal class My;
            """;
        var compilation = Compile(source, context => context.RegisterPostInitializationOutput(ParamsTestAttribute.AddTo));
        var myType = compilation.GetSemanticModel(compilation.SyntaxTrees.First()).LookupNamespacesAndTypes(0, name: "My").First();
        var attribute = myType.GetAttributes().First();

        // Act
        var result = ParamsTestAttribute.From(attribute);

        // Assert
        result.Should().BeEquivalentTo(new ParamsTestAttribute());
    }

    private static Compilation Compile(string source, Action<IncrementalGeneratorInitializationContext> initialize)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var assemblyDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = new[]
            {
                typeof(object),
                typeof(Enumerable),
                typeof(IncrementalGeneratorPostInitializationContext),
                typeof(TextReader),
                typeof(ImmutableArray<>),
                typeof(AttributeTargets),
            }.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "System.Runtime.Extensions.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "System.Collections.dll")),
            });
        
        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] {syntaxTree,},
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TestSourceGenerator(initialize));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);

        return updatedCompilation;
    }
}

internal class TestSourceGenerator(Action<IncrementalGeneratorInitializationContext> initialize) : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) => initialize(context);
}

[GenerateAttribute(AttributeTargets.Class)]
internal partial record ParamsTestAttribute(params string[] Args);