namespace DarkLink.RoslynHelpers.AttributeGenerator.Test;

[TestClass]
public class GeneratorTest : VerifySourceGenerator
{
    [TestMethod]
    public async Task DefaultAttribute()
    {
        var source = @"
using DarkLink.RoslynHelpers;

[GenerateAttribute]
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
