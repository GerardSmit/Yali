using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yali.Attributes;
using Yali.Native;

namespace Yali.Examples._01._Proxy
{
    // To use a C# class in Lua you'll have to add the attribute [LuaClass].
    // By default the methods are not visible and can be registered with the attribute [LuaMethod].
    // You can change this behavior by setting the property "DefaultMethodVisible" to true.
    [LuaClass(DefaultMethodVisible = true)]
    public class SharedClass
    {
        // Yali will transform all the Lua objects to the correct C# types.
        public static void Print(string str) => Console.WriteLine(str);

        // Yali will automatically await the method if it returns a Task.
        public static Task Wait(int timeOut) => Task.Delay(timeOut);

        // The following types will be automatically added without the need of providing them in Lua:
        //  CancellationToken - The token that was given in "engine.ExecuteAsync" or "func.CallAsync".
        //  LuaArguments      - All the arguments given by Lua.
        //  Engine            - The engine that called the method.
        public static void Notice(Engine engine, LuaArguments args)
        {
            Console.WriteLine($"The engine {engine} said: {string.Join(", ", args.Select(a => a.AsString()))}");
        }

        // If the method is non-static Yali enforces the runtime to provide the SharedClass instance.
        // Which in Lua can be called with "instance:NonStatic()"
        public void NonStatic()
        {

        }
    }
}
