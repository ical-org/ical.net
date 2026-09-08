//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Runtime.Serialization;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class UtcOffsetConverter : CalendarPropertyConverter<UtcOffset>
{
    public override UtcOffset? Read(CalendarReader reader, IParameterCollection parameters)
    {
        if (!reader.TryGetUtcOffset(out var utcOffset))
        {
            throw new SerializationException($"Failed to parse UTC offset at line {reader.LineNumber}");
        }

        return utcOffset;
    }

    public override void Write(CalendarWriter writer, UtcOffset value)
    {
        writer.WriteValueRaw(value.ToString());
    }
}
