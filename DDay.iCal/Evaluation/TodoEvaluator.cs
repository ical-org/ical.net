using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class TodoEvaluator :
        RecurringEvaluator
    {
        #region Protected Properties

        protected ITodo Todo
        {
            get
            {
                return Recurrable as ITodo;
            }
        }

        #endregion

        #region Constructors

        public TodoEvaluator(ITodo todo) : base(todo)
        {
        }

        #endregion

        #region Public Methods

        // FIXME: re-implement this:

        //public void EvaluateToPreviousOccurrence(IDateTime completedDate, IDateTime currDt)
        //{
        //    DateTime beginningDate = DateUtil.GetSimpleDateTimeData(completedDate);

        //    if (Todo.RecurrenceRules != null)
        //    {
        //        foreach (IRecurrencePattern rrule in Todo.RecurrenceRules)
        //            DetermineStartingRecurrence(rrule, ref beginningDate);
        //    }
        //    if (Todo.RecurrenceDates != null)
        //    {
        //        foreach (IPeriodList rdate in Todo.RecurrenceDates) 
        //            DetermineStartingRecurrence(rdate, ref beginningDate);
        //    }
        //    if (Todo.ExceptionRules != null)
        //    {
        //        foreach (IRecurrencePattern exrule in Todo.ExceptionRules) 
        //            DetermineStartingRecurrence(exrule, ref beginningDate);
        //    }
        //    if (Todo.ExceptionDates != null)
        //    {
        //        foreach (IPeriodList exdate in Todo.ExceptionDates) 
        //            DetermineStartingRecurrence(exdate, ref beginningDate);
        //    }

        //    Evaluate(Todo.Start, DateUtil.GetSimpleDateTimeData(Todo.Start), beginningDate.UTC, currDt.UTC);
        //}

        //public void DetermineStartingRecurrence(IPeriodList rdate, ref DateTime dt)
        //{
        //    IEvaluator evaluator = rdate.GetService(typeof(IEvaluator)) as IEvaluator;
        //    if (evaluator == null)
        //    {
        //        // FIXME: throw a specific, typed exception here.
        //        throw new Exception("Could not determine starting recurrence: a period evaluator could not be found!");
        //    }

        //    foreach (IPeriod p in evaluator.Periods)
        //    {
        //        if (p.StartTime.LessThan(dt))
        //            dt = p.StartTime;
        //    }
        //}

        //public void DetermineStartingRecurrence(IRecurrencePattern recur, ref DateTime dt)
        //{
        //    if (recur.Count != int.MinValue)
        //        dt = Todo.Start.Copy<IDateTime>();
        //    else
        //        IncrementDate(ref dt, recur, -recur.Interval);
        //}

        #endregion

        #region Overrides

        public override IList<IPeriod> Evaluate(IDateTime referenceDate, DateTime startDate, DateTime fromDate, DateTime toDate)
        {
            // TODO items can only recur if a start date is specified
            if (Todo.Start != null)
            {
                // Add the todo itself, before recurrence rules are evaluated
                IPeriod startPeriod = new Period(Todo.Start);
                if (!Periods.Contains(startPeriod))
                    Periods.Add(startPeriod);

                base.Evaluate(referenceDate, startDate, fromDate, toDate);

                // Ensure each period has a duration
                for (int i = 0; i < Periods.Count; i++)
                {
                    IPeriod p = Periods[i];
                    if (p.EndTime == null)
                    {
                        p.Duration = Todo.Duration;
                        if (p.Duration != null)
                            p.EndTime = p.StartTime.Add(Todo.Duration);
                        else p.EndTime = p.StartTime;
                    }                    
                }

                return Periods;
            }
            return new List<IPeriod>();
        }

        #endregion
    }
}
