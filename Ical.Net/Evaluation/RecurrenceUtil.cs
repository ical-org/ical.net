//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation;

internal class RecurrenceUtil
{
    public static IEnumerable<Occurrence> GetOccurrences(IRecurrable recurrable, CalDateTime dt, bool includeReferenceDateInResults) => GetOccurrences(recurrable,
        new CalDateTime(dt.Date), new CalDateTime(dt.Date.AddDays(1)), includeReferenceDateInResults);

    public static IEnumerable<Occurrence> GetOccurrences(IRecurrable recurrable, CalDateTime periodStart, CalDateTime periodEnd, bool includeReferenceDateInResults)
    {
        var evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
        if (evaluator == null || recurrable.Start == null)
        {
            return [];
        }

        // Ensure the start time is associated with the object being queried
        var start = recurrable.Start;
        start.AssociatedObject = recurrable as ICalendarObject;

        // Change the time zone of periodStart/periodEnd as needed
        // so they can be used during the evaluation process.

        if (periodStart != null)
            periodStart = new CalDateTime(periodStart.Date, periodStart.Time, start.TzId);
        if (periodEnd != null)
            periodEnd = new CalDateTime(periodEnd.Date, periodEnd.Time, start.TzId);

        var periods = evaluator.Evaluate(start, periodStart, periodEnd,
            includeReferenceDateInResults);

        var occurrences =
            from p in periods
            let endTime = p.EndTime ?? p.StartTime
            where
                (((periodStart == null) || endTime.GreaterThan(periodStart)) && ((periodEnd == null) || p.StartTime.LessThan(periodEnd)) ||
                (periodStart.Equals(periodEnd) && p.StartTime.LessThanOrEqual(periodStart) && endTime.GreaterThan(periodEnd))) || //A period that starts at the same time it ends
                (p.StartTime.Equals(endTime) && p.StartTime.Equals(periodStart)) //An event that starts at the same time it ends
            select new Occurrence(recurrable, p);

        return occurrences;
    }

    public static bool?[] GetExpandBehaviorList(RecurrencePattern p)
    {
        // See the table in RFC 5545 Section 3.3.10 (Page 43).
        switch (p.Frequency)
        {
            case FrequencyType.Minutely:
                return new bool?[] { false, null, false, false, false, false, false, true, false };
            case FrequencyType.Hourly:
                return new bool?[] { false, null, false, false, false, false, true, true, false };
            case FrequencyType.Daily:
                return new bool?[] { false, null, null, false, false, true, true, true, false };
            case FrequencyType.Weekly:
                return new bool?[] { false, null, null, null, true, true, true, true, false };
            case FrequencyType.Monthly:
                {
                    var row = new bool?[] { false, null, null, true, true, true, true, true, false };

                    // Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
                    if (p.ByMonthDay.Count > 0)
                    {
                        row[4] = false;
                    }

                    return row;
                }
            case FrequencyType.Yearly:
                {
                    var row = new bool?[] { true, true, true, true, true, true, true, true, false };

                    // Limit if BYYEARDAY or BYMONTHDAY is present; otherwise,
                    // special expand for WEEKLY if BYWEEKNO present; otherwise,
                    // special expand for MONTHLY if BYMONTH present; otherwise,
                    // special expand for YEARLY.
                    if (p.ByYearDay.Count > 0 || p.ByMonthDay.Count > 0)
                    {
                        row[4] = false;
                    }

                    return row;
                }
            default:
                return new bool?[] { false, null, false, false, false, false, false, false, false };
        }
    }
}
