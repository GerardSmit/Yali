using System.Threading.Tasks;
using Yali.Tests.Extensions;
using Xunit;
using Yali.Extensions;
using Yali.Tests.Proxies;
using Yali.Utils;

namespace Yali.Tests
{
    public class LuaTest
    {
        private readonly Engine _engine;

        public LuaTest()
        {
            _engine = Engine.CreateDefault()
                .AddAssertLibrary()
                .SetClass<UserManager>("userManager");
        }

        [Theory]
        [MemberData(nameof(LuaDataSource.TestData), MemberType = typeof(LuaDataSource))]
        public async Task TestScript(Labeled<string> script)
        {
            await _engine.ExecuteAsync(script.Data);
        }

        [Fact]
        public async Task TestParse()
        {
            var func = _engine.Parse("return true");
            var result = await _engine.ExecuteAsync(func);

            Assert.True(result[0].AsBool());
        }

        [Fact]
        public async Task TestLargeNumber()
        {
            var func = _engine.Parse("return 4294967294");
            var result = await _engine.ExecuteAsync(func);

            Assert.Equal(4294967294d, result[0].AsNumber());
        }
    }
}
