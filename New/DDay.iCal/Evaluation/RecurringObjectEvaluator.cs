using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class RecurringObjectEvaluator :
        Evaluator
    {
        #region Private Fields

        private IRecurrable m_Recurrable;

        #endregion

        #region Protected Properties

        protected IRecurrable Recurrable
        {
            get { return m_Recurrable; }
            set { m_Recurrable = value; }
        }

        #endregion

        #region Constructors

        public RecurringObjectEvaluator(IRecurrable obj)
        {
            Recurrable = obj;

            // We're not sure if the object is a calendar object
            // or a calendar data type, so we need to assign
            // the associated object manually
            if (obj is ICalendarObject)
                AssociatedObject = (ICalendarObject)obj;
            if (obj is ICalendarDataType)
            {
                ICalendarDataType dt = (ICalendarDataType)obj;
                AssociatedObject = dt.AssociatedObject;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period
        /// to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRRule(IDateTime startTime, IDateTime fromTime, IDateTime toTime)
        {
            // Handle RRULEs
            if (Recurrable.RecurrenceRules != null)
            {
                foreach (IRecurrencePattern rrule in Recurrable.RecurrenceRules)
                {
                    IEvaluator evaluator = rrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        //// Get a list of static occurrences
                        //// This is important to correctly calculate
                        //// recurrences with COUNT.
                        // FIXME: static occurrences can be added elsewhere.
                        //evaluator.StaticOccurrences.Clear();
                        //foreach (Period p in Periods)
                        //    evaluator.StaticOccurrences.Add(p.StartTime);

                        ////
                        //// Determine the last allowed date in this recurrence
                        ////
                        // FIXME: can this be removed?
                        //if (rrule.Until != null && (m_Until == null || m_Until < rrule.Until))
                        //    m_Until = rrule.Until.Copy<IDateTime>();

                        IList<IPeriod> periods = evaluator.Evaluate(startTime, fromTime, toTime);
                        foreach (IPeriod p in periods)
                        {
                            if (!Periods.Contains(p))
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
        virtual protected void EvaluateRDate(IDateTime startTime, IDateTime fromTime, IDateTime toTime)
        {
            // Handle RDATEs
            if (Recurrable.RecurrenceDates != null)
            {
                foreach (IRecurrenceDate rdate in Recurrable.RecurrenceDates)
                {
                    IEvaluator evaluator = rdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(startTime, fromTime, toTime);
                        foreach (IPeriod p in periods)
                        {
                            if (!Periods.Contains(p))
                            {
                                Periods.Add(p);
                            }
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
        virtual protected void EvaluateExRule(IDateTime startTime, IDateTime fromTime, IDateTime toTime)
        {
            // Handle EXRULEs
            if (Recurrable.ExceptionRules != null)
            {
                foreach (IRecurrencePattern exrule in Recurrable.ExceptionRules)
                {
                    IEvaluator evaluator = exrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(startTime, fromTime, toTime);
                        for (int i = 0; i < periods.Count; i++)
                        {
                            IPeriod p = periods[i];
                            if (this.Periods.Contains(p))
                                this.Periods.Remove(p);
                        }
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
        virtual protected void EvaluateExDate(IDateTime startTime, IDateTime fromTime, IDateTime toTime)
        {
            // Handle EXDATEs
            if (Recurrable.ExceptionDates != null)
            {
                foreach (IRecurrenceDate exdate in Recurrable.ExceptionDates)
                {
                    IEvaluator evaluator = exdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(startTime, fromTime, toTime);
                        for (int i = 0; i < periods.Count; i++)
                        {
                            IPeriod p = periods[i];

                            // If no time was provided for the ExDate, then it excludes the entire day
                            if (!p.StartTime.HasTime || (p.EndTime != null && !p.EndTime.HasTime))
                                p.MatchesDateOnly = true;

                            while (Periods.Contains(p))
                                Periods.Remove(p);
                        }
                    }
                }
            }
        }

        #endregion

        #region Overrides

        public override IList<IPeriod> Evaluate(IDateTime startTime, IDateTime fromTime, IDateTime toTime)
        {
            Associate(startTime, fromTime, toTime);

            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == null && EvaluationEndBounds == null) ||
                (toTime == EvaluationStartBounds) ||
                (fromTime == EvaluationEndBounds))
            {
                EvaluateRRule(startTime, fromTime, toTime);
                EvaluateRDate(startTime, fromTime, toTime);
                EvaluateExRule(startTime, fromTime, toTime);
                EvaluateExDate(startTime, fromTime, toTime);
                if (EvaluationStartBounds == null || EvaluationStartBounds.GreaterThan(fromTime))
                    EvaluationStartBounds = fromTime.Copy<IDateTime>();
                if (EvaluationEndBounds == null || EvaluationEndBounds.LessThan(toTime))
                    EvaluationEndBounds = toTime.Copy<IDateTime>();
            }

            if (EvaluationStartBounds != null && fromTime.LessThan(EvaluationStartBounds))
                Evaluate(startTime, fromTime, EvaluationStartBounds);
            if (EvaluationEndBounds != null && toTime.GreaterThan(EvaluationEndBounds))
                Evaluate(startTime, EvaluationEndBounds, toTime);

            // Sort the list
            m_Periods.Sort();

            for (int i = 0; i < Periods.Count; i++)
            {
                IPeriod p = Periods[i];

                // Ensure the time's properties are consistent with the start time
                p.StartTime.TZID = Recurrable.Start.TZID;
                p.StartTime.AssociateWith(Recurrable.Start.AssociatedObject);
            }

            Deassociate(startTime, fromTime, toTime);

            return Periods;
        }

        #endregion
    }
}
