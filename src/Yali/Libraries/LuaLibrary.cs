using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Native;
using Yali.Native.Value;

namespace Yali.Libraries
{
    public static class LuaLibrary
    {
        public static LuaArguments Assert(LuaArguments args)
        {
            if (args.Length > 0 && !args[0].AsBool())
            {
                throw new LuaException(args.Length == 1 ? "Assertion failed" : args[1].ToString());
            }

            return Lua.Args();
        }

        public static LuaArguments Error(LuaArguments args)
        {
            throw new LuaException(args[0].ToString());
        }

        public static LuaArguments GetMetaTable(Engine engine, LuaArguments args)
        {
            return Lua.Args(args[0].GetMetaTable(engine));
        }

        public static LuaArguments SetMetaTable(Engine engine, LuaArguments args)
        {
            args.Expect(0, LuaType.Table, LuaType.Nil);

            args[0].SetMetaTable(engine, args[1]);
            return Lua.Args();
        }

        public static LuaArguments RawEqual(LuaArguments args)
        {
            return Lua.Args(args[0].Equals(args[1]));
        }

        public static LuaArguments RawGet(LuaArguments args)
        {
            return Lua.Args(args[0].IndexRaw(args[1]));
        }

        public static LuaArguments Rawset(LuaArguments args)
        {
            args[0].NewIndexRaw(args[1], args[2]);
            return Lua.Args(args[0]);
        }

        public static LuaArguments RawLen(LuaArguments args)
        {
            return Lua.Args(args[0].Length);
        }

        public static LuaArguments ToNumber(LuaArguments args)
        {
            return Lua.Args(args[0].TryAsDouble(out var d) ? LuaObject.FromNumber(d) : LuaNil.Instance);
        }

        public static Task<LuaArguments> ToString(LuaArguments args, CancellationToken token = default)
        {
            return Lua.ArgsAsync(args[0].AsString());
        }

        public static LuaArguments Type(LuaArguments obj)
        {
            return Lua.Args(obj[0].Type.ToLuaName());
        }

        private static async Task<LuaArguments> GetNext(Engine engine, LuaArguments x, CancellationToken token = default)
        {
            var s = x[0];
            var var = x[1].AsNumber() + 1;
            var val = await s.IndexAsync(engine, var, token);

            return val.IsNil() ? Lua.Args(LuaNil.Instance) : Lua.Args(var, val);
        }

        public static Task<LuaArguments> Ipairs(Engine engine, LuaArguments args, CancellationToken token = default)
        {
            var handler = args[0].GetMetaMethod(engine, "__ipairs");

            if (!handler.IsNil())
            {
                return handler.CallAsync(engine, args, token);
            }

            if (!args[0].IsTable())
            {
                throw new LuaException("t must be a table");
            }

            return Lua.ArgsAsync(LuaObject.FromFunction(GetNext), args[0], 0);
        }

        public static Task<LuaArguments> Pairs(Engine engine, LuaArguments args, CancellationToken token = default)
        {
            var handler = args[0].GetMetaMethod(engine, "__pairs");

            return !handler.IsNil() 
                ? handler.CallAsync(engine, args, token) 
                : Lua.ArgsAsync(LuaObject.FromFunction(Next), args[0], LuaNil.Instance);
        }

        public static LuaArguments Next(Engine engine, LuaArguments args)
        {
            var table = args[0];
            var index = args[1];

            if (!table.IsTable())
            {
                throw new LuaException("t must be a table");
            }

            var keys = table.Keys.ToArray();

            if (index.IsNil())
            {
                return keys.Length == 0 
                    ? Lua.Args() 
                    : Lua.Args(keys[0], table.IndexRaw(keys[0]));
            }

            var pos = Array.IndexOf(keys, index);

            return pos == keys.Length - 1
                ? Lua.Args() 
                : Lua.Args(keys[pos + 1], table.IndexRaw(keys[pos + 1]));
        }

        public static LuaArguments Unpack(LuaArguments args)
        {
            return args;
        }
    }
}
