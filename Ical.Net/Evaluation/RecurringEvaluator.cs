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

public class RecurringEvaluator : Evaluator
{
    protected IRecurrable Recurrable { get; set; }

    public RecurringEvaluator(IRecurrable obj)
    {
        Recurrable = obj;
    }

    /// <summary>
    /// Evaluates the RRule component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="options"></param>
    protected IEnumerable<Period> EvaluateRRule(CalDateTime referenceDate, CalDateTime? periodStart, EvaluationOptions? options)
    {
        if (!Recurrable.RecurrenceRules.Any())
            return [];

        var periodsQueries = Recurrable.RecurrenceRules.Select(rule =>
        {
            var ruleEvaluator = new RecurrencePatternEvaluator(rule);
            return ruleEvaluator.Evaluate(referenceDate, periodStart, options);
        })
            // Enumerate the outer sequence (not the inner sequences of periods themselves) now to ensure
            // the initialization code is run, including validation and error handling.
            // This way we receive validation errors early, not only when enumeration starts.
            .ToList(); //NOSONAR - deliberately enumerate here

        return periodsQueries.OrderedMergeMany();
    }

    /// <summary> Evaluates the RDate component. </summary>
    protected IEnumerable<Period> EvaluateRDate(CalDateTime referenceDate, CalDateTime? periodStart)
    {
        var recurrences = Recurrable.RecurrenceDates
                .GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime)
                .AsEnumerable();

        if (periodStart != null)
            recurrences = recurrences.Where(p => p.StartTime.GreaterThanOrEqual(periodStart));

        return new SortedSet<Period>(recurrences);
    }

    /// <summary>
    /// Evaluates the ExRule component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="options"></param>
    protected IEnumerable<Period> EvaluateExRule(CalDateTime referenceDate, CalDateTime? periodStart, EvaluationOptions? options)
    {
        if (!Recurrable.ExceptionRules.Any())
            return [];

        var exRuleEvaluatorQueries = Recurrable.ExceptionRules.Select(exRule =>
        {
            var exRuleEvaluator = new RecurrencePatternEvaluator(exRule);
            return exRuleEvaluator.Evaluate(referenceDate, periodStart, options);
        })
            // Enumerate the outer sequence (not the inner sequences of periods themselves) now to ensure
            // the initialization code is run, including validation and error handling.
            // This way we receive validation errors early, not only when enumeration starts.
            .ToList(); //NOSONAR - deliberately enumerate here

        return exRuleEvaluatorQueries.OrderedMergeMany();
    }

    /// <summary>
    /// Evaluates the ExDate component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    protected IEnumerable<Period> EvaluateExDate(CalDateTime referenceDate, CalDateTime? periodStart)
    {
        var exDates = Recurrable.ExceptionDates.GetAllPeriodsByKind(PeriodKind.DateOnly, PeriodKind.DateTime)
            .AsEnumerable();

        if (periodStart != null)
            exDates = exDates.Where(p => p.StartTime.GreaterThanOrEqual(periodStart));

        return new SortedSet<Period>(exDates);
    }

    public override IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, EvaluationOptions? options)
    {
        IEnumerable<Period> rruleOccurrences;

        // Only add referenceDate if there are no RecurrenceRules defined. This is in line
        // with RFC 5545 which requires DTSTART to match any RRULE. If it doesn't, the behaviour
        // is undefined. It seems to be good practice not to return the referenceDate in this case.
        rruleOccurrences = !Recurrable.RecurrenceRules.Any()
            ? [new Period(referenceDate)]
            : EvaluateRRule(referenceDate, periodStart, options);

        var rdateOccurrences = EvaluateRDate(referenceDate, periodStart);

        var exRuleExclusions = EvaluateExRule(referenceDate, periodStart, options);

        // EXDATEs could contain date-only entries while DTSTART is date-time. Probably this isn't supported
        // by the RFC, but it seems to be used in the wild (see https://github.com/ical-org/ical.net/issues/829).
        // So we must make sure to return all-day EXDATEs that could overlap with recurrences, even if the day starts
        // before `periodStart`. We therefore start 2 days earlier (2 for safety regarding the TZ).
        // DISCUSS: Should we support date-only EXDATEs being mixed with date-time DTSTARTs at all? What
        // if one has a time zone and the other doesn't? Many details are unclear.
        var exDateExclusions = EvaluateExDate(referenceDate, periodStart?.AddDays(-2));

        var periods =
            rruleOccurrences
            .OrderedMerge(rdateOccurrences)
            .OrderedDistinct()
            .OrderedExclude(exRuleExclusions)
            .OrderedExclude(exDateExclusions, Comparer<Period>.Create(CompareExDateOverlap))

            // Convert overflow exceptions to expected ones.
            .HandleEvaluationExceptions();

        return periods;
    }

    /// <summary>
    /// Compares whether the given period's date overlaps with the given EXDATE. The dates are
    /// considered to overlap if they start at the same time, or the EXDATE is an all-day date
    /// and the period's start date is the same as the EXDATE's date.
    /// <para/>
    /// Note: <see cref="Period.EffectiveDuration"/> for <paramref name="exDate"/> is always <see langword="null"/>.
    /// </summary>
    private static int CompareExDateOverlap(Period period, Period exDate)
    {
        var cmp = period.CompareTo(exDate);
        if ((cmp != 0) && !exDate.StartTime.HasTime && (period.StartTime.Value.Date == exDate.StartTime.Value))
            cmp = 0;

        return cmp;
    }
}
