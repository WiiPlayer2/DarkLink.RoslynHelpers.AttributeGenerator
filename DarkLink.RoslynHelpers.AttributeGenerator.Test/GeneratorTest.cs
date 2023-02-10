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
public partial class TestAttribute
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
public partial class TestAttribute
{
}
";

        await Verify(source);
    }

    [TestMethod]
    public async Task DefaultAttributeWithConstructor()
    {
        var source = @"
using System;
using DarkLink.RoslynHelpers;

[GenerateAttribute(AttributeTargets.All)]
public partial class TestAttribute
{
    public TestAttribute(string requiredArgument, int optionalArgument = 42) { }
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
