using Yali.Extensions;
using Yali.Tests.Libraries;

namespace Yali.Tests.Extensions
{
    public static class EngineExtensions
    {
        public static Engine AddAssertLibrary(this Engine engine)
        {
            engine.Set("assert", new AssertLibrary());

            return engine;
        }
    }
}
