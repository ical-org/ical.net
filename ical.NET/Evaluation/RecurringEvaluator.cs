using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Evaluation
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
                var dt = (ICalendarDataType)obj;
                AssociatedObject = dt.AssociatedObject;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period to the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        /// <param name="includeReferenceDateInResults"></param>
        virtual protected void EvaluateRRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Handle RRULEs
            if (Recurrable.RecurrenceRules != null &&
                Recurrable.RecurrenceRules.Count > 0)
            {
                foreach (var rrule in Recurrable.RecurrenceRules)
                {
                    var evaluator = rrule.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
                        Periods.UnionWith(periods);
                    }
                }
            }
            else if (includeReferenceDateInResults)
            {
                // If no RRULEs were found, then we still need to add
                // the initial reference date to the results.
                IPeriod p = new Period(referenceDate.Copy<IDateTime>());
                Periods.UnionWith(new [] {p});
            }
        }

        /// <summary>
        /// Evalates the RDate component, and adds each specified DateTime or Period to the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        virtual protected void EvaluateRDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle RDATEs
            if (Recurrable.RecurrenceDates != null)
            {
                foreach (var rdate in Recurrable.RecurrenceDates)
                {
                    var evaluator = rdate.GetService(typeof(IEvaluator)) as IEvaluator;
                    if (evaluator != null)
                    {
                        var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
                        Periods.UnionWith(periods);
                    }
                }
            }
        }

        /// <summary>
        /// Evaulates the ExRule component, and excludes each specified DateTime from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle EXRULEs
            if (Recurrable.ExceptionRules == null)
            {
                return;
            }

            var excluded = Recurrable.ExceptionRules
                .Select(rule => rule.GetService(typeof(IEvaluator)) as IEvaluator)
                .Where(evaluator => evaluator != null)
                .SelectMany(evaluator => evaluator.Evaluate(referenceDate,periodStart, periodEnd, false))
                .ToList();

            Periods.ExceptWith(excluded);
        }

        /// <summary>
        /// Evalates the ExDate component, and excludes each specified DateTime or Period from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        virtual protected void EvaluateExDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle EXDATEs
            if (Recurrable.ExceptionDates == null)
            {
                return;
            }


            foreach (var exdate in Recurrable.ExceptionDates)
            {
                var evaluator = exdate.GetService(typeof(IEvaluator)) as IEvaluator;
                if (evaluator == null)
                {
                    continue;
                }

                var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
                foreach (var p in periods.Where(p => !p.StartTime.HasTime || (p.EndTime != null && !p.EndTime.HasTime)))
                {
                    p.MatchesDateOnly = true;
                }
                Periods.ExceptWith(periods);
            }
        }

        #endregion

        #region Overrides

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
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

            return Periods;
        }

        #endregion
    }
}
