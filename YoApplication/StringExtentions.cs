using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoApplication
{
    public static class StringExtentions
    {
        public static string[] GetBlocks(this string s)
        {
            string separators = " '.,!:;?\n\r\t()-\'\"";
            var list = new List<string>();
            for (var i = 0; i < s.Length; i++)
            {
                if (!separators.Contains(s[i]))
                {
                    var countCharNotSeparators = 0;
                    while (!separators.Contains(s[i + countCharNotSeparators]))
                    {
                        countCharNotSeparators++;
                        if (i + countCharNotSeparators >= s.Length)
                            break;
                    }
                    if (countCharNotSeparators != 0)
                        list.Add(s.Substring(i, countCharNotSeparators));
                    if (countCharNotSeparators != 0)
                        i += countCharNotSeparators - 1;
                }
                else
                {
                    var countCharSeparators = 0;
                    while (separators.Contains(s[i + countCharSeparators]))
                    {
                        countCharSeparators++;
                        if (i + countCharSeparators >= s.Length)
                            break;
                    }
                    if (countCharSeparators != 0)
                        list.Add(s.Substring(i, countCharSeparators));
                    if (countCharSeparators != 0)
                        i += countCharSeparators - 1;
                }
            }
            return list.ToArray(); ;
        }
    }
}
