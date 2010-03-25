using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class RecurringEvaluator :
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

        public RecurringEvaluator(IRecurrable obj)
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
        virtual protected void EvaluateRRule(IDateTime referenceDate, DateTime startTime, DateTime fromTime, DateTime toTime)
        {
            // Handle RRULEs
            if (Recurrable.RecurrenceRules != null)
            {
                foreach (IRecurrencePattern rrule in Recurrable.RecurrenceRules)
                {
                    IEvaluator evaluator = rrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, startTime, fromTime, toTime);
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
        virtual protected void EvaluateRDate(IDateTime referenceDate, DateTime startTime, DateTime fromTime, DateTime toTime)
        {
            // Handle RDATEs
            if (Recurrable.RecurrenceDates != null)
            {
                foreach (IPeriodList rdate in Recurrable.RecurrenceDates)
                {
                    IEvaluator evaluator = rdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, startTime, fromTime, toTime);
                        foreach (IPeriod p in periods)
                        {
                            if (!Periods.Contains(p))
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
        virtual protected void EvaluateExRule(IDateTime referenceDate, DateTime startTime, DateTime fromTime, DateTime toTime)
        {
            // Handle EXRULEs
            if (Recurrable.ExceptionRules != null)
            {
                foreach (IRecurrencePattern exrule in Recurrable.ExceptionRules)
                {
                    IEvaluator evaluator = exrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, startTime, fromTime, toTime);
                        foreach (IPeriod p in periods)                        
                        {                            
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
        virtual protected void EvaluateExDate(IDateTime referenceDate, DateTime startTime, DateTime fromTime, DateTime toTime)
        {
            // Handle EXDATEs
            if (Recurrable.ExceptionDates != null)
            {
                foreach (IPeriodList exdate in Recurrable.ExceptionDates)
                {
                    IEvaluator evaluator = exdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, startTime, fromTime, toTime);
                        foreach (IPeriod p in periods)                        
                        {
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

        public override IList<IPeriod> Evaluate(IDateTime referenceDate, DateTime startTime, DateTime fromTime, DateTime toTime)
        {
            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue) ||
                (toTime.Equals(EvaluationStartBounds)) ||
                (fromTime.Equals(EvaluationEndBounds)))
            {
                EvaluateRRule(referenceDate, startTime, fromTime, toTime);
                EvaluateRDate(referenceDate, startTime, fromTime, toTime);
                EvaluateExRule(referenceDate, startTime, fromTime, toTime);
                EvaluateExDate(referenceDate, startTime, fromTime, toTime);
                if (EvaluationStartBounds == null || EvaluationStartBounds > fromTime)
                    EvaluationStartBounds = fromTime;
                if (EvaluationEndBounds == null || EvaluationEndBounds < toTime)
                    EvaluationEndBounds = toTime;
            }

            if (EvaluationStartBounds != null && fromTime < EvaluationStartBounds)
                Evaluate(referenceDate, startTime, fromTime, EvaluationStartBounds);
            if (EvaluationEndBounds != null && toTime > EvaluationEndBounds)
                Evaluate(referenceDate, startTime, EvaluationEndBounds, toTime);

            // Sort the list
            m_Periods.Sort();

            return Periods;
        }

        #endregion
    }
}
