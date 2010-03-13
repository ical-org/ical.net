using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class TodoPeriodEvaluator :
        RecurringComponentPeriodEvaluator
    {
        #region Protected Properties

        protected ITodo Todo
        {
            get
            {
                return Component as ITodo;
            }
        }

        #endregion

        #region Constructors

        public TodoPeriodEvaluator(ITodo todo) : base(todo)
        {
        }

        #endregion

        #region Public Methods

        public void EvaluateToPreviousOccurrence(iCalDateTime completedDate, iCalDateTime currDt)
        {
            iCalDateTime beginningDate = completedDate;

            if (Todo.RecurrenceRules != null)
            {
                foreach (IRecurrencePattern rrule in Todo.RecurrenceRules)
                    DetermineStartingRecurrence(rrule, ref beginningDate);
            }
            if (Todo.RecurrenceDates != null)
            {
                foreach (IRecurrenceDate rdate in Todo.RecurrenceDates) 
                    DetermineStartingRecurrence(rdate, ref beginningDate);
            }
            if (Todo.ExceptionRules != null)
            {
                foreach (IRecurrencePattern exrule in Todo.ExceptionRules) 
                    DetermineStartingRecurrence(exrule, ref beginningDate);
            }
            if (Todo.ExceptionDates != null)
            {
                foreach (IRecurrenceDate exdate in Todo.ExceptionDates) 
                    DetermineStartingRecurrence(exdate, ref beginningDate);
            }

            Evaluate(Todo.Start, beginningDate, currDt);
        }

        public void DetermineStartingRecurrence(IRecurrenceDate rdate, ref iCalDateTime dt)
        {
            IPeriodEvaluator evaluator = rdate.GetService(typeof(IPeriodEvaluator)) as IPeriodEvaluator;
            if (evaluator == null)
            {
                // FIXME: throw a specific, typed exception here.
                throw new Exception("Could not determine starting recurrence: a period evaluator could not be found!");
            }

            foreach (Period p in evaluator.Periods)
            {
                if (p.StartTime < dt)
                    dt = p.StartTime;
            }
        }

        public void DetermineStartingRecurrence(IRecurrencePattern recur, ref iCalDateTime dt)
        {
            if (recur.Count != int.MinValue)
                dt = Todo.Start;
            else recur.IncrementDate(ref dt, -recur.Interval);
        }

        #endregion

        #region Overrides

        public override IList<Period> Evaluate(iCalDateTime startDate, iCalDateTime fromDate, iCalDateTime toDate)
        {
            // TODO items can only recur if a start date is specified
            if (Todo.Start.IsAssigned)
            {
                // Add the todo itself, before recurrence rules are evaluated
                Period startPeriod = new Period(Todo.Start);
                if (!Periods.Contains(startPeriod))
                    Periods.Add(startPeriod);

                base.Evaluate(startDate, fromDate, toDate);

                // Ensure each period has a duration
                for (int i = 0; i < Periods.Count; i++)
                {
                    Period p = Periods[i];
                    if (p.EndTime == null)
                    {
                        p.Duration = Todo.Duration;
                        if (p.Duration != null)
                            p.EndTime = p.StartTime + Todo.Duration;
                        else p.EndTime = p.StartTime;
                    }
                    else
                    {
                        // Ensure the Kind of time is consistent with Start
                        iCalDateTime dt = p.EndTime;
                        dt.IsUniversalTime = Todo.Start.IsUniversalTime;
                        p.EndTime = dt;
                    }

                    Periods[i] = p;
                }

                return Periods;
            }
            return new List<Period>();
        }

        #endregion
    }
}
