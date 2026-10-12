//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using System.Text;

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

        var builder = new StringBuilder();
        builder.Append(p.Name + "=");

        // RFC 6868 caret-encode each value, so ^, DQUOTE and newlines survive round-trips.
        var values = string.Join(",", System.Linq.Enumerable.Select(p.Values, v => CaretEncoding.Encode(v ?? string.Empty)));

        // Surround the parameter value with double quotes, if the value
        // contains any problematic characters.
        if (values.IndexOfAny(new[] { ';', ':', ',' }) >= 0)
        {
            values = "\"" + values + "\"";
        }
        builder.Append(values);
        return builder.ToString();
    }

    public override object? Deserialize(TextReader tr) => null;
}
