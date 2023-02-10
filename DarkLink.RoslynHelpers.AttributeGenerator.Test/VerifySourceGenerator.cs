using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Test;

public abstract class VerifySourceGenerator : VerifyBase
{
    protected Task Verify(string source, Action<Compilation, ImmutableArray<Diagnostic>>? verifyCompilation, Func<SettingsTask, SettingsTask>? configure = default)
    {
        configure ??= _ => _;

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

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new Generator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);

        verifyCompilation?.Invoke(updatedCompilation, diagnostics);

        return configure(Verify(driver))
            .UseDirectory("Snapshots");
    }

    protected Task Verify(string source, Func<SettingsTask, SettingsTask>? configure = default) =>
        Verify(source, (compilation, _) =>
        {
            var diagnostics = compilation.GetDiagnostics();
            var errors = string.Join(Environment.NewLine, diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error));
            Assert.IsFalse(errors.Any(), $"Compilation failed:\n{string.Join("\n", errors)}");
        }, configure);
}
