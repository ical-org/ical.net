using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation
{
    public class TodoEvaluator : RecurringEvaluator
    {
        protected ITodo Todo => Recurrable as ITodo;

        public TodoEvaluator(ITodo todo) : base(todo) {}

        public void EvaluateToPreviousOccurrence(IDateTime completedDate, IDateTime currDt)
        {
            var beginningDate = completedDate.Copy<IDateTime>();

            if (Todo.RecurrenceRules != null)
            {
                foreach (var rrule in Todo.RecurrenceRules)
                {
                    DetermineStartingRecurrence(rrule, ref beginningDate);
                }
            }
            if (Todo.RecurrenceDates != null)
            {
                foreach (var rdate in Todo.RecurrenceDates)
                {
                    DetermineStartingRecurrence(rdate, ref beginningDate);
                }
            }
            if (Todo.ExceptionRules != null)
            {
                foreach (var exrule in Todo.ExceptionRules)
                {
                    DetermineStartingRecurrence(exrule, ref beginningDate);
                }
            }
            if (Todo.ExceptionDates != null)
            {
                foreach (var exdate in Todo.ExceptionDates)
                {
                    DetermineStartingRecurrence(exdate, ref beginningDate);
                }
            }

            Evaluate(Todo.Start, DateUtil.GetSimpleDateTimeData(beginningDate), DateUtil.GetSimpleDateTimeData(currDt).AddTicks(1), true);
        }

        public void DetermineStartingRecurrence(IPeriodList rdate, ref IDateTime dt)
        {
            var evaluator = rdate.GetService(typeof (IEvaluator)) as IEvaluator;
            if (evaluator == null)
            {
                // FIXME: throw a specific, typed exception here.
                throw new Exception("Could not determine starting recurrence: a period evaluator could not be found!");
            }

            foreach (var p in evaluator.Periods)
            {
                if (p.StartTime.LessThan(dt))
                {
                    dt = p.StartTime;
                }
            }
        }

        public void DetermineStartingRecurrence(IRecurrencePattern recur, ref IDateTime dt)
        {
            if (recur.Count != int.MinValue)
            {
                dt = Todo.Start.Copy<IDateTime>();
            }
            else
            {
                var dtVal = dt.Value;
                IncrementDate(ref dtVal, recur, -recur.Interval);
                dt.Value = dtVal;
            }
        }

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // TODO items can only recur if a start date is specified
            if (Todo.Start != null)
            {
                base.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);

                // Ensure each period has a duration
                foreach (var period in Periods.Where(period => period.EndTime == null))
                {
                    period.Duration = Todo.Duration;
                    if (period.Duration != null)
                    {
                        period.EndTime = period.StartTime.Add(Todo.Duration);
                    }
                    else
                    {
                        period.Duration = Todo.Duration;
                    }
                }
                return Periods;
            }
            return new HashSet<IPeriod>();
        }
    }
}