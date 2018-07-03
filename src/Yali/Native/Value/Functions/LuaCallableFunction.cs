using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Native.Proxy;

namespace Yali.Native.Value.Functions
{
    public class LuaCallableFunction : LuaFunction
    {
        public static LuaFunction Instance = new LuaCallableFunction();

        public override Task<LuaArguments> CallAsync(Engine engine, LuaArguments args,
            CancellationToken token = default)
        {
            var first = args[0];

            if (first.IsNil())
            {
                throw new LuaException("bad argument #1");
            }

            var instance = (ICallableProxy)first.ToObject();

            return instance.CallAsync(Lua.Args(args.Skip(1)), token);
        }

        public override object ToObject()
        {
            return this;
        }
    }
}
