//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;

namespace Ical.Net.Serialization;

public class ParameterSerializer : SerializerBase
{
    public ParameterSerializer() { }

    public ParameterSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(CalendarParameter);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not CalendarParameter p)
        {
            return null;
        }

        // "Section 3.2:  Property parameter values MUST NOT contain the DQUOTE character."
        // Therefore, let's strip any double quotes from the value.
        var values = string.Join(",", p.Values).Replace("\"", string.Empty);

        // Surround the parameter value with double quotes, if the value
        // contains any problematic characters.
        if (values.IndexOfAny(QuotedValueChars) >= 0)
        {
            return p.Name + "=\"" + values + "\"";
        }
        return p.Name + "=" + values;
    }

    private static readonly char[] QuotedValueChars = [';', ':', ','];

    public override object? Deserialize(TextReader tr) => null;
}
