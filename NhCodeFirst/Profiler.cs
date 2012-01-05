using System;

namespace NhCodeFirst
{
    public static class Profiler
    {
        static Profiler()
        {
            Step = s => new DummyDisposable();
        }

        public static Func<string, IDisposable> Step { get; set; }
    }

    public class DummyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
