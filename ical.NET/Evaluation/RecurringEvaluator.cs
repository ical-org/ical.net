using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Evaluation
{
    public class RecurringEvaluator : Evaluator
    {
        protected IRecurrable Recurrable { get; set; }

        public RecurringEvaluator(IRecurrable obj)
        {
            Recurrable = obj;

            // We're not sure if the object is a calendar object
            // or a calendar data type, so we need to assign
            // the associated object manually
            if (obj is ICalendarObject)
            {
                AssociatedObject = (ICalendarObject) obj;
            }
            if (obj is ICalendarDataType)
            {
                var dt = (ICalendarDataType) obj;
                AssociatedObject = dt.AssociatedObject;
            }
        }

        /// <summary>
        /// Evaulates the RRule component, and adds each specified Period to the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        /// <param name="includeReferenceDateInResults"></param>
        protected virtual void EvaluateRRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Handle RRULEs
            if (Recurrable.RecurrenceRules != null && Recurrable.RecurrenceRules.Count > 0)
            {
                var evaluator = Recurrable.RecurrenceRules.First().GetService(typeof(IEvaluator)) as IEvaluator;
                var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
                Periods.UnionWith(periods);
            }
            else if (includeReferenceDateInResults)
            {
                // If no RRULEs were found, then we still need to add
                // the initial reference date to the results.
                IPeriod p = new Period(referenceDate);
                Periods.UnionWith(new [] {p});
            }
        }

        /// <summary>
        /// Evaulates the ExRule component, and excludes each specified DateTime from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        protected virtual void EvaluateExRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            // Handle EXRULEs
            if (Recurrable.ExceptionRules == null)
            {
                return;
            }

            var excluded =
                Recurrable.ExceptionRules.Select(rule => rule.GetService(typeof (IEvaluator)) as IEvaluator)
                    .Where(evaluator => evaluator != null)
                    .SelectMany(evaluator => evaluator.Evaluate(referenceDate, periodStart, periodEnd, false))
                    .ToList();

            Periods.ExceptWith(excluded);
        }

        /// <summary>
        /// Evalates the ExDate component, and excludes each specified DateTime or Period from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        protected virtual void EvaluateExDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            if (Recurrable.ExceptionDates == null || !Recurrable.ExceptionDates.Any())
            {
                return;
            }

            var evaluator = Recurrable.ExceptionDates.First().GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator == null)
            {
                return;
            }

            var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
            foreach (var p in periods.Where(p => !p.StartTime.HasTime || (p.EndTime != null && !p.EndTime.HasTime)))
            {
                p.MatchesDateOnly = true;
            }
            Periods.ExceptWith(periods);
        }

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue) || (periodEnd.Equals(EvaluationStartBounds)) ||
                (periodStart.Equals(EvaluationEndBounds)))
            {
                EvaluateRRule(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
                EvaluateExRule(referenceDate, periodStart, periodEnd);
                EvaluateExDate(referenceDate, periodStart, periodEnd);
                if (EvaluationStartBounds == DateTime.MaxValue || EvaluationStartBounds > periodStart)
                {
                    EvaluationStartBounds = periodStart;
                }
                if (EvaluationEndBounds == DateTime.MinValue || EvaluationEndBounds < periodEnd)
                {
                    EvaluationEndBounds = periodEnd;
                }
            }
            else
            {
                if (EvaluationStartBounds != DateTime.MaxValue && periodStart < EvaluationStartBounds)
                {
                    Evaluate(referenceDate, periodStart, EvaluationStartBounds, includeReferenceDateInResults);
                }
                if (EvaluationEndBounds != DateTime.MinValue && periodEnd > EvaluationEndBounds)
                {
                    Evaluate(referenceDate, EvaluationEndBounds, periodEnd, includeReferenceDateInResults);
                }
            }

            return Periods;
        }
    }
}