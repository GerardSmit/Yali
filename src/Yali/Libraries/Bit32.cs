using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yali.Attributes;
using Yali.Native;

namespace Yali.Libraries
{
    [LuaClass(DefaultMethodVisible = true)]
    public class Bit32
    {
        public static int Band(LuaArguments args)
        {
            return args.Select(o => o.AsInt()).Aggregate((l, r) => l & r);
        }

        public static uint Bnot(uint l)
        {
            return ~l;
        }

        public static int Bor(LuaArguments args)
        {
            return args.Select(o => o.AsInt()).Aggregate((l, r) => l | r);
        }

        public static int Bxor(LuaArguments args)
        {
            return args.Select(o => o.AsInt()).Aggregate((l, r) => l ^ r);
        }

        public static int Lshift(int x, int disp)
        {
            return x << disp;
        }

        public static int Rshift(int x, int disp)
        {
            return x >> disp;
        }
        public static int Lrotate(int value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static int Rrotate(int value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
    }
}
