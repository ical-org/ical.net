//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class DurationConverter : CalendarPropertyConverter<Duration>
{
    public override Duration Read(CalendarReader reader, IParameterCollection parameters)
    {
        if (reader.TryGetDuration(out var duration))
        {
            return duration;
        }

        throw new FormatException("String value is not in the ISO 8601 basic format for DURATION");
    }

    public override void Write(CalendarWriter writer, Duration value)
    {
        writer.WriteValue(value.ToBasicIso());
    }
}
