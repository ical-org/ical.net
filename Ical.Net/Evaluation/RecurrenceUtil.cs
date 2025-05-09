﻿//
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

internal static class RecurrenceUtil
{
    public static IEnumerable<Occurrence> GetOccurrences(IRecurrable recurrable, CalDateTime? periodStart, EvaluationOptions? options = null)
    {
        var evaluator = recurrable.Evaluator;
        if (evaluator == null || recurrable.Start == null)
        {
            return [];
        }

        // Ensure the start time is associated with the object being queried
        var start = recurrable.Start;

        // Change the time zone of periodStart as needed
        // so they can be used during the evaluation process.

        if (periodStart != null)
            periodStart = new CalDateTime(periodStart.Date, periodStart.Time, start.TzId);

        var periods = evaluator.Evaluate(start, periodStart, options);
        if (periodStart != null)
        {
            periods =
                from p in periods
                let endTime = p.EndTime ?? p.StartTime
                where
                    p.StartTime.GreaterThanOrEqual(periodStart)
                    || endTime.GreaterThan(periodStart)
                select p;
        }

        return periods.Select(p => new Occurrence(recurrable, p));
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

    public static IEnumerable<T> HandleEvaluationExceptions<T>(this IEnumerable<T> sequence)
        =>
            sequence

            // ArgumentOutOfRangeException is raised by operations on System.DateOnly et al when exceeding
            // the maximum supported date/time value, which is 9999-12-31. When evaluation recurrence rules,
            // these exceptions could basically be raised anywhere, so we handle them here centrally and
            // convert them to EvaluationOutOfRangeException, which are specified to be raised in such cases.
            // There shouldn't be other causes for this type of exceptions, as most validations of the pattern
            // itself are already done earlier, before doing the actual enumeration.
            // Intentionally don't include the outer exception as this most likely is not a technical but a usage error.
            .Catch<T, ArgumentOutOfRangeException>(_ => throw new EvaluationOutOfRangeException("An out-of-range value was encountered while evaluating occurrences. This commonly happens when trying to enumerate an unbounded RRULE to its end. Consider applying the .TakeWhile() operator."))

            // System.OverflowException is raised by NodaTime when exceeding the maximum supported date/time
            // value of one tick before 10000-01-01.
            .Catch<T, OverflowException>(_ => throw new EvaluationOutOfRangeException("An overflow was encountered while evaluating the calendar occurrences. This commonly happens when trying to enumerate an unbounded RRULE to its end. Consider applying the .TakeWhile() operator."));
}
