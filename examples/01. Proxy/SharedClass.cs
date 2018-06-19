using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Yali.Attributes;

namespace Yali.Examples._01._Proxy
{
    // By default the methods are not visible.
    // You can change behavior with "DefaultMethodVisible = true".
    [LuaClass(DefaultMethodVisible = true)]
    public class SharedClass
    {
        // Yali will transform all the parameters to the correct type.
        public static void Print(string str) => Console.WriteLine(str);

        // Yali will automatically await the method if it returns a Task.
        public static Task Wait(int timeOut) => Task.Delay(timeOut);

        // The following types will be automatically added without the need of providing them in Lua:
        //  CancellationToken - The token given at func.CallAsync.
        //  LuaArguments      - All the arguments given by Lua.
        //  Engine			  - The engine that called the method.
        public static void Notice(Engine engine, string str)
        {
            Console.WriteLine($"The engine {engine} said: {str}");
        }

        // If the method is non-static Yali enforces the runtime to provide the SharedClass instance.
        // Which in Lua can be called with "instance:NonStatic()"
        public void NonStatic()
        {

        }
    }
}
