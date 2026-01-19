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
using NodaTime;

namespace Ical.Net.Evaluation;

internal static class RecurrenceUtil
{
    public static IEnumerable<Occurrence> GetOccurrences(IRecurrable recurrable, ZonedDateTime periodStart, EvaluationOptions? options = null)
    {
        return GetOccurrences(recurrable, periodStart.Zone, periodStart.ToInstant(), options);
    }

    public static IEnumerable<Occurrence> GetOccurrences(IRecurrable recurrable, DateTimeZone timeZone, Instant? periodStart, EvaluationOptions? options = null)
    {
        var evaluator = recurrable.Evaluator;
        if (evaluator == null || recurrable.Start == null)
        {
            return [];
        }

        var start = recurrable.Start;

        var periods = evaluator.Evaluate(start, timeZone, periodStart, options);
        if (periodStart != null)
        {
            periods =
                from p in periods
                where
                    p.Start.ToInstant() >= periodStart
                    || (p.End != null && p.End.Value.ToInstant() > periodStart.Value)
                select p;
        }

        return periods.Select(p => new Occurrence(recurrable, p.Start, p.End ?? p.Start));
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
