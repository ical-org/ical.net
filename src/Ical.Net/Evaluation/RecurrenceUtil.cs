using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation
{
    internal static class RecurrenceUtil
    {
        public static void ClearEvaluation(this IRecurrable recurrable)
        {
            var evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            evaluator?.Clear();
        }

        public static HashSet<Occurrence> GetOccurrences(this IRecurrable recurrable, IDateTime dt, bool includeReferenceDateInResults)
            => GetOccurrences(recurrable, new CalDateTime(dt.AsSystemLocal.Date), new CalDateTime(dt.AsSystemLocal.Date.AddDays(1).AddSeconds(-1)), includeReferenceDateInResults);

        public static HashSet<Occurrence> GetOccurrences(this IRecurrable recurrable, IDateTime periodStart, IDateTime periodEnd, bool includeReferenceDateInResults)
        {
            if (recurrable.Start == null || !(recurrable.GetService(typeof(IEvaluator)) is IEvaluator evaluator))
            {
                return new HashSet<Occurrence>();
            }

            // Ensure the start time is associated with the object being queried
            var start = recurrable.Start;
            start.AssociatedObject = recurrable as ICalendarObject;

            // Change the time zone of periodStart/periodEnd as needed 
            // so they can be used during the evaluation process.

            periodStart.TzId = start.TzId;
            periodEnd.TzId = start.TzId;

            var periods = evaluator.Evaluate(start, periodStart.GetSimpleDateTimeData(), periodEnd.GetSimpleDateTimeData(),
                includeReferenceDateInResults);

            var otherOccurrences = from p in periods
                let endTime = p.EndTime ?? p.StartTime
                where endTime.GreaterThan(periodStart) && p.StartTime.LessThanOrEqual(periodEnd)
                select new Occurrence(recurrable, p);

            var occurrences = new HashSet<Occurrence>(otherOccurrences);
            return occurrences;
        }

        /// <summary> See the table in RFC 5545 Section 3.3.10 (Page 43). </summary>
        public static bool?[] GetExpandBehaviorList(this RecurrencePattern p)
            => p.Frequency switch { // Limit if BYMONTHDAY is present; otherwise, special expand for MONTHLY.
                FrequencyType.Monthly => new bool?[] {false, null, null, true, p.ByMonthDay.Count <= 0, true, true, true, false},
                FrequencyType.Minutely => new bool?[] {false, null, false, false, false, false, false, true, false},
                FrequencyType.Hourly => new bool?[] {false, null, false, false, false, false, true, true, false},
                FrequencyType.Daily => new bool?[] {false, null, null, false, false, true, true, true, false},
                FrequencyType.Weekly => new bool?[] {false, null, null, null, true, true, true, true, false},
                FrequencyType.Yearly
                     // Limit if BYYEARDAY or BYMONTHDAY is present; otherwise,
                     // special expand for WEEKLY if BYWEEKNO present; otherwise,
                     // special expand for MONTHLY if BYMONTH present; otherwise,
                     // special expand for YEARLY.
                     => new bool?[] {true, true, true, true, p.ByYearDay.Count <= 0 && p.ByMonthDay.Count <= 0, true, true, true, false},
                _ => new bool?[] {false, null, false, false, false, false, false, false, false}
            };
    }
}