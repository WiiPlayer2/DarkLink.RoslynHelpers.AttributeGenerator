namespace DarkLink.RoslynHelpers.AttributeGenerator.Test;

[TestClass]
public class GeneratorTest : VerifySourceGenerator
{
    [TestMethod]
    public async Task AttributeWithConstructorAndOtherNameAndNamespace()
    {
        var source = @"
using System;
using DarkLink.RoslynHelpers;

[GenerateAttribute(AttributeTargets.All, Name = ""OtherNameAttribute"", Namespace = ""OtherNamespace.Here"")]
public partial class TestAttribute
{
    public TestAttribute(string requiredArgument, int optionalArgument = 42) { }
}
";

        await Verify(source);
    }

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

    [TestMethod]
    public async Task GeneratorAttributeCode()
    {
        var source = @"
using System;

namespace DarkLink.RoslynHelpers.AttributeGenerator;

[GenerateAttribute(AttributeTargets.Class, Namespace = ""DarkLink.RoslynHelpers"", Name = ""GenerateAttributeAttribute"")]
internal partial class GenerateAttributeData2
{
    public AttributeTargets ValidOn { get; }

    public bool AllowMultiple { get; }

    public bool Inherited { get; }

    public string? Namespace { get; }

    public string? Name { get; }

    public GenerateAttributeData2(AttributeTargets validOn, bool allowMultiple = false, bool inherited = true, string? @namespace = default, string? @name = default)
    {
        ValidOn = validOn;
        AllowMultiple = allowMultiple;
        Inherited = inherited;
        Namespace = @namespace;
        Name = name;
    }
}
";

        await Verify(source);
    }
}
