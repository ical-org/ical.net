using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class RecurringComponentPeriodEvaluator :
        PeriodEvaluator
    {
        #region Private Fields

        private IRecurringComponent m_Component;

        #endregion

        #region Protected Properties

        protected IRecurringComponent Component
        {
            get { return m_Component; }
            set { m_Component = value; }
        }

        #endregion

        #region Constructors

        public RecurringComponentPeriodEvaluator(IRecurringComponent comp)
        {
            Component = comp;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period
        /// to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRRule(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle RRULEs
            if (Component.RecurrenceRules != null)
            {
                foreach (IRecurrencePattern rrule in Component.RecurrenceRules)
                {
                    // Get a list of static occurrences
                    // This is important to correctly calculate
                    // recurrences with COUNT.
                    rrule.StaticOccurrences = new List<iCalDateTime>();
                    foreach (Period p in Periods)
                        rrule.StaticOccurrences.Add(p.StartTime);

                    //
                    // Determine the last allowed date in this recurrence
                    //
                    if (rrule.Until.IsAssigned && (!m_Until.IsAssigned || m_Until < rrule.Until))
                        m_Until = rrule.Until;

                    IList<iCalDateTime> DateTimes = rrule.Evaluate(Component.Start, FromDate, ToDate);
                    for (int i = 0; i < DateTimes.Count; i++)
                    {
                        iCalDateTime newDt = new iCalDateTime(DateTimes[i]);
                        newDt.TZID = Component.Start.TZID;

                        DateTimes[i] = newDt;
                        Period p = new Period(newDt);

                        if (!Periods.Contains(p))
                        {
                            this.Periods.Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evalates the RDate component, and adds each specified DateTime or
        /// Period to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRDate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle RDATEs
            if (Component.RecurrenceDates != null)
            {
                foreach (IRecurrenceDate rdate in Component.RecurrenceDates)
                {
                    IList<Period> periods = rdate.Evaluate(DTStart, FromDate, ToDate);
                    foreach (Period p in periods)
                    {
                        if (!Periods.Contains(p))
                        {
                            Periods.Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evaulates the ExRule component, and excludes each specified DateTime
        /// from the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExRule(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle EXRULEs
            if (Component.ExceptionRules != null)
            {
                foreach (IRecurrencePattern exrule in Component.ExceptionRules)
                {
                    IList<iCalDateTime> dateTimes = exrule.Evaluate(Component.Start, FromDate, ToDate);
                    for (int i = 0; i < dateTimes.Count; i++)
                    {
                        iCalDateTime dt = dateTimes[i];
                        dt.TZID = Component.Start.TZID;
                        dateTimes[i] = dt;

                        Period p = new Period(dt);
                        if (this.Periods.Contains(p))
                            this.Periods.Remove(p);
                    }
                }
            }
        }

        /// <summary>
        /// Evalates the ExDate component, and excludes each specified DateTime or
        /// Period from the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExDate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            // Handle EXDATEs
            if (Component.ExceptionDates != null)
            {
                foreach (IRecurrenceDate exdate in Component.ExceptionDates)
                {
                    IList<Period> periods = exdate.Evaluate(Component.Start, FromDate, ToDate);
                    for (int i = 0; i < periods.Count; i++)
                    {
                        Period p = periods[i];

                        // If no time was provided for the ExDate, then it excludes the entire day
                        if (!p.StartTime.HasTime || (p.EndTime != null && !p.EndTime.HasTime))
                            p.MatchesDateOnly = true;

                        while (Periods.Contains(p))
                            Periods.Remove(p);
                    }
                }
            }
        }

        #endregion

        #region Overrides

        public override IList<Period> Evaluate(iCalDateTime startDate, iCalDateTime fromDate, iCalDateTime toDate)
        {
            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((!EvaluationStartBounds.IsAssigned && !EvaluationEndBounds.IsAssigned) ||
                (ToDate == EvaluationStartBounds) ||
                (FromDate == EvaluationEndBounds))
            {
                EvaluateRRule(FromDate, ToDate);
                EvaluateRDate(FromDate, ToDate);
                EvaluateExRule(FromDate, ToDate);
                EvaluateExDate(FromDate, ToDate);
                if (!EvaluationStartBounds.IsAssigned || EvaluationStartBounds > FromDate)
                    EvaluationStartBounds = FromDate;
                if (!EvaluationEndBounds.IsAssigned || EvaluationEndBounds < ToDate)
                    EvaluationEndBounds = ToDate;
            }

            if (EvaluationStartBounds.IsAssigned && FromDate < EvaluationStartBounds)
                Evaluate(FromDate, EvaluationStartBounds);
            if (EvaluationEndBounds.IsAssigned && ToDate > EvaluationEndBounds)
                Evaluate(EvaluationEndBounds, ToDate);

            // FIXME: can't sort an IList...
            Periods.Sort();

            for (int i = 0; i < Periods.Count; i++)
            {
                Period p = Periods[i];

                // Ensure the Kind of time is consistent with DTStart
                iCalDateTime startTime = p.StartTime;
                startTime.IsUniversalTime = Component.Start.IsUniversalTime;
                p.StartTime = startTime;

                Periods[i] = p;
            }

            return Periods;
        }

        #endregion
    }
}
