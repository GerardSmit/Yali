using System.Threading.Tasks;
using Yali.Tests.Extensions;
using Xunit;

namespace Yali.Tests
{
    public class LuaTest
    {
        private readonly Engine _engine;

        public LuaTest()
        {
            _engine = Engine.CreateDefault()
                .AddAssertLibrary();
        }

        [Theory]
        [MemberData(nameof(LuaDataSource.TestData), MemberType = typeof(LuaDataSource))]
        public async Task TestScript(Labeled<string> script)
        {
            await _engine.ExecuteAsync(script.Data);
        }
    }
}
