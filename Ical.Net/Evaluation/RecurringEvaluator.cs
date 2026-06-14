//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NodaTime;

namespace Ical.Net.Evaluation;

public abstract class RecurringEvaluator : IEvaluator
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
    protected IEnumerable<ZonedDateTime> EvaluateRRule(CalDateTime referenceDate, DateTimeZone timeZone, Instant? periodStart, EvaluationOptions? options)
    {
        if (Recurrable.RecurrenceRule is null)
            return [];

        var ruleEvaluator = new RecurrenceRuleEvaluator(Recurrable.RecurrenceRule, referenceDate, timeZone, periodStart, options);

        return ruleEvaluator.Evaluate();
    }

    protected abstract EvaluationPeriod EvaluateRDate(DataTypes.Period rdate, DateTimeZone referenceTimeZone);

    /// <summary> Evaluates the RDate component. </summary>
    protected SortedSet<EvaluationPeriod> EvaluateRDate(DateTimeZone referenceTimeZone)
    {
        var result = Recurrable.RecurrenceDates
            .GetAllPeriodsByKind(PeriodKind.Period, PeriodKind.DateOnly, PeriodKind.DateTime)
            .Select(x => EvaluateRDate(x, referenceTimeZone))
            // Convert overflow exceptions to expected ones.
            .HandleEvaluationExceptions();

        return new SortedSet<EvaluationPeriod>(result);
    }

    /// <summary>
    /// Evaluates the ExDate component.
    /// </summary>
    /// <param name="periodKinds">The period kinds to be returned. Used as a filter.</param>
    private IEnumerable<DataTypes.Period> EvaluateExDate(params PeriodKind[] periodKinds)
        => Recurrable.ExceptionDates.GetAllPeriodsByKind(periodKinds);

    /// <summary>
    /// Determines the end of an occurrence given the start.
    /// This usually depends on the original reccurring source.
    /// </summary>
    /// <param name="start">Start of an occurrence</param>
    /// <returns>End of an occurrence</returns>
    protected abstract ZonedDateTime GetEnd(ZonedDateTime start);

    public virtual IEnumerable<EvaluationPeriod> Evaluate(
        CalDateTime referenceDate,
        ZonedDateTime periodStart,
        EvaluationOptions? options) => Evaluate(referenceDate, periodStart.Zone, periodStart.ToInstant(), options);

    public virtual IEnumerable<EvaluationPeriod> Evaluate(
        CalDateTime referenceDate,
        DateTimeZone timeZone,
        Instant? periodStart,
        EvaluationOptions? options)
    {
        // Evaluate recurrence in the reference zone
        var zonedReference = referenceDate.ToZonedOrDefault(timeZone);

        // Only add referenceDate if there is no RecurrenceRule. This is in line
        // with RFC 5545 which requires DTSTART to match any RRULE. If it doesn't, the behaviour
        // is undefined. It seems to be good practice not to return the referenceDate in this case.
        var rruleOccurrences = Recurrable.RecurrenceRule is null
            ? [zonedReference]
            : EvaluateRRule(referenceDate, zonedReference.Zone, periodStart, options);

        var periods = rruleOccurrences
            .Select(start => new EvaluationPeriod(start, GetEnd(start)));

        // Merge with recurrence dates if there are any
        if (!Recurrable.RecurrenceDates.IsEmpty())
        {
            var rdateOccurrences = EvaluateRDate(zonedReference.Zone);

            periods = periods
                .OrderedMerge(rdateOccurrences)
                .OrderedDistinct();
        }

        // Filter by periodStart
        if (periodStart is not null)
        {
            // Include occurrences that start before periodStart, but end after periodStart.
            periods = periods.Where(p => (p.Start.ToInstant() >= periodStart.Value)
                || (p.End.ToInstant() > periodStart.Value));
        }

        // Filter out exception dates if there are any
        if (!Recurrable.ExceptionDates.IsEmpty())
        {
            // Exclude occurrences according to EXDATEs.
            var exDateExclusionsDateTime = new HashSet<Instant>(EvaluateExDate(PeriodKind.DateTime)
                .Select(x => x.StartTime.ToZonedOrDefault(zonedReference.Zone).ToInstant()));

            if (exDateExclusionsDateTime.Count > 0)
            {
                periods = periods.Where(x => !exDateExclusionsDateTime.Contains(x.Start.ToInstant()));
            }

            // EXDATEs could contain date-only entries while DTSTART is date-time. This case isn't clearly defined
            // by the RFC, but it seems to be used in the wild (see https://github.com/ical-org/ical.net/issues/829).
            // Different systems handle this differently, e.g. Outlook excludes any occurrences where the date portion
            // matches an date-only EXDATE, while Google Calendar ignores such EXDATEs completely, if DTSTART is date-time.
            // In Ical.Net we follow the Outlook approach, which requires us to handle date-only EXDATEs separately.
            // In such cases we exclude those occurrences that, in their respective time zone, have a date component
            // that matches an EXDATE.
            var exDateExclusionsDateOnly = new HashSet<LocalDate>(EvaluateExDate(PeriodKind.DateOnly)
                .Select(x => x.StartTime.ToLocalDateTime().Date));

            if (exDateExclusionsDateOnly.Count > 0)
            {
                periods = periods.Where(dt => !exDateExclusionsDateOnly.Contains(dt.Start.Date));
            }
        }

        // Convert results to the requested time zone
        periods = periods.Select(x => x.WithZone(timeZone));

        periods = periods
            // Convert overflow exceptions to expected ones.
            .HandleEvaluationExceptions();

        return periods;
    }
}
