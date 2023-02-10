using System.Runtime.CompilerServices;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Test
{
    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Init() => VerifySourceGenerators.Enable();
    }
}