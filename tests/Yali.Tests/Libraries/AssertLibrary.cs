﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Yali.Attributes;
using Yali.Native;
using Yali.Native.Proxy;
using Yali.Native.Value;
using Yali.Runtime.Ast;

namespace Yali.Tests.Libraries
{
    [LuaClass(DefaultMethodVisible = true)]
    public class AssertLibrary : ICallableProxy
    {
        [LuaMethod("Equal")]
        public static async Task Equal(Engine engine, LuaObject left, LuaObject right)
        {
            var areEqual = await LuaObject.BinaryOperationAsync(engine, BinaryOp.Equal, left, right);

            if (!areEqual)
            {
                throw new EqualException(left, right);
            }
        }

        [LuaMethod("NotEqual")]
        public static async Task NotEqual(Engine engine, LuaObject left, LuaObject right)
        {
            var areDifferent = await LuaObject.BinaryOperationAsync(engine, BinaryOp.Different, left, right);

            if (!areDifferent)
            {
                throw new NotEqualException(left, right);
            }
        }

        [LuaMethod("True")]
        public static void True(LuaObject obj)
        {
            Assert.True(obj.AsBool());
        }

        [LuaMethod("False")]
        public static void False(LuaObject obj)
        {
            Assert.False(obj.AsBool());
        }

        [LuaMethod("Throws")]
        public static Task Throws(Engine engine, LuaObject obj, LuaArguments args)
        {
            return Assert.ThrowsAsync<LuaException>(() => obj.CallAsync(engine, Lua.Args(args.Skip(1))));
        }

        public Task<LuaArguments> CallAsync(LuaArguments args, CancellationToken token = default)
        {
            Assert.True(args[0].ToBoolean());

            return Lua.ArgsAsync();
        }
    }
}