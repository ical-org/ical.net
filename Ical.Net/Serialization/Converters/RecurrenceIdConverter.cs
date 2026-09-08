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

internal class RecurrenceIdConverter : CalendarPropertyConverter<RecurrenceIdentifier>
{
    public override RecurrenceIdentifier? Read(CalendarReader reader, IParameterCollection parameters)
    {
        var rangeString = parameters.Get("RANGE")?.ToUpperInvariant();

        var range = rangeString == "THISANDFUTURE"
            ? RecurrenceRange.ThisAndFuture : RecurrenceRange.ThisInstance;

        var tzId = parameters.Get("TZID");

        if (!reader.TryGetDateTime(tzId, out var dateTime))
        {
            throw new SerializationException($"Failed to parse RECURRENCE-ID value at line {reader.LineNumber}");
        }

        return new RecurrenceIdentifier(dateTime, range);
    }

    public override void Write(CalendarWriter writer, RecurrenceIdentifier recurrenceId)
    {
        var value = recurrenceId.StartTime;

        if (!value.HasTime)
        {
            writer.WriteParameter("VALUE", "DATE");
        }

        if (recurrenceId.Range == RecurrenceRange.ThisAndFuture)
        {
            writer.WriteParameter("RANGE", "THISANDFUTURE");
        }

        // Write all UTC values as instants ending with Z
        if (value.TzId != null && !value.IsUtc)
        {
            writer.WriteParameter("TZID", value.TzId);
        }

        writer.WriteValue(value.ToBasicIso());
    }

    public override void WriteParameters(CalendarWriter writer, IEnumerable<CalendarParameter> parameters, ICalendarParameterCollectionContainer container)
    {
        // Set VALUE based on value
        parameters = parameters
            .Where(x => !x.Name.Equals("VALUE", StringComparison.OrdinalIgnoreCase));

        base.WriteParameters(writer, parameters, container);
    }
}
