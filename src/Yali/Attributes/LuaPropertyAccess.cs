using System;

namespace Yali.Attributes
{
    [Flags]
    public enum LuaPropertyAccess
    {
        None = 0,
        Readable = 1,
        Writeable = 2,
        Both = 3
    }
}
