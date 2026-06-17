//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Utility;
using NodaTime;


namespace Ical.Net.Tests;
internal static class TestExtensions
{
    public static IEnumerable<Occurrence> GetOccurrences(this CalendarEvent calendarEvent, CalDateTime dateTime, EvaluationOptions? options = null)
    {
        return calendarEvent.GetOccurrences(dateTime.ToZonedDateTime("America/New_York"), options);
    }

    public static IEnumerable<Occurrence> TakeWhileBefore(this IEnumerable<Occurrence> sequence, CalDateTime periodEnd)
        => sequence.TakeWhile(p => p.Start.ToInstant() < periodEnd.ToZonedDateTime("America/New_York").ToInstant());

    public static IEnumerable<ZonedDateTime> TakeWhileBefore(this IEnumerable<ZonedDateTime> sequence, CalDateTime periodEnd)
        => sequence.TakeWhile(p => p.ToInstant() < periodEnd.ToZonedDateTime("America/New_York").ToInstant());

    public static IEnumerable<ZonedDateTime> Evaluate(this RecurrenceRule pattern, CalDateTime referenceDate, CalDateTime periodStart, EvaluationOptions? options = null)
    {
        var zonedStart = periodStart.ToZonedDateTime("America/New_York");
        return pattern.Evaluate(referenceDate, zonedStart, options);
    }

    public static IEnumerable<ZonedDateTime> Evaluate(this RecurrenceRule pattern, CalDateTime referenceDate, ZonedDateTime periodStart, EvaluationOptions? options = null)
    {
        return new RecurrenceRuleEvaluator(pattern, referenceDate, periodStart, CalendarTimeZoneProviders.TzdbWithAliases, options).Evaluate();
    }

    public static ZonedDateTime ToZonedDateTime(this CalDateTime value, string zoneId)
    {
        var tz = CalendarTimeZoneProviders.TzdbWithAliases[zoneId];
        return value.ToZonedOrDefault(tz, CalendarTimeZoneProviders.TzdbWithAliases).WithZone(tz);
    }

    public static ZonedDateTime ToZonedDateTime(this CalDateTime value)
    {
        return value.ToZonedDateTime(CalendarTimeZoneProviders.TzdbWithAliases);
    }

    public static ZonedDateTime ToZonedOrDefault(this CalDateTime value, DateTimeZone timeZone)
    {
        return value.ToZonedOrDefault(timeZone, CalendarTimeZoneProviders.TzdbWithAliases);
    }
}
