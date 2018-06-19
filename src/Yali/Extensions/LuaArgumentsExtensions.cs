using System.Linq;
using System.Threading.Tasks;
using Yali.Native;
using Yali.Native.Value;

namespace Yali.Extensions
{
    public static class LuaArgumentsExtensions
    {
        public static async Task<LuaObject> FirstAsync(this Task<LuaArguments> task) => (await task)[0];

        public static async Task<LuaArguments> ToArgsAsync(this Task<LuaObject> task) => new LuaArguments(await task);

        public static void Expect(this LuaArguments args, int id, params LuaType[] types)
        {
            var obj = args[id];

            if (types.Any(t => obj.Type == t))
            {
                return;
            }

            var typeNames = string.Join(" or ", types.Select(t => t.ToLuaName()));
            throw new LuaException($"bad argument #{id + 1} to 'setmetatable' ({typeNames} expected)");
        }
    }
}
