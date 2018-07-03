using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Yali.Native;

namespace Yali.Utils
{
    public static class RegexUtils
    {
        private static string GetMagicPattern(char c)
        {
            switch (c)
            {
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

        public static Regex FromPattern(string luaPattern)
        {
            var pattern = new StringBuilder();
            var current = pattern;
            var openGroups = 0;
            var insideSet = false;
            var addRange = false;
            var sets = new List<string>();

            for (var i = 0; i < luaPattern.Length; i++)
            {
                var c = luaPattern[i];
                var startIndex = current.Length - 1;

                switch (c)
                {
                    // Escape and character classes
                    case '%':
                        if (++i >= luaPattern.Length)
                        {
                            current.Append('%');
                            continue;
                        }

                        c = luaPattern[i];

                        if (c == 'b')
                        {
                            if (i + 2 >= luaPattern.Length) throw new LuaException("malformed pattern");
                            var start = luaPattern[++i];
                            var end = luaPattern[++i];

                            throw new NotImplementedException();
                        }
                        else if (insideSet)
                        {
                            sets.Add(GetMagicPattern(c));
                        }
                        else
                        {
                            current.Append(GetMagicPattern(c));
                        }

                        break;

                    case '.':
                        current.Append('.');
                        break;

                    // Sets
                    case '-' when !addRange:
                        addRange = true;
                        continue;

                    case '[' when !insideSet:
                        current = new StringBuilder();
                        current.Append('[');

                        if (luaPattern[i + 1] == '^')
                        {
                            current.Append("^");
                            i++;
                        }

                        insideSet = true;

                        break;

                    case ']' when insideSet:
                        current.Append(']');
                        insideSet = false;

                        // Add the set.
                        var set = current.ToString();

                        if (set != "[]" && set != "[^]")
                        {
                            sets.Add(set);
                        }

                        pattern.Append(sets.Count > 1 ? $"(?:{string.Join("|", sets)})" : sets[0]);

                        // Reset
                        current = pattern;
                        sets.Clear();

                        break;

                    // Groups
                    case '(' when !insideSet:
                        openGroups++;
                        current.Append('(');
                        break;
                    case ')' when !insideSet && openGroups > 0:
                        openGroups--;
                        current.Append(')');
                        break;

                    // Anchors
                    case '^' when i == 0:
                        current.Append('^');
                        break;
                    case '$' when i == luaPattern.Length - 1:
                        current.Append('$');
                        break;

                    // Quantifiers
                    case '?' when !insideSet:
                    case '*' when !insideSet:
                    case '+' when !insideSet:
                        current.Append(c);
                        break;
                    case '-' when !insideSet && i > 0:
                        current.Append('?');
                        break;

                    default:
                        if (addRange)
                        {
                            current.Append("-");
                            addRange = false;
                        }

                        current.Append(Regex.Escape(c.ToString()));
                        break;
                }

                if (addRange)
                {
                    current.Insert(startIndex, Regex.Escape("-"));
                    addRange = false;
                }
            }

            return new Regex(current.ToString());
        }
    }
}
