# Yali
Yali is **y**et **a**nother **Lua** **i**nterpreter for .NET.

> **Note**: this project is still in development.
> Most of the code is tested by unit tests, but it's possible that the API will change in the future.

## Why
Why another interpreter? I needed a fully asynchronous interpreter for my web project. Most interpreters were synchronized and not thread-safe. Next to that I wanted full support for metatables and string patterns.

## Features
- Async
- Proxies
- Metatables
- String patterns

## Examples
### Creating the engine
```csharp
var engine = Engine.Create();		// Creates an empty engine (which contains no functions or variables whatsoever)

engine.AddLuaLibrary(); 		// Adds the Lua functions (e.g. assert, error, setmetatable).
engine.AddStringLibrary();		// Adds the string library (http://lua-users.org/wiki/StringLibraryTutorial)
engine.AddMathLibrary();		// Adds the math library (http://lua-users.org/wiki/MathLibraryTutorial)

// If you want all the library above, you can also use "Engine.CreateDefault":
var defaultEngine = Engine.CreateDefault();
```

### Proxy
```csharp
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

var engine = Engine.Create();

engine.Set("foo", new SharedClass());

await engine.ExecuteAsync(@"
	foo.print('Hello C#!')
	foo.wait(1000)
	foo.notice('Engine is automatically injected')
	foo:nonstatic()
");
```

## License
This project is MIT licensed.

The parser (found in [src/Yali/Runtime](src/Yali/Runtime)) is originally from [NetLua](https://github.com/frabert/NetLua) and is modified to support hex numbers, varargs and multi-line strings.
NetLua is also MIT licensed. The license can be found at [their repository](https://github.com/frabert/NetLua/blob/master/LICENSE) and at the headers of the parser files.