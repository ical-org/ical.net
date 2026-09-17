//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class PeriodListConverter : CalendarPropertyConverter<PeriodList>
{
    public override PeriodList? Read(CalendarReader reader, IParameterCollection parameters)
    {
        var tzId = parameters.Get("TZID");

        var list = new PeriodList();

        while (true)
        {
            if (reader.TryGetPeriod(tzId, out var period))
            {
                list.Add(period);
            }
            else if (reader.TryGetDateTime(tzId, out var dt))
            {
                list.Add(dt);
            }
            else
            {
                break;
            }
        }

        return list;
    }

    public override void Write(CalendarWriter writer, PeriodList value)
    {
        // All values are expected to be the same time zone
        if (value.TzId is { } tzId && tzId != "UTC")
        {
            writer.WriteParameter("TZID", tzId);
        }

        switch (value.PeriodKind)
        {
            case PeriodKind.Period:
                writer.WriteParameter("VALUE", "PERIOD");
                break;
            case PeriodKind.DateOnly:
                writer.WriteParameter("VALUE", "DATE");
                break;
        }

        foreach (var period in value)
        {
            writer.WriteValue(period.ToBasicIso());
        }
    }

    public override void WriteParameters(
        CalendarWriter writer,
        IEnumerable<CalendarParameter> parameters,
        ICalendarParameterCollectionContainer container)
    {
        // Use parameters from PeriodList value instead
        var parametersExceptValue = parameters
            .Where(x => !x.Name.Equals("TZID", StringComparison.OrdinalIgnoreCase)
                && !x.Name.Equals("VALUE", StringComparison.OrdinalIgnoreCase));

        base.WriteParameters(writer, parametersExceptValue, container);
    }
}
