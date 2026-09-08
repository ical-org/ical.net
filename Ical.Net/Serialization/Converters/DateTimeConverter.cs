//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class DateTimeConverter : CalendarPropertyConverter<CalDateTime>
{
    public override CalDateTime? Read(CalendarReader reader, IParameterCollection parameters)
    {
        var tzId = parameters.Get("TZID");
        if (reader.TryGetDateTime(tzId, out var dt))
        {
            return dt;
        }

        throw new SerializationException($"Failed to parse DATE or DATE-TIME value at line {reader.LineNumber}");
    }

    public override void Write(CalendarWriter writer, CalDateTime value)
    {
        if (!value.HasTime)
        {
            writer.WriteParameter("VALUE", "DATE");
        }

        // Write all UTC values as instants ending with Z
        if (value.TzId != null && !value.IsUtc)
        {
            writer.WriteParameter("TZID", value.TzId);
        }

        writer.WriteValue(value.ToBasicIso());
    }

    public override void WriteParameters(
        CalendarWriter writer,
        IEnumerable<CalendarParameter> parameters,
        ICalendarParameterCollectionContainer container)
    {
        // Use TZID from CalDateTime instead
        var parametersExceptTzid = parameters.Where(x =>
            !x.Name.Equals("TZID", StringComparison.OrdinalIgnoreCase)
            && !x.Name.Equals("VALUE", StringComparison.OrdinalIgnoreCase));

        base.WriteParameters(writer, parametersExceptTzid, container);
    }
}
