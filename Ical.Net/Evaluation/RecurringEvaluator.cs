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
using NodaTime.TimeZones;

namespace Ical.Net.Evaluation;

public abstract class RecurringEvaluator : Evaluator
{
    protected IRecurrable Recurrable { get; set; }

    protected abstract DataTypes.Duration? DefaultDuration { get; }

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
    protected IEnumerable<EvaluationPeriod> EvaluateRRule(CalDateTime referenceDate, DateTimeZone timeZone, Instant? periodStart, EvaluationOptions? options)
    {
        if (!Recurrable.RecurrenceRules.Any())
            return [];

        var d = this.DefaultDuration;
        Instant? effPeriodStart;

        if (d != null && periodStart != null)
        {
            effPeriodStart = periodStart.Value
                .InZone(timeZone)
                .LocalDateTime
                .Minus(d.Value.ToPeriod())
                .InZoneLeniently(timeZone)
                .ToInstant();
        }
        else
        {
            effPeriodStart = periodStart;
        }

        var periodsQueries = Recurrable.RecurrenceRules.Select(rule =>
        {
            var ruleEvaluator = new RecurrencePatternEvaluator(rule);
            return ruleEvaluator.Evaluate(referenceDate, timeZone, effPeriodStart, options);
        })
            // Enumerate the outer sequence (not the inner sequences of periods themselves) now to ensure
            // the initialization code is run, including validation and error handling.
            // This way we receive validation errors early, not only when enumeration starts.
            .ToList(); //NOSONAR - deliberately enumerate here

        return periodsQueries.OrderedMergeMany();
    }

    protected abstract EvaluationPeriod EvaluateRDate(DataTypes.Period rdate, DateTimeZone referenceTimeZone);

    /// <summary> Evaluates the RDate component. </summary>
    protected SortedSet<EvaluationPeriod> EvaluateRDate(DateTimeZone referenceTimeZone)
    {
        var result = Recurrable.RecurrenceDates
            .GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime)
            .Select(x => EvaluateRDate(x, referenceTimeZone));

        return new SortedSet<EvaluationPeriod>(result);
    }

    /// <summary>
    /// Evaluates the ExRule component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="options"></param>
    [Obsolete("EXRULE is marked as deprecated in RFC 5545 and will be removed in a future version")]
    private IEnumerable<EvaluationPeriod> EvaluateExRule(CalDateTime referenceDate, DateTimeZone timeZone, EvaluationOptions? options)
    {
        if (!Recurrable.ExceptionRules.Any())
            return [];

        // We don't apply periodStart here. Calculating it would be complex because
        // RDATE's may have arbitrary durations.
        Instant? effPeriodStart = null;

        var exRuleEvaluatorQueries = Recurrable.ExceptionRules.Select(exRule =>
        {
            var exRuleEvaluator = new RecurrencePatternEvaluator(exRule);
            return exRuleEvaluator.Evaluate(referenceDate, timeZone, effPeriodStart, options);
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
    /// <param name="periodKinds">The period kinds to be returned. Used as a filter.</param>
    private IEnumerable<DataTypes.Period> EvaluateExDate(params PeriodKind[] periodKinds)
        => new SortedSet<DataTypes.Period>(Recurrable.ExceptionDates.GetAllPeriodsByKind(periodKinds));

    /// <summary>
    /// Determines the end of an occurrence given the start.
    /// This usually depends on the original reccurring source.
    /// </summary>
    /// <param name="start">Start of an occurrence</param>
    /// <returns>End of an occurrence</returns>
    protected abstract ZonedDateTime GetEnd(ZonedDateTime start);

    public override IEnumerable<EvaluationPeriod> Evaluate(CalDateTime referenceDate, DateTimeZone timeZone, Instant? periodStart, EvaluationOptions? options)
    {
        IEnumerable<EvaluationPeriod> rruleOccurrences;

        // Evaluate recurrence in the reference zone
        var zonedReference = referenceDate.AsZonedOrDefault(timeZone);

        // Only add referenceDate if there are no RecurrenceRules defined. This is in line
        // with RFC 5545 which requires DTSTART to match any RRULE. If it doesn't, the behaviour
        // is undefined. It seems to be good practice not to return the referenceDate in this case.
        rruleOccurrences = !Recurrable.RecurrenceRules.Any()
            ? [new EvaluationPeriod(zonedReference)]
            : EvaluateRRule(referenceDate, zonedReference.Zone, periodStart, options);

        var rdateOccurrences = EvaluateRDate(zonedReference.Zone);

        var periods =
            rruleOccurrences
            .OrderedMerge(rdateOccurrences)
            .OrderedDistinct();

        // Apply the default duration, if any.
        periods = periods.Select(p => (p.End != null) ? p : new EvaluationPeriod(p.Start, GetEnd(p.Start)));

        // Filter by periodStart
        if (periodStart is not null)
        {
            // Include occurrences that start before periodStart, but end after periodStart.
            periods = periods.Where(p => (p.Start.ToInstant() >= periodStart.Value)
                || (p.End?.ToInstant() > periodStart.Value));
        }

        var exRuleExclusions = EvaluateExRule(referenceDate, zonedReference.Zone, options);

        // EXDATEs could contain date-only entries while DTSTART is date-time. This case isn't clearly defined
        // by the RFC, but it seems to be used in the wild (see https://github.com/ical-org/ical.net/issues/829).
        // Different systems handle this differently, e.g. Outlook excludes any occurrences where the date portion
        // matches an date-only EXDATE, while Google Calendar ignores such EXDATEs completely, if DTSTART is date-time.
        // In Ical.Net we follow the Outlook approach, which requires us to handle date-only EXDATEs separately.
        var exDateExclusionsDateOnly = new HashSet<LocalDate>(EvaluateExDate(PeriodKind.DateOnly)
            .Select(x => x.StartTime.ToLocalDateTime().Date));

        var exDateExclusionsDateTime = EvaluateExDate(PeriodKind.DateTime)
            .Select(x => new EvaluationPeriod(x.StartTime.ToZonedDateTime(zonedReference.Zone)));

        // Exclude occurrences according to EXRULEs and EXDATEs.
        periods = periods
            .OrderedExclude(exRuleExclusions)
            .OrderedExclude(exDateExclusionsDateTime)

            // We accept date-only EXDATEs to be used with date-time DTSTARTs. In such cases we exclude those occurrences
            // that, in their respective time zone, have a date component that matches an EXDATE.
            // See https://github.com/ical-org/ical.net/pull/830 for more information.
            //
            // The order of dates in the EXDATEs doesn't necessarily match the order of dates returned by RDATEs
            // due to RDATEs could have different time zones. We therefore use a regular `.Where()` to look up
            // the EXDATEs in the HashSet rather than using `.OrderedExclude()`, which would require correct ordering.
            .Where(dt => !exDateExclusionsDateOnly.Contains(dt.Start.Date));

        // Convert results to the requested time zone
        periods = periods.Select(x => x.WithZone(timeZone));

        periods = periods
            // Convert overflow exceptions to expected ones.
            .HandleEvaluationExceptions();

        return periods;
    }
}
