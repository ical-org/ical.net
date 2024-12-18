﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

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

    /// <summary>
    /// Evaluates this event to determine the dates and times for which the event occurs.
    /// This method only evaluates events which occur between <paramref name="periodStart"/>
    /// and <paramref name="periodEnd"/>; therefore, if you require a list of events which
    /// occur outside of this range, you must specify a <paramref name="periodStart"/> and
    /// <paramref name="periodEnd"/> which encapsulate the date(s) of interest.
    /// </summary>
    /// <remarks>
    /// For events with very complex recurrence rules, this method may be a bottleneck
    /// during processing time, especially when this method in called for a large number
    /// of events, in sequence, or for a very large time span.
    /// </remarks>
    /// <param name="referenceTime"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    /// <param name="includeReferenceDateInResults"></param>
    /// <returns></returns>
    public override IEnumerable<Period> Evaluate(IDateTime referenceTime, DateTime? periodStart, DateTime? periodEnd, bool includeReferenceDateInResults)
    {
        // Evaluate recurrences normally
        var periods = base.Evaluate(referenceTime, periodStart, periodEnd, includeReferenceDateInResults)
            .Select(WithEndTime);

        return periods;
    }

    /// <summary>
    /// The <paramref name="period"/> to evaluate has the <see cref="Period.StartTime"/> set,
    /// but neither <see cref="Period.EndTime"/> nor <see cref="Period.Duration"/> are set.
    /// </summary>
    /// <param name="period">The period where <see cref="Period.EndTime"/> will be set.</param>
    /// <returns>Returns the <paramref name="period"/> with <see cref="Period.EndTime"/> and exact <see cref="Period.Duration"/> set.</returns>
    private Period WithEndTime(Period period)
    {
        /*
           Calculate the nominal duration of the event which is the duration of the
           first occurrence from the event's definition.
           
           The nominal duration is used, because the period end time gets the same timezone as the event end time.
           This ensures that the end time is correct, even for DST transitions.
           The exact duration is calculated from the zoned end time and the zoned start time,
           and it may differ from the nominal duration.
         */
        var nominalDuration = CalendarEvent.CalcFirstNominalDuration();

        IDateTime endTime;
        if (nominalDuration == TimeSpan.Zero)
        {
            // For a zero-duration event, the end time is the same as the start time.
            endTime = period.StartTime;
        }
        else
        {
            // Calculate the end time of the event as a DateTime
            var endDt = period.StartTime.Value.Add(nominalDuration);

            // Create a CalDateTime object with the calculated end time.
            // Ensure date-only or date-time consistency with the start time,
            // and use the timezone from the event's definition.
            endTime = new CalDateTime(
                DateOnly.FromDateTime(endDt),
                period.StartTime.HasTime ? TimeOnly.FromDateTime(endDt) : null,
                CalendarEvent.End?.TzId ?? CalendarEvent.Start.TzId);
        }

        // Return the Period object with the calculated end time and duration.
        period.Duration = endTime.Subtract(period.StartTime); // exact duration
        period.EndTime = endTime; // Only EndTime is relevant for further processing.

        return period;
    }
}
