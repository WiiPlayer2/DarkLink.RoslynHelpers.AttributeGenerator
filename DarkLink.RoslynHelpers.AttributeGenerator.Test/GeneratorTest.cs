namespace DarkLink.RoslynHelpers.AttributeGenerator.Test;

[TestClass]
public class GeneratorTest : VerifySourceGenerator
{
    [TestMethod]
    public async Task ComplexAttribute()
    {
        var source = @"
using System;
using DarkLink.RoslynHelpers;

[GenerateAttribute(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public class TestAttribute
{
}
";

        await Verify(source);
    }

    [TestMethod]
    public async Task DefaultAttribute()
    {
        var source = @"
using System;
using DarkLink.RoslynHelpers;

[GenerateAttribute(AttributeTargets.All)]
public class TestAttribute
{
}
";

        await Verify(source);
    }

    [TestMethod]
    public async Task Empty()
    {
        var source = string.Empty;

        await Verify(source);
    }
}
