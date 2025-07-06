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

public abstract class RecurringEvaluator : Evaluator
{
    protected IRecurrable Recurrable { get; set; }

    protected RecurringEvaluator(IRecurrable obj)
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
    protected IEnumerable<Period> EvaluateRDate(CalDateTime? periodStart)
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
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="periodKinds">The period kinds to be returned. Used as a filter.</param>
    private IEnumerable<Period> EvaluateExDate(CalDateTime? periodStart, params PeriodKind[] periodKinds)
    {
        var exDates = Recurrable.ExceptionDates.GetAllPeriodsByKind(periodKinds)
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

        var rdateOccurrences = EvaluateRDate(periodStart);

        var exRuleExclusions = EvaluateExRule(referenceDate, periodStart, options);

        // EXDATEs could contain date-only entries while DTSTART is date-time. Probably this isn't supported
        // by the RFC, but it seems to be used in the wild (see https://github.com/ical-org/ical.net/issues/829).
        // So we must make sure to return all-day EXDATEs that could overlap with recurrences, even if the day starts
        // before `periodStart`. We therefore start 2 days earlier (2 for safety regarding the TZ).
        var exDateExclusionsDateOnly = new HashSet<DateOnly>(EvaluateExDate(periodStart?.AddDays(-2), PeriodKind.DateOnly)
            .Select(x => x.StartTime.Date));

        var exDateExclusionsDateTime = EvaluateExDate(periodStart, PeriodKind.DateTime);

        var periods =
            rruleOccurrences
            .OrderedMerge(rdateOccurrences)
            .OrderedDistinct()
            .OrderedExclude(exRuleExclusions)
            .OrderedExclude(exDateExclusionsDateTime)

            // We accept date-only EXDATEs to be used with date-time DTSTARTs. In such cases we exclude those occurrences
            // that, in their respective time zone, have a date component that matches an EXDATE.
            // See https://github.com/ical-org/ical.net/pull/830 for more information.
            //
            // The order of dates in the EXDATEs doesn't necessarily match the order of dates returned by RDATEs
            // due to RDATEs could have different time zones. We therefore use a regular `.Where()` to look up
            // the EXDATEs in the HashSet rather than using `.OrderedExclude()`, which would require correct ordering.
            .Where(dt => !exDateExclusionsDateOnly.Contains(dt.StartTime.Date))

            // Convert overflow exceptions to expected ones.
            .HandleEvaluationExceptions();

        return periods;
    }
}
