﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

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
            AssociatedObject = (ICalendarObject)obj;
        }
        if (obj is ICalendarDataType)
        {
            var dt = (ICalendarDataType)obj;
            AssociatedObject = dt.AssociatedObject;
        }
    }

    /// <summary>
    /// Evaluates the RRule component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    /// <param name="includeReferenceDateInResults"></param>
    protected HashSet<Period> EvaluateRRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
    {
        if (Recurrable.RecurrenceRules == null || !Recurrable.RecurrenceRules.Any())
        {
            return new HashSet<Period>();
        }

        var periodsQuery = Recurrable.RecurrenceRules.SelectMany(rule =>
        {
            var ruleEvaluator = rule.GetService(typeof(IEvaluator)) as IEvaluator;
            if (ruleEvaluator == null)
            {
                return Enumerable.Empty<Period>();
            }
            return ruleEvaluator.Evaluate(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
        });

        var periods = new HashSet<Period>(periodsQuery);

        //Only add referenceDate if there are no RecurrenceRules defined
        if (includeReferenceDateInResults && (Recurrable.RecurrenceRules == null || !Recurrable.RecurrenceRules.Any()))
        {
            periods.UnionWith(new[] { new Period(referenceDate) });
        }
        return periods;
    }

    /// <summary> Evaluates the RDate component. </summary>
    protected HashSet<Period> EvaluateRDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
    {
        if (Recurrable.RecurrenceDates == null || !Recurrable.RecurrenceDates.Any())
        {
            return new HashSet<Period>();
        }

        var recurrences = new HashSet<Period>(Recurrable.RecurrenceDates.SelectMany(rdate => rdate));
        return recurrences;
    }

    /// <summary>
    /// Evaluates the ExRule component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    protected HashSet<Period> EvaluateExRule(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
    {
        if (Recurrable.ExceptionRules == null || !Recurrable.ExceptionRules.Any())
        {
            return new HashSet<Period>();
        }

        var exRuleEvaluatorQuery = Recurrable.ExceptionRules.SelectMany(exRule =>
        {
            var exRuleEvaluator = exRule.GetService(typeof(IEvaluator)) as IEvaluator;
            if (exRuleEvaluator == null)
            {
                return Enumerable.Empty<Period>();
            }
            return exRuleEvaluator.Evaluate(referenceDate, periodStart, periodEnd, false);
        });

        var exRuleExclusions = new HashSet<Period>(exRuleEvaluatorQuery);
        return exRuleExclusions;
    }

    /// <summary>
    /// Evaluates the ExDate component.
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart">The beginning date of the range to evaluate.</param>
    /// <param name="periodEnd">The end date of the range to evaluate.</param>
    protected HashSet<Period> EvaluateExDate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
    {
        if (Recurrable.ExceptionDates == null || !Recurrable.ExceptionDates.Any())
        {
            return new HashSet<Period>();
        }

        var exDates = new HashSet<Period>(Recurrable.ExceptionDates.SelectMany(exDate => exDate));
        return exDates;
    }

    public override HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
    {
        var periods = new HashSet<Period>();

        var rruleOccurrences = EvaluateRRule(referenceDate, periodStart, periodEnd, includeReferenceDateInResults);
        //Only add referenceDate if there are no RecurrenceRules defined
        if (includeReferenceDateInResults && (Recurrable.RecurrenceRules == null || !Recurrable.RecurrenceRules.Any()))
        {
            rruleOccurrences.UnionWith(new[] { new Period(referenceDate), });
        }

        var rdateOccurrences = EvaluateRDate(referenceDate, periodStart, periodEnd);

        var exRuleExclusions = EvaluateExRule(referenceDate, periodStart, periodEnd);
        var exDateExclusions = EvaluateExDate(referenceDate, periodStart, periodEnd);

        //Exclusions trump inclusions
        periods.UnionWith(rruleOccurrences);
        periods.UnionWith(rdateOccurrences);
        periods.ExceptWith(exRuleExclusions);
        periods.ExceptWith(exDateExclusions);

        var dateOverlaps = FindDateOverlaps(periods, exDateExclusions);
        periods.ExceptWith(dateOverlaps);

        return periods;
    }

    private static HashSet<Period> FindDateOverlaps(HashSet<Period> periods, HashSet<Period> dates)
    {
        var datesWithoutTimes = new HashSet<DateTime>(dates.Where(d => !d.StartTime.HasTime).Select(d => d.StartTime.Value));
        var overlaps = new HashSet<Period>(periods.Where(p => datesWithoutTimes.Contains(p.StartTime.Value.Date)));
        return overlaps;
    }
}
