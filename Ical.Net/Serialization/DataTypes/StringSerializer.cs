//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class StringSerializer : EncodableDataTypeSerializer
{
    public StringSerializer() { }

    public StringSerializer(SerializationContext ctx) : base(ctx) { }

    protected virtual string? Unescape(string? value)
    {
        // Skip values that do not need to be unescaped
        if (value == null)
        {
            return value;
        }

#if NET8_0_OR_GREATER
        return UnescapeWithSpan(value);
#else

        var pos = value.IndexOf('\\');
        if (pos == -1)
        {
            // Nothing to unescape
            return value;
        }

        var sb = new StringBuilder(value.Length);

        // Copy up to first backslash
        sb.Append(value, 0, pos);

        while (pos < value.Length)
        {
            if (pos + 1 == value.Length)
            {
                // Last character is a backslash that escapes nothing.
                // Include backslash in result.
                sb.Append('\\');
                break;
            }

            var next = value[pos + 1];
            var consumed = ConsumeEscaped(next, sb);

            pos += consumed;

            // Check for next escaped value
            var nextPos = value.IndexOf('\\', pos);
            if (nextPos == -1)
            {
                sb.Append(value, pos, value.Length - pos);
                break;
            }

            sb.Append(value, pos, nextPos - pos);
            pos = nextPos;
        }

        return sb.ToString();
#endif
    }

#if NET8_0_OR_GREATER
    private static string UnescapeWithSpan(string valueToUnescape)
    {
        var value = valueToUnescape.AsSpan();

        var pos = value.IndexOf('\\');
        if (pos == -1)
        {
            // Nothing to unescape
            return valueToUnescape;
        }

        var sb = new StringBuilder(value.Length);
        sb.Append(value[..pos]);
        value = value[pos..];

        while (!value.IsEmpty)
        {
            if (value.Length == 1)
            {
                // Last character is a backslash that escapes nothing.
                // Include backslash in result.
                sb.Append('\\');
                break;
            }

            var next = value[1];
            var consumed = ConsumeEscaped(next, sb);

            value = value[consumed..];

            // Check for next escaped value
            pos = value.IndexOf('\\');
            if (pos == -1)
            {
                sb.Append(value);
                break;
            }

            sb.Append(value[..pos]);
            value = value[pos..];
        }

        return sb.ToString();
    }
#endif

    /// <summary>
    /// Adds the escaped character to the StringBuilder.
    /// </summary>
    /// <param name="escaped"></param>
    /// <param name="sb"></param>
    /// <returns>The number of characters consumed, including the backslash.</returns>
    private static int ConsumeEscaped(char escaped, StringBuilder sb)
    {
        if (escaped is 'n' or 'N')
        {
            sb.Append('\n');
            return 2;
        }

        // Double quotes aren't escaped in RFC2445, but are in Mozilla Sunbird (0.5-)
        if (escaped is '\\' or ';' or ',' or '"')
        {
            sb.Append(escaped);
            return 2;
        }

        // Backslash is escaping an invalid character, just
        // include the backslash and leave it unchanged.
        sb.Append('\\');
        return 1;
    }

    /// <summary>
    /// Characters that must be escaped for TEXT values. Note that \r is also
    /// included even though it is not specified in the RFC - it is treated as
    /// a \n value.
    /// </summary>
#if NET8_0_OR_GREATER
    private static readonly SearchValues<char> _textEscapeChars = SearchValues.Create("\\;,\n\r");
#else
    private static readonly char[] _textEscapeChars = ['\\', ';', ',', '\n', '\r'];
#endif

    protected virtual string? Escape(string? value)
    {
        if (value == null)
        {
            return value;
        }

#if NET8_0_OR_GREATER
        return EscapeWithSearchValues(value);
#else
        // Skip escaping if there are no characters to escape. This improves
        // the performance of the more common case of short TEXT values with
        // no characters to escape.
        var pos = value.IndexOfAny(_textEscapeChars);
        if (pos == -1)
        {
            // Nothing to escape
            return value;
        }

        var sb = new StringBuilder(value.Length);

        // Copy up to first character that needs to be escaped
        sb.Append(value, 0, pos);

        while (pos < value.Length)
        {
            // Escape the character
            sb.Append('\\');

            var toEscape = value[pos];
            var consumed = 1;

            if (toEscape == '\r')
            {
                // Treat \r as \n
                sb.Append('n');

                // Treat \r\n as \n
                if (pos + 1 < value.Length && value[pos + 1] == '\n')
                {
                    consumed = 2;
                }
            }
            else if (toEscape == '\n')
            {
                sb.Append('n');
            }
            else
            {
                sb.Append(toEscape);
            }

            pos += consumed;

            // Check for next value to escape
            var nextPos = value.IndexOfAny(_textEscapeChars, pos);
            if (nextPos == -1)
            {
                sb.Append(value, pos, value.Length - pos);
                break;
            }

            sb.Append(value, pos, nextPos - pos);
            pos = nextPos;
        }

        return sb.ToString();
#endif
    }

#if NET8_0_OR_GREATER
    private static string EscapeWithSearchValues(string valueToEscape)
    {
        var value = valueToEscape.AsSpan();

        var pos = value.IndexOfAny(_textEscapeChars);
        if (pos == -1)
        {
            // Nothing to escape
            return valueToEscape;
        }

        var sb = new StringBuilder(value.Length);
        sb.Append(value[..pos]);
        value = value[pos..];

        while (!value.IsEmpty)
        {
            // Escape the character
            sb.Append('\\');

            var toEscape = value[0];
            var consumed = 1;

            if (toEscape == '\r')
            {
                // Treat \r as \n
                sb.Append('n');

                // Treat \r\n as \n
                if (value.Length > 1 && value[1] == '\n')
                {
                    consumed = 2;
                }
            }
            else if (toEscape == '\n')
            {
                sb.Append('n');
            }
            else
            {
                sb.Append(toEscape);
            }

            value = value[consumed..];

            pos = value.IndexOfAny(_textEscapeChars);
            if (pos == -1)
            {
                sb.Append(value);
                break;
            }

            sb.Append(value[..pos]);
            value = value[pos..];
        }

        return sb.ToString();
    }
#endif

    public override Type TargetType => typeof(string);

    public override string? SerializeToString(object? obj)
    {
        if (obj == null)
        {
            return null;
        }

        var values = new List<string>();
        if (obj is string s)
        {
            values.Add(s);
        }
        else if (obj is IEnumerable enumerable)
        {
            values.AddRange(from object child in enumerable select child.ToString());
        }

        if (SerializationContext.Peek() is ICalendarObject co)
        {
            // Encode the string as needed.
            var dt = new EncodableDataType
            {
                AssociatedObject = co
            };
            for (var i = 0; i < values.Count; i++)
            {
                var value = Encode(dt, Escape(values[i]));
                if (value != null)
                {
                    values[i] = value;
                }
            }

            return string.Join(",", values);
        }

        for (var i = 0; i < values.Count; i++)
        {
            var escaped = Escape(values[i]);
            if (escaped != null)
            {
                values[i] = escaped;
            }
        }
        return string.Join(",", values);
    }

    internal static readonly Regex UnescapedCommas = new Regex(@"(?<!\\),", RegexOptions.Compiled, RegexDefaults.Timeout);
    public override object? Deserialize(TextReader? tr)
    {
        if (tr == null)
        {
            return null;
        }

        var value = tr.ReadToEnd();

        var serializeAsList = false;

        // Ensure SerializationContext is not null before accessing Peek()
        var context = SerializationContext;
        if (context.Peek() is ICalendarProperty cp)
        {
            var dataTypeMapper = GetService<DataTypeMapper>();
            serializeAsList = dataTypeMapper.GetPropertyAllowsMultipleValues(cp);
        }

        var dt = new EncodableDataType
        {
            AssociatedObject = context.Peek() as ICalendarObject
        };

        var encodedValues = serializeAsList ? UnescapedCommas.Split(value) : new[] { value };
        var escapedValues = encodedValues.Select(v => Decode(dt, v)).ToList();
        var values = escapedValues.Select(Unescape).ToList();

        if (values.Count == 1)
        {
            return values[0];
        }
        return values;
    }
}
