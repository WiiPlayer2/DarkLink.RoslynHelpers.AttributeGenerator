using System;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Test
{
    [TestClass]
    public class GeneratorTest : VerifySourceGenerator
    {
        [TestMethod]
        public async Task Empty()
        {
            var source = string.Empty;

            await Verify(source);
        }
    }
}