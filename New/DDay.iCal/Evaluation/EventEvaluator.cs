using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class EventEvaluator :
        RecurringEvaluator
    {
        #region Protected Properties

        protected IEvent Event
        {
            get { return Recurrable as IEvent; }
            set { Recurrable = value; }
        }

        #endregion

        #region Constructors

        public EventEvaluator(IEvent evt) : base(evt)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Evaluates this event to determine the dates and times for which the event occurs.
        /// This method only evaluates events which occur between <paramref name="FromDate"/>
        /// and <paramref name="ToDate"/>; therefore, if you require a list of events which
        /// occur outside of this range, you must specify a <paramref name="FromDate"/> and
        /// <paramref name="ToDate"/> which encapsulate the date(s) of interest.
        /// <note type="caution">
        ///     For events with very complex recurrence rules, this method may be a bottleneck
        ///     during processing time, especially when this method in called for a large number
        ///     of events, in sequence, or for a very large time span.
        /// </note>
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        /// <returns></returns>
        public override IList<IPeriod> Evaluate(IDateTime referenceTime, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {   
            // Evaluate recurrences normally
            base.Evaluate(referenceTime, periodStart, periodEnd, includeReferenceDateInResults);

            // Ensure each period has a duration
            for (int i = 0; i < Periods.Count; i++)
            {
                IPeriod p = Periods[i];
                if (p.EndTime == null)
                {
                    p.Duration = Event.Duration;
                    if (p.Duration != null)
                        p.EndTime = p.StartTime.Add(Event.Duration);
                    else p.EndTime = p.StartTime;
                }
            }

            return Periods;
        }

        #endregion
    }
}
