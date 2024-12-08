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

public class EventEvaluator : RecurringEvaluator
{
    protected CalendarEvent CalendarEvent
    {
        get => Recurrable as CalendarEvent;
        set => Recurrable = value;
    }

    public EventEvaluator(CalendarEvent evt) : base(evt) { }

    /// <summary>
    /// Evaluates this event to determine the dates and times for which the event occurs.
    /// This method only evaluates events which occur between <paramref name="periodStart"/>
    /// and <paramref name="periodEnd"/>; therefore, if you require a list of events which
    /// occur outside of this range, you must specify a <paramref name="periodStart"/> and
    /// <paramref name="periodEnd"/> which encapsulate the date(s) of interest.
    /// <note type="caution">
    ///     For events with very complex recurrence rules, this method may be a bottleneck
    ///     during processing time, especially when this method in called for a large number
    ///     of events, in sequence, or for a very large time span.
    /// </note>
    /// </summary>
    /// <param name="referenceTime"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    /// <param name="includeReferenceDateInResults"></param>
    /// <returns></returns>
    public override IEnumerable<Period> Evaluate(IDateTime referenceTime, DateTime? periodStart, DateTime? periodEnd, bool includeReferenceDateInResults)
    {
        Period WithDuration(Period period)
        {
            var duration = CalendarEvent.GetFirstDuration();
            var endTime = duration == default
                ? period.StartTime
                : period.StartTime.Add(CalendarEvent.GetFirstDuration());

            return new Period(period.StartTime)
            {
                Duration = duration,
                EndTime = endTime,
            };
        }

        // Evaluate recurrences normally
        var periods = base.Evaluate(referenceTime, periodStart, periodEnd, includeReferenceDateInResults)
            .Select(WithDuration);

        return periods;
    }
}
