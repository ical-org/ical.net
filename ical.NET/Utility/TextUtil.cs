using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Utility
{
    public static class TextUtil
    {
        public static string WrapLines(string value)
        {
            // NOTE: Made this method more efficient by removing
            // the use of strings, and only using StringBuilders.
            // Also, the "while" loop was removed, and StringBuilder
            // modifications are kept at a minimum.
            var result = new StringBuilder();
            var current = new StringBuilder(value);

            // Wrap lines at 75 characters, per RFC 2445 "folding" technique
            var i = 0;
            if (current.Length > 75)
            {
                result.Append(current.ToString(0, 75) + "\r\n ");
                for (i = 75; i < current.Length - 74; i += 74)
                {
                    result.Append(current.ToString(i, 74) + "\r\n ");
                }
            }
            result.Append(current.ToString(i, current.Length - i));
            result.Append("\r\n");

            return result.ToString();
        }

        /// <summary>
        /// Removes blank lines from a string with normalized (\r\n)
        /// line endings.
        /// NOTE: this method makes the line/col numbers output from
        /// antlr incorrect.
        /// </summary>
        public static string RemoveEmptyLines(string s)
        {
            var len = -1;
            while (len != s.Length)
            {
                s = s.Replace("\r\n\r\n", "\r\n");
                len = s.Length;
            }
            return s;
        }

        internal static readonly Regex NormalizeToCrLf = new Regex(@"((\r(?=[^\n]))|((?<=[^\r])\n))", RegexOptions.Compiled);

        /// <summary>
        /// Normalizes line endings, converting "\r" into "\r\n" and "\n" into "\r\n".        
        /// </summary>
        public static TextReader Normalize(string s, ISerializationContext ctx)
        {
            // Replace \r and \n with \r\n.
            s = NormalizeToCrLf.Replace(s, Environment.NewLine);

            var settings = ctx.GetService(typeof (ISerializationSettings)) as ISerializationSettings;
            if (settings == null || !settings.EnsureAccurateLineNumbers)
            {
                s = RemoveEmptyLines(UnwrapLines(s));
            }

            return new StringReader(s);
        }

        internal static readonly Regex NewLineMatch = new Regex(@"(\r\n[ \t])", RegexOptions.Compiled);

        /// <summary>
        /// Unwraps lines from the RFC 2445 "line folding" technique.
        /// NOTE: this method makes the line/col numbers output from
        /// antlr incorrect.
        /// </summary>
        public static string UnwrapLines(string s)
        {
            return NewLineMatch.Replace(s, string.Empty);
        }
    }
}