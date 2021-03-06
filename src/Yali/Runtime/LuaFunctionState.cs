﻿using Yali.Native;

namespace Yali.Runtime
{
    public sealed class LuaFunctionState
    {
        public LuaFunctionState()
        {
            ReturnArguments = Lua.Args();
        }

        public LuaArguments ReturnArguments { get; private set; }

        public bool DidReturn { get; private set; }

        public void SetResult(LuaArguments args)
        {
            ReturnArguments = args;
            DidReturn = true;
        }
    }
}
