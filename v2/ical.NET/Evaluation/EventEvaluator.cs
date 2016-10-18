﻿using System;
using System.Collections.Generic;
using System.Linq;
using ical.net.Interfaces.Components;
using ical.net.Interfaces.DataTypes;

namespace ical.net.Evaluation
{
    public class EventEvaluator : RecurringEvaluator
    {
        protected Event Event
        {
            get { return Recurrable as Event; }
            set { Recurrable = value; }
        }

        public EventEvaluator(Event evt) : base(evt) {}

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
        public override HashSet<IPeriod> Evaluate(IDateTime referenceTime, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Evaluate recurrences normally
            base.Evaluate(referenceTime, periodStart, periodEnd, includeReferenceDateInResults);

            foreach (var period in Periods.Where(period => period.EndTime == null))
            {
                period.Duration = Event.Duration;
                period.EndTime = period.Duration == null ? period.StartTime : period.StartTime.Add(Event.Duration);
            }

            // Ensure each period has a duration
            //for (var i = 0; i < Periods.Count; i++)
            //{
            //    var p = Periods[i];
            //    if (p.EndTime == null)
            //    {
            //        p.Duration = Event.Duration;
            //        if (p.Duration != null)
            //            p.EndTime = p.StartTime.Add(Event.Duration);
            //        else p.EndTime = p.StartTime;
            //    }
            //}

            return Periods;
        }
    }
}