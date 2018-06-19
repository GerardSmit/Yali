using System;
using System.Threading;
using System.Threading.Tasks;
using Yali.Native;
using Yali.Native.Value;

namespace Yali.Extensions
{
    public static class LuaObjectExtensions
    {
        public static bool IsNil(this LuaObject obj) => obj.Type == LuaType.Nil;

        public static bool IsTable(this LuaObject obj) => obj.Type == LuaType.Table;

        public static bool IsFunction(this LuaObject obj) => obj.Type == LuaType.Function;

        public static bool IsNumber(this LuaObject obj) => obj.Type == LuaType.Number;

        public static bool IsString(this LuaObject obj) => obj.Type == LuaType.String;

        public static bool TryAsDouble(this LuaObject obj, out double dbl)
        {
            dbl = obj.AsNumber();
            return !double.IsNaN(dbl);
        }

        public static void NewIndexLocal(this LuaObject table, LuaObject key, Func<Engine, LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            table.NewIndexRaw(key, LuaObject.FromFunction(func));
        }

        public static void NewIndexLocal(this LuaObject table, LuaObject key, Func<Engine, LuaArguments, LuaArguments> func)
        {
            table.NewIndexRaw(key, LuaObject.FromFunction(func));
        }
    }
}
