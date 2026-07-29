//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

public class TimeZoneInfoEvaluator : RecurringEvaluator
{
    protected VTimeZoneInfo TimeZoneInfo
    {
        get => Recurrable as VTimeZoneInfo ?? throw new InvalidOperationException();
        set => Recurrable = value;
    }

    protected override NodaTime.ZonedDateTime GetEnd(NodaTime.ZonedDateTime start) => start;

    protected override NodaTime.IDateTimeZoneProvider TimeZoneProvider => CalendarTimeZoneProviders.TzdbWithAliases;

    protected override EvaluationPeriod EvaluateRDate(Period rdate, NodaTime.DateTimeZone referenceTimeZone)
    {
        var offsetFrom = TimeZoneInfo.OffsetFrom
            ?? throw new InvalidOperationException($"Time zone info \"{TimeZoneInfo.TimeZoneName}\" must have a TZOFFSETFROM value");

        // RFC 5545:
        //  "RDATE" in this usage MUST be specified as a date with local time value,
        //  relative to the UTC offset specified in the "TZOFFSETFROM" property.
        //
        // Convert RDATE to an instant using the TZOFFSETFROM value.
        // Do NOT use the reference time zone!
        var start = rdate.StartTime.ToLocalDateTime()
            .WithOffset(NodaTime.Offset.FromTimeSpan(offsetFrom.Offset))
            .InFixedZone();

        // Time zone transitions do not have a duration, so start is the end.
        return new EvaluationPeriod(start, start);
    }

    public TimeZoneInfoEvaluator(IRecurrable tzi) : base(tzi) { }
}
