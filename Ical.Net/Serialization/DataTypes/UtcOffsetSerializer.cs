//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class UtcOffsetSerializer : EncodableDataTypeSerializer
{
    public UtcOffsetSerializer() { }

    public UtcOffsetSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(UtcOffset);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not UtcOffset offset) return null;

        var value = (offset.Positive ? "+" : "-") + offset.Hours.ToString("00", CultureInfo.InvariantCulture) +
                    offset.Minutes.ToString("00", CultureInfo.InvariantCulture) +
                    (offset.Seconds != 0 ? offset.Seconds.ToString("00", CultureInfo.InvariantCulture) : string.Empty);

        // Encode the value as necessary
        return Encode(offset, value);
    }

    public override object? Deserialize(TextReader tr)
    {
        var offsetString = tr.ReadToEnd();
        try
        {
            var offset = new UtcOffset(offsetString);
            return offset;
        }
        catch
        {
            return null;
        }
    }

    private static readonly string[] _supportedFormats = ["hhmmss", "hhmm", "hh"];

    public static TimeSpan GetOffset(string rawOffset)
    {
        // Determine if the offset is negative
        var isNegative = rawOffset.StartsWith("-");
        rawOffset = rawOffset.TrimStart('+', '-');

        if (TimeSpan.TryParseExact(rawOffset, _supportedFormats, CultureInfo.InvariantCulture, out var ts))
        {
            return isNegative ? -ts : ts;
        }

        throw new FormatException($"{rawOffset} is not a valid UTC offset.");
    }
}
