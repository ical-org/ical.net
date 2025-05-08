//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class StringSerializer : EncodableDataTypeSerializer
{
    public StringSerializer() { }

    public StringSerializer(SerializationContext ctx) : base(ctx) { }

    internal static readonly Regex SingleBackslashMatch = new Regex(@"(?<!\\)\\(?!\\)", RegexOptions.Compiled, RegexDefaults.Timeout);

    protected virtual string? Unescape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        value = value.Replace(@"\n", "\n");
        value = value.Replace(@"\N", "\n");
        value = value.Replace(@"\;", ";");
        value = value.Replace(@"\,", ",");
        // NOTE: double quotes aren't escaped in RFC2445, but are in Mozilla Sunbird (0.5-)
        value = value.Replace("\\\"", "\"");

        // Replace all single-backslashes with double-backslashes.
        value = SingleBackslashMatch.Replace(value, "\\\\");

        // Unescape double backslashes
        value = value.Replace(@"\\", @"\");
        return value;
    }

    protected virtual string? Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        // NOTE: fixed a bug that caused text parsing to fail on
        // programmatically entered strings.
        // SEE unit test SERIALIZE25().
        value = value.Replace(SerializationConstants.LineBreak, @"\n");
        value = value.Replace("\r", @"\n");
        value = value.Replace("\n", @"\n");
        value = value.Replace(";", @"\;");
        value = value.Replace(",", @"\,");
        return value;
    }

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
