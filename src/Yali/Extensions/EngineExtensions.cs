using System;
using System.Threading;
using System.Threading.Tasks;
using Yali.Libraries;
using Yali.Native;
using Yali.Native.Value;

namespace Yali.Extensions
{
    public static class EngineExtensions
    {
        public static Engine Set(this Engine engine, LuaObject key, LuaObject value)
        {
            engine.Global.NewIndexRaw(key, value);
            return engine;
        }

        public static LuaObject Get(this Engine engine, LuaObject key)
        {
            return engine.Global.IndexRaw(key);
        }

        public static Engine Set(this Engine engine, LuaObject key, Func<Engine, LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            return engine.Set(key, LuaObject.FromFunction(func));
        }

        public static Engine Set(this Engine engine, LuaObject key, Func<Engine, LuaArguments, LuaArguments> func)
        {
            return engine.Set(key, LuaObject.FromFunction(func));
        }

        public static Engine Set(this Engine engine, LuaObject key, Func<LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            return engine.Set(key, LuaObject.FromFunction((e, a, t) => func(a, t)));
        }

        public static Engine Set(this Engine engine, LuaObject key, Func<LuaArguments, LuaArguments> func)
        {
            return engine.Set(key, LuaObject.FromFunction((e, a) => func(a)));
        }
        
        public static Engine Set(this Engine engine, LuaObject key, object value)
        {
            return engine.Set(key, LuaObject.FromObject(value));
        }

        public static Engine AddMathLibrary(this Engine engine)
        {
            engine.Set("math", new MathLibrary());

            return engine;
        }

        public static Engine AddStringFunctions(this Engine engine, LuaObject table)
        {
            var current = engine.StringMetaTable.IndexRaw("__index");

            if (current.IsNil())
            {
                engine.StringMetaTable.NewIndexRaw("__index", table);
            }
            else
            {
                foreach (var key in table.Keys)
                {
                    current.NewIndexRaw(key, table.IndexRaw(key));
                }
            }

            return engine;
        }

        public static Engine AddLuaLibrary(this Engine engine)
        {
            engine.Set("assert", LuaLibrary.Assert);
            engine.Set("error", LuaLibrary.Error);
            engine.Set("getmetatable", LuaLibrary.GetMetaTable);
            engine.Set("setmetatable", LuaLibrary.SetMetaTable);
            engine.Set("rawequal", LuaLibrary.RawEqual);
            engine.Set("rawget", LuaLibrary.RawGet);
            engine.Set("rawset", LuaLibrary.Rawset);
            engine.Set("rawlen", LuaLibrary.RawLen);
            engine.Set("tonumber", LuaLibrary.ToNumber);
            engine.Set("tostring", LuaLibrary.ToString);
            engine.Set("type", LuaLibrary.Type);
            engine.Set("ipairs", LuaLibrary.Ipairs);
            engine.Set("next", LuaLibrary.Next);
            engine.Set("pairs", LuaLibrary.Pairs);

            return engine;
        }

        public static Engine AddStringLibrary(this Engine engine)
        {
            var table = LuaObject.FromObject(new StringLibrary());

            engine.Set("string", table);
            engine.AddStringFunctions(table);

            return engine;
        }
    }
}
