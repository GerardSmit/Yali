using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Attributes;
using Yali.Native;
using Yali.Native.Value;
using Yali.Utils;

namespace Yali.Libraries
{
    [LuaClass(DefaultMethodVisible = true)]
    public class StringLibrary
    {
        private static LuaArguments GetArgs(Match match)
        {
            return match.Groups.Count == 1
                ? Lua.Args(match.Value)
                : Lua.Args(match.Groups.Cast<Group>().Skip(1).Select(m => LuaObject.FromString(m.Value)));
        }

        public static LuaArguments Byte(string str, int startIndex = 1, int? endIndex = null)
        {
            startIndex -= 1;
            endIndex -= 1;

            var length = endIndex - startIndex ?? 1;

            if (startIndex < 0)
            {
                length = length + startIndex;
                startIndex = 0;
            }

            if (startIndex < 0 || length <= 0)
            {
                return Lua.Args();
            }

            return Lua.Args(str.Skip(startIndex).Take(1).Select(c => LuaObject.FromNumber(c)));
        }

        public static string Char(LuaArguments args)
        {
            return new string(args.Select(o => (char)o.AsNumber()).ToArray());
        }

        public static LuaArguments Find(string str, string pattern, int? startIndex = null, bool plain = false)
        {
            startIndex -= 1;

            if (!startIndex.HasValue)
            {
                startIndex = 0;
            }
            else if (startIndex < 0)
            {
                startIndex = str.Length - startIndex * -1;
            }

            if (plain)
            {
                var index = str.IndexOf(pattern, startIndex.Value, StringComparison.Ordinal);

                return index == -1 ? Lua.Args() : Lua.Args(1 + index, 1 + index + pattern.Length);
            }

            var regex = RegexUtils.FromPattern(pattern);
            var match = regex.Match(str, startIndex.Value);

            return !match.Success ? Lua.Args() : Lua.Args(1 + match.Index, 1 + match.Index + match.Length);
        }

        public static LuaArguments Match(string str, string pattern, int? startIndex = null)
        {
            startIndex -= 1;

            if (!startIndex.HasValue)
            {
                startIndex = 0;
            }
            else if (startIndex < 0)
            {
                startIndex = str.Length - startIndex * -1;
            }

            var regex = RegexUtils.FromPattern(pattern);
            var match = regex.Match(str, startIndex.Value);

            return !match.Success ? Lua.Args() : GetArgs(match);
        }

        public static int Len(string str)
        {
            return str.Length;
        }

        public static string Lower(string str)
        {
            return str.ToLower();
        }

        public static string Upper(string str)
        {
            return str.ToUpper();
        }

        public static string Reverse(string str)
        {
            var chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string Format(LuaArguments args)
        {
            return InternalStringUtils.Sprintf(args[0].AsString(), args.Skip(1).Select(i => i.ToObject()).ToArray());
        }

        public static string Rep(string str, int num)
        {
            return new StringBuilder(str.Length * num).Insert(0, str, num).ToString();
        }

        public static string Sub(string str, int start, int? end = null)
        {
            start -= 1;
            end -= 1;

            if (start < 0) start = str.Length + start + 1;
            if (end.HasValue && end < 0) end = str.Length + end.Value;

            return end.HasValue ? str.Substring(start, end.Value - start + 1) : str.Substring(start);
        }

        public static async Task<LuaArguments> Gsub(Engine engine, string str, string pattern, LuaObject replacement, int? limit = null, CancellationToken token = default)
        {
            var regex = RegexUtils.FromPattern(pattern);

            return await regex.ReplaceAsync(str, async match =>
            {
                if (replacement.IsFunction())
                {
                    return await replacement.CallAsync(engine, GetArgs(match), token).FirstAsync();
                }

                var repl = replacement.AsString();

                for (var i = 0; i < match.Groups.Count; i++)
                {
                    repl = repl.Replace($"%{i + 1}", match.Groups[i].Value);
                }

                return repl;
            }, limit);
        }
    }
}
