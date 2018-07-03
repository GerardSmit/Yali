using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Native;
using Yali.Native.Value;
using Yali.Native.Value.Functions;

namespace Yali.Utils
{
    internal static class MethodUtils
    {
        private static object InvokeMethod(MethodBase method, Engine engine, LuaArguments args, CancellationToken token = default)
        {
            var instance = (object) null;
            var offset = 0;

            if (!method.IsStatic)
            {
                try
                {
                    if (args[0].IsNil())
                    {
                        throw new ArgumentException();
                    }

                    offset = 1;
                    instance = args[0].ToObject(method.DeclaringType);
                }
                catch (ArgumentException)
                {
                    throw new LuaException("bad argument #1");
                }
            }

            var index = offset;
            var parameters = method.GetParameters();
            var methodArgs = new object[parameters.Length];
            var argCount = args.Length - offset;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                // Convert the argument
                object arg;

                if (parameter.ParameterType == typeof(LuaArguments))
                {
                    arg = args;
                }
                else if (parameter.ParameterType == typeof(CancellationToken))
                {
                    arg = token;
                }
                else if (parameter.ParameterType == typeof(Engine))
                {
                    arg = engine;
                }
                else if (index >= argCount)
                {
                    if (!parameter.HasDefaultValue)
                    {
                        var paramCount = parameters.Count(p =>
                            p.ParameterType != typeof(LuaArguments) && 
                            p.ParameterType != typeof(CancellationToken) &&
                            p.ParameterType != typeof(Engine)
                        );

                        throw new LuaException($"expected {paramCount} arguments, got {argCount} instead");
                    }

                    arg = parameter.DefaultValue;
                }
                else
                {
                    try
                    {
                        arg = args[index++].ToObject(parameter.ParameterType);
                    }
                    catch (ArgumentException)
                    {
                        throw new LuaException($"bad argument #{index + 1}");
                    }
                }

                methodArgs[i] = arg;
            }

            return method.Invoke(instance, methodArgs);
        }

        private static LuaArguments CreateArgs(object obj)
        {
            if (obj is LuaArguments args)
            {
                return args;
            }

            return Lua.Args(LuaObject.FromObject(obj));
        }

        public static LuaFunction CreateFunction(MethodInfo method)
        {
            var returnType = method.ReturnType;
            var isTask = typeof(Task).IsAssignableFrom(returnType);

            // Non-async
            if (!isTask)
            {
                return new LuaDirectFunction((engine, args) => CreateArgs(InvokeMethod(method, engine, args)));
            }

            // Async
            var hasReturn = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);

            return new LuaAsyncFunction(async (engine, args, token) =>
            {
                var task = (Task)InvokeMethod(method, engine, args, token);

                await task;

                if (!hasReturn)
                {
                    return Lua.Args();
                }

                var returnProperty = task.GetType().GetProperty("Result") ?? throw new InvalidOperationException();

                return CreateArgs(returnProperty.GetValue(task));
            });
        }
    }
}
