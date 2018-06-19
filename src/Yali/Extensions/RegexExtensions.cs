using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yali.Native;

namespace Yali.Extensions
{
    internal static class RegexExtensions
    {
        public static async Task<LuaArguments> ReplaceAsync(this Regex regex, string input, Func<Match, Task<string>> replacementFn, int? max = null)
        {
            var sb = new StringBuilder();
            var lastIndex = 0;
            var counter = 0;

            foreach (Match match in regex.Matches(input))
            {
                if (max.HasValue && counter >= max.Value)
                {
                    break;
                }

                sb.Append(input, lastIndex, match.Index - lastIndex)
                    .Append(await replacementFn(match).ConfigureAwait(false));

                lastIndex = match.Index + match.Length;
                counter++;
            }

            sb.Append(input, lastIndex, input.Length - lastIndex);

            return Lua.Args(sb.ToString(), counter);
        }
    }
}
