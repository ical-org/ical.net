//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NodaTime;

namespace Ical.Net.Evaluation;

public class TodoEvaluator : RecurringEvaluator
{
    protected Todo Todo => Recurrable as Todo ?? throw new InvalidOperationException();

    public TodoEvaluator(Todo todo) : base(todo) { }

    protected override EvaluationPeriod EvaluateRDate(DataTypes.Period rdate, DateTimeZone referenceTimeZone)
    {
        var start = rdate.StartTime.AsZonedOrDefault(referenceTimeZone);

        ZonedDateTime? end;
        if (rdate.Duration is { } duration)
        {
            end = start.LocalDateTime
                .Plus(duration.GetNominalPart())
                .InZoneRelativeTo(start)
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
                .InZoneRelativeTo(start)
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
