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

internal class FreeBusyConverter : CalendarPropertyConverter<FreeBusyEntry>
{
    private const string Fbtype = "FBTYPE";

    public override FreeBusyEntry? Read(CalendarReader reader, IParameterCollection parameters)
    {
        // Date values should always be in UTC
        if (reader.TryGetPeriod(null, out var period))
        {
            var fbType = parameters.Get(Fbtype) switch
            {
                "FREE" => FreeBusyStatus.Free,
                "BUSY-UNAVAILABLE" => FreeBusyStatus.BusyUnavailable,
                "BUSY-TENTATIVE" => FreeBusyStatus.BusyTentative,
                _ => FreeBusyStatus.Busy
            };

            return new FreeBusyEntry(period, fbType);
        }

        throw new SerializationException($"Failed to parse FREEBUSY value at line {reader.LineNumber}");
    }

    public override void Write(CalendarWriter writer, FreeBusyEntry value)
    {
        switch (value.Status)
        {
            case FreeBusyStatus.BusyTentative:
                writer.WriteParameter(Fbtype, "BUSY-TENTATIVE");
                break;
            case FreeBusyStatus.BusyUnavailable:
                writer.WriteParameter(Fbtype, "BUSY-UNAVAILABLE");
                break;
            case FreeBusyStatus.Free:
                writer.WriteParameter(Fbtype, "FREE");
                break;
        }

        writer.WriteValue(value.ToBasicIso());
    }

    public override void WriteParameters(
        CalendarWriter writer,
        IEnumerable<CalendarParameter> parameters,
        ICalendarParameterCollectionContainer container)
    {
        // Use parameters from value instead
        var parametersExceptValue = parameters
            .Where(x => !x.Name.Equals(Fbtype, StringComparison.OrdinalIgnoreCase));

        base.WriteParameters(writer, parametersExceptValue, container);
    }
}
