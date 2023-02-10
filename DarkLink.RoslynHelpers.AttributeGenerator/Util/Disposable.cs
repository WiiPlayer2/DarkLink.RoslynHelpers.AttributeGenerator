using System;

namespace DarkLink.RoslynHelpers.AttributeGenerator.Util;

internal static class Disposable
{
    private record ActionDisposable(Action Action) : IDisposable
    {
        public void Dispose() => Action();
    }

    public static IDisposable Create(Action action) => new ActionDisposable(action);
}
