﻿using System.Globalization;

namespace Yali.Native
{
    /// <summary>
    /// A Lua type
    /// 
    /// http://www.lua.org/pil/2.html
    /// </summary>
    public enum LuaType : byte
    {
        Nil,
        Boolean,
        Number,
        String,
        UserData,
        Function,
        Thread,
        Table
    }

    public static class LuaTypeExtensions
    {
        public static string ToLuaName(this LuaType type) => type.ToString().ToLower(CultureInfo.InvariantCulture);
    }
}