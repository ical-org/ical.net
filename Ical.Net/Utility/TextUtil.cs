//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.Serialization;

namespace Ical.Net.Utility;

internal static class TextUtil
{
    /// <summary>
    /// Folds lines to 75 octets. It appends a CrLf, and prepends the next line with a space.
    /// per RFC https://tools.ietf.org/html/rfc5545#section-3.1
    /// </summary>
    public static void FoldLines(this StringBuilder result, string inputLine)
    {
        if (Encoding.UTF8.GetByteCount(inputLine) <= 75)
        {
            result.Append(inputLine);
            result.Append(SerializationConstants.LineBreak);
            return;
        }

        // Use ArrayPool<char> to rent arrays for processing
        // Cannot use Span<char> and stackalloc because netstandard2.0 does not support it
        var arrayPool = ArrayPool<char>.Shared;
        var currentLineArray = arrayPool.Rent(76); // 75 characters + 1 space

        try
        {
            var currentLineIndex = 0;
            var byteCount = 0;
            var charCount = 0;

            while (charCount < inputLine.Length)
            {
                var currentCharByteCount = Encoding.UTF8.GetByteCount([inputLine[charCount]]);

                if (byteCount + currentCharByteCount > 75)
                {
                    result.Append(currentLineArray, 0, currentLineIndex);
                    result.Append(SerializationConstants.LineBreak);

                    currentLineIndex = 0;
                    byteCount = 1;
                    currentLineArray[currentLineIndex++] = ' ';
                }

                currentLineArray[currentLineIndex++] = inputLine[charCount];
                byteCount += currentCharByteCount;
                charCount++;
            }

            // Append the remaining characters to the result
            if (currentLineIndex > 0)
            {
                result.Append(currentLineArray, 0, currentLineIndex);
            }

            result.Append(SerializationConstants.LineBreak);
        }
        finally
        {
            // Return the rented array to the pool
            arrayPool.Return(currentLineArray);
        }
    }

    public static IEnumerable<string> Chunk(string str, int chunkSize = 73)
    {
        for (var index = 0; index < str.Length; index += chunkSize)
        {
            yield return str.Substring(index, Math.Min(chunkSize, str.Length - index));
        }
    }

    /// <summary> Removes blank lines from a string with normalized (\r\n) line endings </summary>
    public static string RemoveEmptyLines(string s)
    {
        var len = -1;
        while (len != s.Length)
        {
            s = s.Replace("\r\n\r\n", SerializationConstants.LineBreak);
            len = s.Length;
        }
        return s;
    }

    internal static readonly Regex NormalizeToCrLf = new Regex(@"((\r(?=[^\n]))|((?<=[^\r])\n))", RegexOptions.Compiled, RegexDefaults.Timeout);

    /// <summary>
    /// Normalizes line endings, converting "\r" into "\r\n" and "\n" into "\r\n".
    /// </summary>
    public static TextReader Normalize(string s, SerializationContext ctx)
    {
        // Replace \r and \n with \r\n.
        s = NormalizeToCrLf.Replace(s, SerializationConstants.LineBreak);

        s = RemoveEmptyLines(UnwrapLines(s));

        return new StringReader(s);
    }

    internal static readonly Regex NewLineMatch = new Regex(@"(\r\n[ \t])", RegexOptions.Compiled, RegexDefaults.Timeout);

    /// <summary> Unwraps lines from the RFC 5545 "line folding" technique. </summary>
    public static string UnwrapLines(string s) => NewLineMatch.Replace(s, string.Empty);

    public static bool Contains(this string haystack, string needle, StringComparison stringComparison)
        => haystack.IndexOf(needle, stringComparison) >= 0;
}
