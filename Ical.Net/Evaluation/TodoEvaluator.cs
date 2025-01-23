//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation;

public class TodoEvaluator : RecurringEvaluator
{
    protected Todo Todo => Recurrable as Todo ?? throw new InvalidOperationException();

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

        DetermineStartingRecurrence(Todo.RecurrenceDates.GetAllPeriods(), ref beginningDate);
        DetermineStartingRecurrence(Todo.RecurrenceDates.GetAllDates(), ref beginningDate);

        if (Todo.ExceptionRules != null)
        {
            foreach (var exrule in Todo.ExceptionRules)
            {
                DetermineStartingRecurrence(exrule, ref beginningDate);
            }
        }

        DetermineStartingRecurrence(Todo.ExceptionDates.GetAllDates(), ref beginningDate);

        return Evaluate(Todo.Start, DateUtil.GetSimpleDateTimeData(beginningDate), DateUtil.GetSimpleDateTimeData(currDt).AddTicks(1), true);
    }

    private static void DetermineStartingRecurrence(IEnumerable<Period> rdate, ref IDateTime referenceDateTime)
    {
        var dt2 = referenceDateTime;
        foreach (var p in rdate.Where(p => p.StartTime.LessThan(dt2)))
        {
            referenceDateTime = p.StartTime;
        }
    }

    private static void DetermineStartingRecurrence(IEnumerable<IDateTime> rdate, ref IDateTime referenceDateTime)
    {
        var dt2 = referenceDateTime;
        foreach (var dt in rdate.Where(dt => dt.LessThan(dt2)))
        {
            referenceDateTime = dt;
        }
    }

    private void DetermineStartingRecurrence(RecurrencePattern recur, ref IDateTime referenceDateTime)
    {
        if (recur.Count.HasValue)
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

        return base.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults)
            .Select(p => p);
    }
}
