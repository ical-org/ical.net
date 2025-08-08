//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net.Evaluation;

public class TodoEvaluator : RecurringEvaluator
{
    protected Todo Todo => Recurrable as Todo ?? throw new InvalidOperationException();

    protected override DataTypes.Duration? DefaultDuration => Todo.EffectiveDuration;

    public TodoEvaluator(Todo todo) : base(todo) { }

    internal IEnumerable<EvaluationPeriod> EvaluateToPreviousOccurrence(ZonedDateTime completedDate, ZonedDateTime currDt, EvaluationOptions? options)
    {
        // Cannot evaluate a todos that have no start date
        if (Todo.Start == null)
            return [];

        var beginningDate = completedDate;

        foreach (var rrule in Todo.RecurrenceRules)
        {
            DetermineStartingRecurrence(rrule, ref beginningDate);
        }

        DetermineStartingRecurrence(Todo.RecurrenceDates.GetAllPeriods()
            .Select(x => new EvaluationPeriod(x.StartTime.ToZonedDateTime(completedDate.Zone), x.EndTime?.ToZonedDateTime(completedDate.Zone))), ref beginningDate);
        DetermineStartingRecurrence(Todo.RecurrenceDates.GetAllDates()
            .Select(x => x.ToZonedDateTime(completedDate.Zone)), ref beginningDate);

        foreach (var exrule in Todo.ExceptionRules)
        {
            DetermineStartingRecurrence(exrule, ref beginningDate);
        }

        DetermineStartingRecurrence(Todo.ExceptionDates.GetAllDates()
            .Select(x => x.ToZonedDateTime(completedDate.Zone)), ref beginningDate);

        if (Todo.Start == null)
        {
            throw new InvalidOperationException("Todo.Start must not be null.");
        }

        return Evaluate(Todo.Start, beginningDate, options)
            .Where(p => p.Start.ToInstant() <= currDt.ToInstant());
    }

    private static void DetermineStartingRecurrence(IEnumerable<EvaluationPeriod> rdate, ref ZonedDateTime referenceDateTime)
    {
        var dt2 = referenceDateTime;
        foreach (var p in rdate.Where(p => p.Start.ToInstant() < dt2.ToInstant()))
        {
            referenceDateTime = p.Start;
        }
    }

    private static void DetermineStartingRecurrence(IEnumerable<ZonedDateTime> rdate, ref ZonedDateTime referenceDateTime)
    {
        var dt2 = referenceDateTime;
        foreach (var dt in rdate.Where(dt => dt.ToInstant() < dt2.ToInstant()))
        {
            referenceDateTime = dt;
        }
    }

    private void DetermineStartingRecurrence(RecurrencePattern recur, ref ZonedDateTime referenceDateTime)
    {
        if (Todo.Start is null) return;

        if (recur.Count.HasValue)
        {
            // Caller ensures that Start is not null
            referenceDateTime = Todo.Start!.AsZonedOrDefault(referenceDateTime.Zone);
        }
        else
        {
            IncrementDate(ref referenceDateTime, recur, -recur.Interval);
        }
    }

    protected override EvaluationPeriod EvaluateRDate(DataTypes.Period rdate, DateTimeZone referenceTimeZone)
    {
        var start = rdate.StartTime.AsZonedOrDefault(referenceTimeZone);

        ZonedDateTime? end = null;
        if (rdate.Duration is { } duration)
        {
            end = start.LocalDateTime
                .Plus(duration.GetNominalPart())
                .InZone(start.Zone, ResolveFrom(start))
                .Plus(duration.GetTimePart());
        }
        else if (rdate.EndTime is { } dtEnd)
        {
            var exactDuration = dtEnd.ToInstant() - rdate.StartTime.ToInstant();

            if (exactDuration < NodaTime.Duration.Zero)
            {
                throw new InvalidOperationException("DtEnd is before DtStart");
            }

            end = start.Plus(exactDuration);
        }
        else
        {
            // Use event
            end = GetEnd(start);
        }

        return new EvaluationPeriod(start, end);
    }

    protected override ZonedDateTime GetEnd(ZonedDateTime start)
    {
        var dtStart = Todo.DtStart;

        if (Todo.Duration is { } duration)
        {
            if (dtStart is null)
            {
                throw new EvaluationException("DtStart must be set when a TODO has a duration.");
            }

            // Add nominal values (weeks & days) to local time
            // and then add accurate time to zoned time.
            return start.LocalDateTime
                .Plus(duration.GetNominalPart())
                .InZone(start.Zone, ResolveFrom(start))
                .Plus(duration.GetTimePart());
        }

        if (Todo.Due is { } due && dtStart is not null)
        {
            var exactDuration = due.ToInstant() - dtStart.ToInstant();
            return start.Plus(exactDuration);
        }

        // No due date, just return start
        return start;
    }

    public override IEnumerable<EvaluationPeriod> Evaluate(CalDateTime referenceDate, DateTimeZone timeZone, Instant? periodStart, EvaluationOptions? options)
    {
        // Items can only recur if a start date is specified
        if (Todo.Start == null)
            return [];

        return base.Evaluate(referenceDate, timeZone, periodStart, options);
    }
}
