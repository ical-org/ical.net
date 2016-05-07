using System.Collections.Generic;

namespace DDay.iCal
{
    public class RecurrenceUtil
    {
        static public void ClearEvaluation(IRecurrable recurrable)
        {
            var evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator != null)
                evaluator.Clear();
        }

        static public HashSet<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime dt, bool includeReferenceDateInResults)
        {
            return GetOccurrences(
                recurrable, 
                new iCalDateTime(dt.AsSystemLocal.Date), 
                new iCalDateTime(dt.AsSystemLocal.Date.AddDays(1).AddSeconds(-1)),
                includeReferenceDateInResults);
        }

        static public HashSet<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime periodStart, IDateTime periodEnd, bool includeReferenceDateInResults)
        {
            var occurrences = new HashSet<Occurrence>();

            var evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator != null)
            {
                // Ensure the start time is associated with the object being queried
                var start = recurrable.Start.Copy<IDateTime>();
                start.AssociatedObject = recurrable as ICalendarObject;

                // Change the time zone of periodStart/periodEnd as needed 
                // so they can be used during the evaluation process.
                periodStart = DateUtil.MatchTimeZone(start, periodStart);
                periodEnd = DateUtil.MatchTimeZone(start, periodEnd);

                var periods = evaluator.Evaluate(
                    start,
                    DateUtil.GetSimpleDateTimeData(periodStart),
                    DateUtil.GetSimpleDateTimeData(periodEnd),
                    includeReferenceDateInResults);

                foreach (var p in periods)
                {
                    // Filter the resulting periods to only contain those 
                    // that occur sometime between startTime and endTime.
                    // NOTE: fixes bug #3007244 - GetOccurences not returning long spanning all-day events 
                    var endTime = p.EndTime ?? p.StartTime;
                    if (endTime.GreaterThan(periodStart) && p.StartTime.LessThanOrEqual(periodEnd))
                        occurrences.Add(new Occurrence(recurrable, p));
                }
            }
            return occurrences;
        }

        static public bool?[] GetExpandBehaviorList(IRecurrencePattern p)
        {
            // See the table in RFC 5545 Section 3.3.10 (Page 43).
            switch (p.Frequency)
            {                
                case FrequencyType.Minutely: return new bool?[] { false, null, false, false, false, false, false, true, false };
                case FrequencyType.Hourly:   return new bool?[] { false, null, false, false, false, false, true, true, false };
                case FrequencyType.Daily:    return new bool?[] { false, null, null, false, false, true, true, true, false };
                case FrequencyType.Weekly:   return new bool?[] { false, null, null, null, true, true, true, true, false };
                case FrequencyType.Monthly:
                    {
                        var row = new bool?[] { false, null, null, true, true, true, true, true, false };

                        // Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
                        if (p.ByMonthDay.Count > 0)
                            row[4] = false;

                        return row;
                    }
                case FrequencyType.Yearly:
                    {
                        var row = new bool?[] { true, true, true, true, true, true, true, true, false };

                        // Limit if BYYEARDAY or BYMONTHDAY is present; otherwise,
                        // special expand for WEEKLY if BYWEEKNO present; otherwise,
                        // special expand for MONTHLY if BYMONTH present; otherwise,
                        // special expand for YEARLY.
                        if (p.ByYearDay.Count > 0 || p.ByMonthDay.Count > 0)
                            row[4] = false;

                        return row;
                    }
                default:
                    return new bool?[] { false, null, false, false, false, false, false, false, false };
            }
        }
    }
}
