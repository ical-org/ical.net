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
        virtual protected void EvaluateRRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Handle RRULEs
            if (Recurrable.RecurrenceRules != null &&
                Recurrable.RecurrenceRules.Count > 0)
            {
                foreach (IRecurrencePattern rrule in Recurrable.RecurrenceRules)
                {
                    IEvaluator evaluator = rrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
                        foreach (IPeriod p in periods)
                        {
                            if (!Periods.Contains(p))
                                Periods.Add(p);
                        }
                    }
                }
            }
            else if (includeReferenceDateInResults)
            {
                // If no RRULEs were found, then we still need to add
                // the initial reference date to the results.
                IPeriod p = new Period(referenceDate.Copy<IDateTime>());
                if (!Periods.Contains(p))
                    Periods.Add(p);
            }
        }

        /// <summary>
        /// Evalates the RDate component, and adds each specified DateTime or
        /// Period to the <see cref="Periods"/> collection.
        /// </summary>
        /// <param name="FromDate">The beginning date of the range to evaluate.</param>
        /// <param name="ToDate">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle RDATEs
            if (Recurrable.RecurrenceDates != null)
            {
                foreach (IPeriodList rdate in Recurrable.RecurrenceDates)
                {
                    IEvaluator evaluator = rdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
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
        virtual protected void EvaluateExRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle EXRULEs
            if (Recurrable.ExceptionRules != null)
            {
                foreach (IRecurrencePattern exrule in Recurrable.ExceptionRules)
                {
                    IEvaluator evaluator = exrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
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
        virtual protected void EvaluateExDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle EXDATEs
            if (Recurrable.ExceptionDates != null)
            {
                foreach (IPeriodList exdate in Recurrable.ExceptionDates)
                {
                    IEvaluator evaluator = exdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        IList<IPeriod> periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
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

        public override IList<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue) ||
                (periodEnd.Equals(EvaluationStartBounds)) ||
                (periodStart.Equals(EvaluationEndBounds)))
            {
                EvaluateRRule(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
                EvaluateRDate(referenceDate, periodStart, periodEnd);
                EvaluateExRule(referenceDate, periodStart, periodEnd);
                EvaluateExDate(referenceDate, periodStart, periodEnd);
                if (EvaluationStartBounds == DateTime.MaxValue || EvaluationStartBounds > periodStart)
                    EvaluationStartBounds = periodStart;
                if (EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < periodEnd)
                    EvaluationEndBounds = periodEnd;
            }
            else 
            {
                if (EvaluationStartBounds != DateTime.MaxValue && periodStart < EvaluationStartBounds)
                    Evaluate(referenceDate, periodStart, EvaluationStartBounds, includeReferenceDateInResults);
                if (EvaluationEndBounds != DateTime.MinValue && periodEnd > EvaluationEndBounds)
                    Evaluate(referenceDate, EvaluationEndBounds, periodEnd, includeReferenceDateInResults);
            }

            // Sort the list
            m_Periods.Sort();

            return Periods;
        }

        #endregion
    }
}
