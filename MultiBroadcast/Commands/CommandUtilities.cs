using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiBroadcast.Commands
{
    public static class CommandUtilities
    {
        public static bool GetIntArguments(string text, out int[] args)
        {
            string[] source = text.Split('.');
            for (int index = 0; index < source.Length; ++index)
            {
                int result;
                if (!int.TryParse(source[index], out result))
                {
                    args = Array.Empty<int>();
                    return false;
                }
                source[index] = result.ToString();
            }
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            //args = ((IEnumerable<string>)source).Select<string, int>(CommandUtilities.\u003C\u003EO.\u003C0\u003E__Parse ?? (CommandUtilities.\u003C\u003EO.\u003C0\u003E__Parse = new Func<string, int>(int.Parse))).ToArray<int>();
            args = source.Select(int.Parse).ToArray();

            return true;
        }

        public static string GetStringFromArray<T>(IEnumerable<T> array) => string.Join<T>(", ", array);
    }
}
