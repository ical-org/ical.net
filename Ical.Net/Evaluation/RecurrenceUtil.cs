﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

internal class RecurrenceUtil
{
    public static IEnumerable<Occurrence> GetOccurrences(IRecurrable recurrable, CalDateTime dt, EvaluationOptions? options = null) => GetOccurrences(recurrable,
        new CalDateTime(dt.Date), new CalDateTime(dt.Date.AddDays(1)), options);

    public static IEnumerable<Occurrence> GetOccurrences(IRecurrable recurrable, CalDateTime? periodStart, CalDateTime? periodEnd, EvaluationOptions? options = null)
    {
        var evaluator = recurrable.Evaluator;
        if (evaluator == null || recurrable.Start == null)
        {
            return [];
        }

        // Ensure the start time is associated with the object being queried
        var start = recurrable.Start;

        // Change the time zone of periodStart/periodEnd as needed
        // so they can be used during the evaluation process.

        if (periodStart != null)
            periodStart = new CalDateTime(periodStart.Date, periodStart.Time, start.TzId);
        if (periodEnd != null)
            periodEnd = new CalDateTime(periodEnd.Date, periodEnd.Time, start.TzId);

        var periods = evaluator.Evaluate(start, periodStart, periodEnd, options);

        var occurrences =
            from p in periods
            let endTime = p.EndTime ?? p.StartTime
            where
                (((periodStart == null) || endTime.GreaterThan(periodStart)) && ((periodEnd == null) || p.StartTime.LessThan(periodEnd)) ||
                (periodStart != null && periodStart.Equals(periodEnd)
                 && p.StartTime.LessThanOrEqual(periodStart)
                 && endTime.GreaterThan(periodEnd))) || //A period that starts at the same time it ends
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
                return [false, null, false, false, false, false, false, true, false];
            case FrequencyType.Hourly:
                return [false, null, false, false, false, false, true, true, false];
            case FrequencyType.Daily:
                return [false, null, null, false, false, true, true, true, false];
            case FrequencyType.Weekly:
                return [false, null, null, null, true, true, true, true, false];
            case FrequencyType.Monthly:
                {
                    bool?[] row = [false, null, null, true, true, true, true, true, false];

                    // Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
                    if (p.ByMonthDay.Count > 0)
                    {
                        row[4] = false;
                    }

                    return row;
                }
            case FrequencyType.Yearly:
                {
                    bool?[] row = [true, true, true, true, true, true, true, true, false];

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
                return [false, null, false, false, false, false, false, false, false];
        }
    }
}
