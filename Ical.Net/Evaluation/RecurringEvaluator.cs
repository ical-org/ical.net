//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
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
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    /// <param name="includeReferenceDateInResults"></param>
    protected IEnumerable<Period> EvaluateRRule(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd, bool includeReferenceDateInResults)
    {
        if (Recurrable.RecurrenceRules == null || !Recurrable.RecurrenceRules.Any())
            return [];

        var periodsQueries = Recurrable.RecurrenceRules.Select(rule =>
        {
            var ruleEvaluator = rule.GetService(typeof(IEvaluator)) as IEvaluator;
            if (ruleEvaluator == null)
            {
                return Enumerable.Empty<Period>();
            }
            return ruleEvaluator.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
        })
            // Enumerate the outer sequence (not the inner sequences of periods themselves) now to ensure
            // the initialization code is run, including validation and error handling.
            // This way we receive validation errors early, not only when enumeration starts.
            .ToList(); //NOSONAR - deliberately enumerate here

        return periodsQueries.OrderedMergeMany();
    }

    /// <summary> Evaluates the RDate component. </summary>
    protected IEnumerable<Period> EvaluateRDate(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd)
    {
        var recurrences =
            new SortedSet<Period>(Recurrable.RecurrenceDates
                .GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime));

        return recurrences;
    }

    /// <summary>
    /// Evaluates the ExRule component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    protected IEnumerable<Period> EvaluateExRule(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd)
    {
        if (Recurrable.ExceptionRules == null || !Recurrable.ExceptionRules.Any())
            return [];

        var exRuleEvaluatorQueries = Recurrable.ExceptionRules.Select(exRule =>
        {
            var exRuleEvaluator = exRule.GetService(typeof(IEvaluator)) as IEvaluator;
            if (exRuleEvaluator == null)
            {
                return Enumerable.Empty<Period>();
            }
            return exRuleEvaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
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
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    protected IEnumerable<Period> EvaluateExDate(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd)
    {
        var exDates = new SortedSet<Period>(Recurrable
            .ExceptionDates.GetAllPeriodsByKind(PeriodKind.DateOnly, PeriodKind.DateTime));
        return exDates;
    }

    public override IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd, bool includeReferenceDateInResults)
    {
        var rruleOccurrences = EvaluateRRule(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
        //Only add referenceDate if there are no RecurrenceRules defined
        if (includeReferenceDateInResults && (Recurrable.RecurrenceRules == null || !Recurrable.RecurrenceRules.Any()))
            rruleOccurrences = rruleOccurrences.Prepend(new Period(referenceDate));

        var rdateOccurrences = EvaluateRDate(referenceDate, periodStart, periodEnd);

        var exRuleExclusions = EvaluateExRule(referenceDate, periodStart, periodEnd);
        var exDateExclusions = EvaluateExDate(referenceDate, periodStart, periodEnd);

        var periods =
            rruleOccurrences
            .OrderedMerge(rdateOccurrences)
            .OrderedDistinct()
            .OrderedExclude(exRuleExclusions)
            .OrderedExclude(exDateExclusions, Comparer<Period>.Create(CompareExDateOverlap));

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
