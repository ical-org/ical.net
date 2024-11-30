//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation;

public class TodoEvaluator : RecurringEvaluator
{
    protected Todo Todo => Recurrable as Todo;

    public TodoEvaluator(Todo todo) : base(todo) { }

    internal IEnumerable<Period> EvaluateToPreviousOccurrence(IDateTime completedDate, IDateTime currDt)
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

        return Evaluate(Todo.Start, DateUtil.GetSimpleDateTimeData(beginningDate), DateUtil.GetSimpleDateTimeData(currDt).AddTicks(1), true);
    }

    private void DetermineStartingRecurrence(PeriodList rdate, ref IDateTime referenceDateTime)
    {
        var dt2 = referenceDateTime;
        foreach (var p in rdate.Where(p => p.StartTime.LessThan(dt2)))
        {
            referenceDateTime = p.StartTime;
        }
    }

    private void DetermineStartingRecurrence(RecurrencePattern recur, ref IDateTime referenceDateTime)
    {
        if (recur.Count != int.MinValue)
        {
            referenceDateTime = Todo.Start.Copy<IDateTime>();
        }
        else
        {
            var dtVal = referenceDateTime.Value;
            IncrementDate(ref dtVal, recur, -recur.Interval);
            referenceDateTime = new CalDateTime(DateOnly.FromDateTime(dtVal), TimeOnly.FromDateTime(dtVal)) { AssociatedObject = referenceDateTime.AssociatedObject };
        }
    }

    public override IEnumerable<Period> Evaluate(IDateTime referenceDate, DateTime? periodStart, DateTime? periodEnd, bool includeReferenceDateInResults)
    {
        // TODO items can only recur if a start date is specified
        if (Todo.Start == null)
            return [];

        Period PeriodWithDuration(Period p)
        {
            if (p.EndTime != null)
                return p;

            var period = p.Copy<Period>();

            period.Duration = Todo.Duration;
            if (period.Duration != default)
            {
                period.EndTime = period.StartTime.Add(Todo.Duration);
            }
            else
            {
                period.Duration = Todo.Duration;
            }

            return period;
        }

        return base.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults)
            .Select(PeriodWithDuration);
    }
}
