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

namespace Yali.Libraries
{
    [LuaClass(DefaultMethodVisible = true)]
    public class StringLibrary
    {
        private static string GetMagicPattern(char c)
        {
            switch (c)
            {
                case '.':
                    return ".";
                case 'a':
                    return "[a-zA-Z]";
                case 'A':
                    return "[^a-zA-Z]";
                case 'l':
                    return "[a-z]";
                case 'L':
                    return "[^a-z]";
                case 'u':
                    return "[A-Z]";
                case 'U':
                    return "[^A-Z]";
                case 'p':
                    return "[#^;,.]";
                case 'P':
                    return "[^#^;,.]";
                case 'w':
                    return "[a-zA-Z0-9]";
                case 'W':
                    return "[^a-zA-Z0-9]";
                case 'd':
                    return "[0-9]";
                case 'D':
                    return "[^0-9]";
                case 's':
                    return @"\s";
                case 'S':
                    return @"\S";
                case 'c':
                    return @"[\x00-\x1F\x7F]";
                case 'C':
                    return @"[^\x00-\x1F\x7F]";
                case 'x':
                    return "[0-9A-F]";
                case 'X':
                    return "[^0-9A-F]";
                case 'z':
                    return @"\0";
                case 'Z':
                    return @"[^\0]";
                default:
                    return Regex.Escape(c.ToString());
            }
        }

        private static Regex CompilePattern(string luaPattern)
        {
            var pattern = new StringBuilder();
            var lastWasQuantifier = false;
            var skipQuantifier = true;
            var openSets = 0;
            var openGroups = 0;

            for (var i = 0; i < luaPattern.Length; i++)
            {
                var c = luaPattern[i];

                switch (c)
                {
                    // Escape and character classes
                    case '%':
                        if (++i >= luaPattern.Length)
                        {
                            pattern.Append('%');
                            continue;
                        }

                        c = luaPattern[i];

                        if (c == 'b')
                        {
                            if (i + 2 >= luaPattern.Length) throw new LuaException("malformed pattern (ends with '%')");
                            var start = luaPattern[++i];
                            var end = luaPattern[++i];

                            throw new NotImplementedException();
                        }
                        else
                        {
                            lastWasQuantifier = false;
                            skipQuantifier = false;
                            pattern.Append(GetMagicPattern(c));
                        }

                        break;

                    // Sets
                    case '[':
                        openSets++;
                        lastWasQuantifier = false;
                        skipQuantifier = true;
                        pattern.Append('[');
                        break;
                    case ']' when openSets > 0:
                        openSets--;
                        lastWasQuantifier = false;
                        skipQuantifier = false;
                        pattern.Append(']');
                        break;

                    // Groups
                    case '(':
                        openGroups++;
                        lastWasQuantifier = false;
                        skipQuantifier = true;
                        pattern.Append('(');
                        break;
                    case ')' when openGroups > 0:
                        openGroups--;
                        lastWasQuantifier = false;
                        skipQuantifier = false;
                        pattern.Append(')');
                        break;

                    // Anchors
                    case '^' when i == 0:
                        lastWasQuantifier = false;
                        skipQuantifier = true;
                        pattern.Append('^');
                        break;
                    case '$' when i == luaPattern.Length - 1:
                        lastWasQuantifier = false;
                        skipQuantifier = true;
                        pattern.Append('$');
                        break;

                    // Quantifiers
                    case '?':
                    case '*':
                    case '+':
                        if (skipQuantifier)
                        {
                            goto default;
                        }

                        lastWasQuantifier = true;
                        skipQuantifier = true;
                        pattern.Append(c);
                        break;
                    case '-' when i > 0 && lastWasQuantifier:
                        lastWasQuantifier = false;
                        skipQuantifier = true;
                        pattern.Append('?');
                        break;

                    default:
                        lastWasQuantifier = false;
                        skipQuantifier = false;
                        pattern.Append(Regex.Escape(c.ToString()));
                        break;
                }
            }

            return new Regex(pattern.ToString());
        }

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

            var regex = CompilePattern(pattern);
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

            var regex = CompilePattern(pattern);
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
            var regex = CompilePattern(pattern);

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
