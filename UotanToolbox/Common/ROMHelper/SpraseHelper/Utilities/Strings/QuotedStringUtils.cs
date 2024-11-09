using System.Collections.Generic;

namespace UotanToolbox.Common.ROMHelper.SpraseHelper.Utilities.Strings
{
    internal class QuotedStringUtils
    {
        public static string Quote(string str)
        {
            return string.Format("\"{0}\"", str);
        }

        public static string Unquote(string str)
        {
            string quote = '"'.ToString();
            if (str.Length >= 2 && str.StartsWith(quote) && str.EndsWith(quote))
            {
                return str.Substring(1, str.Length - 2);
            }
            else
            {
                return str;
            }
        }

        public static int IndexOfUnquotedChar(string str, char charToFind)
        {
            return IndexOfUnquotedChar(str, charToFind, 0);
        }

        public static int IndexOfUnquotedChar(string str, char charToFind, int startIndex)
        {
            if (startIndex >= str.Length)
            {
                return -1;
            }

            bool inQuote = false;
            int index = startIndex;
            while (index < str.Length)
            {
                if (str[index] == '"')
                {
                    inQuote = !inQuote;
                }
                else if (!inQuote && str[index] == charToFind)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static List<string> SplitIgnoreQuotedSeparators(string str, char separator)
        {
            List<string> result = new List<string>();
            int nextEntryIndex = 0;
            int separatorIndex = IndexOfUnquotedChar(str, separator);
            while (separatorIndex >= nextEntryIndex)
            {
                result.Add(str.Substring(nextEntryIndex, separatorIndex - nextEntryIndex));

                nextEntryIndex = separatorIndex + 1;
                separatorIndex = IndexOfUnquotedChar(str, separator, nextEntryIndex);
            }
            result.Add(str.Substring(nextEntryIndex));
            return result;
        }
    }
}
