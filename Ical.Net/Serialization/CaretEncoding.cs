//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Text;

namespace Ical.Net.Serialization;

/// <summary>
/// RFC 6868 caret encoding for property parameter values.
/// </summary>
internal static class CaretEncoding
{
    private static readonly char[] Special = { '^', '"', '\r', '\n' };

    /// <summary>
    /// Encodes a parameter value per RFC 6868 §3.1 (single left-to-right pass).
    /// </summary>
    public static string Encode(string value)
    {
        if (value.IndexOfAny(Special) < 0)
            return value;

        var sb = new StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            switch (c)
            {
                case '^': sb.Append("^^"); break;
                case '"': sb.Append("^'"); break;
                case '\r':
                    sb.Append("^n");
                    // Collapse a CRLF pair into a single ^n.
                    if (i + 1 < value.Length && value[i + 1] == '\n') i++;
                    break;
                case '\n': sb.Append("^n"); break;
                default: sb.Append(c); break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Decodes a parameter value per RFC 6868 §3.2 (single left-to-right pass, inverse of <see cref="Encode"/>).
    /// </summary>
    public static string Decode(string value)
    {
        if (value.IndexOf('^') < 0)
            return value;

        var sb = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '^' && i + 1 < value.Length)
            {
                switch (value[i + 1])
                {
                    case 'n': sb.Append('\n'); i++; continue;
                    case '\'': sb.Append('"'); i++; continue;
                    case '^': sb.Append('^'); i++; continue;
                }
                // A caret before any other char is left unchanged (§3.2).
            }
            sb.Append(c);
        }

        return sb.ToString();
    }
}
