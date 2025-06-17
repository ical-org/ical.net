//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

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
    /// This method only evaluates events which occur at or after<paramref name="periodStart"/>.
    /// </summary>
    /// <remarks>
    /// For events with very complex recurrence rules, this method may be a bottleneck
    /// during processing time, especially when this method in called for a large number
    /// of events, in sequence, or for a very large time span.
    /// </remarks>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, EvaluationOptions? options)
    {
        // Evaluate recurrences normally
        var periods = base.Evaluate(referenceDate, periodStart, options)
            .Select(WithFinalDuration);

        return periods;
    }

    /// <summary>
    /// The <paramref name="period"/> to evaluate has the <see cref="Period.StartTime"/> set,
    /// but neither <see cref="Period.EndTime"/> nor <see cref="Period.Duration"/> are set.
    /// </summary>
    /// <param name="period">The period where <see cref="Period.Duration"/> will be set.</param>
    /// <returns>Returns the <paramref name="period"/> with <see cref="Period.Duration"/> and exact <see cref="Period.EffectiveEndTime"/> set.</returns>
    private Period WithFinalDuration(Period period)
    {
        try
        {
            /*
               The period's Duration evaluates the event's definition of DtStart
               and Duration, or the timespan from DtStart to DtEnd.

               The time span is used, because the period end time gets the same timezone as the event end time.
               This ensures that the end time is correct, even for DST transitions.

               The exact duration is calculated from the zoned end time and the zoned start time,
               and it may differ from the time span added to the period start time.
             */

            // The preliminary duration was set in a previous evaluation step
            var duration = period.Duration;

            if (duration == null)
            {
                duration = CalendarEvent.EffectiveDuration;
            }

            var newPeriod = new Period(
                start: period.StartTime,
                duration: duration.Value);

            return newPeriod;
        }
        catch (ArgumentOutOfRangeException)
        {
            // intentionally don't include the outer exception
            throw new EvaluationOutOfRangeException("Evaluation aborted: Calculating the end time of the event occurrence resulted in an out-of-range value. This commonly happens when trying to enumerate an unbounded RRULE to its end. Consider applying the .TakeWhile() operator.");
        }
    }
}
