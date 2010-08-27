using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class RecurrenceUtil
    {
        static public void ClearEvaluation(IRecurrable recurrable)
        {
            IEvaluator evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator != null)
                evaluator.Clear();
        }

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime dt, bool includeReferenceDateInResults)
        {
            return GetOccurrences(
                recurrable, 
                new iCalDateTime(dt.Local.Date), 
                new iCalDateTime(dt.Local.Date.AddDays(1).AddSeconds(-1)),
                includeReferenceDateInResults);
        }

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime periodStart, IDateTime periodEnd, bool includeReferenceDateInResults)
        {
            List<Occurrence> occurrences = new List<Occurrence>();

            IEvaluator evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator != null)
            {
                // Change the time zone of periodStart/periodEnd as needed 
                // so they can be used during the evaluation process.
                periodStart = DateUtil.MatchTimeZone(recurrable.Start, periodStart);
                periodEnd = DateUtil.MatchTimeZone(recurrable.Start, periodEnd);

                IList<IPeriod> periods = evaluator.Evaluate(
                    recurrable.Start,
                    DateUtil.GetSimpleDateTimeData(periodStart),
                    DateUtil.GetSimpleDateTimeData(periodEnd),
                    includeReferenceDateInResults);

                foreach (IPeriod p in periods)
                {
                    // Filter the resulting periods to only contain those 
                    // that occur sometime between startTime and endTime.
                    // NOTE: fixes bug #3007244 - GetOccurences not returning long spanning all-day events 
                    IDateTime endTime = p.EndTime ?? p.StartTime;
                    if (endTime.GreaterThan(periodStart) && p.StartTime.LessThanOrEqual(periodEnd))
                        occurrences.Add(new Occurrence(recurrable, p));
                }

                occurrences.Sort();
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
                        bool?[] row = new bool?[] { false, null, null, true, true, true, true, true, false };

                        // Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
                        if (p.ByMonthDay.Count > 0)
                            row[4] = false;

                        return row;
                    }
                case FrequencyType.Yearly:
                    {
                        bool?[] row = new bool?[] { true, true, true, true, true, true, true, true, false };

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
