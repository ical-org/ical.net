using System;
using System.Collections.Generic;
using System.Linq;
using ical.net.DataTypes;
using ical.net.Interfaces.DataTypes;
using ical.net.Interfaces.Evaluation;
using ical.net.Interfaces.General;

namespace ical.net.Evaluation
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
        protected HashSet<IPeriod> EvaluateRRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            if (Recurrable.RecurrenceRules == null || !Recurrable.RecurrenceRules.Any())
            {
                return new HashSet<IPeriod>();
            }

            var evaluator = Recurrable.RecurrenceRules.First().GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator == null)
            {
                return new HashSet<IPeriod>();
            }
            var periods = evaluator.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);

            if (includeReferenceDateInResults)
            {
                periods.UnionWith(new[] { new Period(referenceDate) });
            }
            return periods;
        }

        /// <summary> Evalates the RDate component, and adds each specified DateTime or Period to the Periods collection. </summary>
        protected HashSet<IPeriod> EvaluateRDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            if (Recurrable.RecurrenceDates == null || !Recurrable.RecurrenceDates.Any())
            {
                return new HashSet<IPeriod>();
            }

            var recurrences = new HashSet<IPeriod>(Recurrable.RecurrenceDates.SelectMany(rdate => rdate));
            return recurrences;
        }

        /// <summary>
        /// Evaulates the ExRule component, and excludes each specified DateTime from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        protected HashSet<IPeriod> EvaluateExRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            if (Recurrable.ExceptionRules == null || !Recurrable.ExceptionRules.Any())
            {
                return new HashSet<IPeriod>();
            }

            var evaluator = Recurrable.ExceptionRules.First().GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator == null)
            {
                return new HashSet<IPeriod>();
            }

            var exRuleEvaluatorQuery = Recurrable.ExceptionRules.SelectMany(exRule => evaluator.Evaluate(referenceDate, periodStart, periodEnd, false));
            var exRuleExclusions = new HashSet<IPeriod>(exRuleEvaluatorQuery);
            return exRuleExclusions;
        }

        /// <summary>
        /// Evalates the ExDate component, and excludes each specified DateTime or Period from the Periods collection.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="periodStart">The beginning date of the range to evaluate.</param>
        /// <param name="periodEnd">The end date of the range to evaluate.</param>
        protected HashSet<IPeriod> EvaluateExDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            if (Recurrable.ExceptionDates == null || !Recurrable.ExceptionDates.Any())
            {
                return new HashSet<IPeriod>();
            }

            var exDates = new HashSet<IPeriod>(Recurrable.ExceptionDates.SelectMany(exDate => exDate));
            return exDates;
        }

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Evaluate extra time periods, without re-evaluating ones that were already evaluated
            if ((EvaluationStartBounds == DateTime.MaxValue && EvaluationEndBounds == DateTime.MinValue) || periodEnd.Equals(EvaluationStartBounds) ||
                periodStart.Equals(EvaluationEndBounds))
            {
                //Exclusions take precedence over inclusions, so build the master set, then subtract the exclusions from it
                var rruleOccurrences = EvaluateRRule(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
                if (includeReferenceDateInResults)
                {
                    rruleOccurrences.UnionWith(new [] {new Period(referenceDate), });
                }

                var rdateOccurrences = EvaluateRDate(referenceDate, periodStart, periodEnd);

                var exRuleExclusions = EvaluateExRule(referenceDate, periodStart, periodEnd);
                var exDateExclusions = EvaluateExDate(referenceDate, periodStart, periodEnd);

                Periods.UnionWith(rruleOccurrences);
                Periods.UnionWith(rdateOccurrences);
                Periods.ExceptWith(exRuleExclusions);
                Periods.ExceptWith(exDateExclusions);

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