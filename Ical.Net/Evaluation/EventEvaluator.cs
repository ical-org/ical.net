//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.CalendarComponents;
using NodaTime;

namespace Ical.Net.Evaluation;

/// <summary>
/// Evaluates a <see cref="CalendarEvent"/> to determine the dates and times for which the event occurs.
/// </summary>
public class EventEvaluator : RecurringEvaluator
{
    protected CalendarEvent CalendarEvent => (CalendarEvent) Recurrable;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventEvaluator"/> class.
    /// </summary>
    /// <param name="evt"></param>
    public EventEvaluator(CalendarEvent evt) : base(evt) { }

    protected override EvaluationPeriod EvaluateRDate(DataTypes.Period rdate, DateTimeZone referenceTimeZone)
	{
		var start = rdate.StartTime.ToZonedOrDefault(referenceTimeZone);

		ZonedDateTime? end;
		if (rdate.Duration is { } duration)
		{
			if (!rdate.StartTime.HasTime && duration.HasTime)
			{
				throw new EvaluationException($"Unable to add time to date-only RDATE {rdate}");
			}

			end = start.LocalDateTime
				.Plus(duration.GetNominalPart())
				.InZoneRelativeTo(start)
				.Plus(duration.GetTimePart());
		}
		else if (rdate.EndTime is { } dtEnd)
		{
			var exactDuration = dtEnd.ToZonedOrDefault(referenceTimeZone).ToInstant() - start.ToInstant();

			if (exactDuration < Duration.Zero)
			{
				throw new InvalidOperationException("DtEnd is before DtStart");
			}

			end = start.Plus(exactDuration);
		}
		else
		{
			if (!rdate.StartTime.HasTime
				&& CalendarEvent.Duration is { } eventDuration
				&& eventDuration.HasTime)
			{
				throw new EvaluationException($"Unable to add time to date-only RDATE {rdate}");
			}

			// Use event
			end = GetEnd(start);
		}

		return new EvaluationPeriod(start, end);
	}

    protected override ZonedDateTime GetEnd(ZonedDateTime start)
    {
        if (CalendarEvent.Duration is { } duration)
        {
            // Add nominal values (weeks & days) to local time
            // and then add accurate time to zoned time.
            return start.LocalDateTime
                .Plus(duration.GetNominalPart())
                .InZoneRelativeTo(start)
                .Plus(duration.GetTimePart());
        }

        if (CalendarEvent.DtStart is not { } dtStart)
        {
            throw new InvalidOperationException("DtStart must be set.");
        }

        if (CalendarEvent.DtEnd is { } dtEnd)
        {
            // The spec says specifying DTEND results in exact time,
            // but tests say that all day events should be treated
            // as a nominal duration.
            if (!dtStart.HasTime && !dtEnd.HasTime)
            {
                // Calculate nominal duration between dates
                var nominalDuration = dtEnd.Date.Minus(dtStart.Date);

                var end = start.LocalDateTime
                    .Plus(nominalDuration)
                    .InZoneRelativeTo(start);

                if (end.LocalDateTime < start.LocalDateTime)
                {
                    throw new InvalidOperationException("DtEnd is before DtStart");
                }

                return end;
            }

            // The spec says DtEnd MUST be the same type as DtStart.
            // Some cases can be reasonably handled though.

            // Assume a floating end is in the time zone of the event.
            if (dtStart.TzId != null && dtEnd.TzId == null && dtEnd.Time != null)
            {
                dtEnd = new(dtEnd.ToLocalDateTime(), dtStart.TzId);
            }

            var exactDuration = dtEnd.ToZonedOrDefault(start.Zone).ToInstant() - dtStart.ToZonedOrDefault(start.Zone).ToInstant();

            if (exactDuration < Duration.Zero)
            {
                throw new InvalidOperationException("DtEnd is before DtStart");
            }

            return start.Plus(exactDuration);
        }

        if (!dtStart.HasTime)
        {
            // Spec says to assume duration is one day date only (nominal, not 24 hours) 
            return start.LocalDateTime
                .Plus(Period.FromDays(1))
                .InZoneRelativeTo(start);
        }

        // Event ends as it starts
        return start;
    }
}
