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

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime dt)
        {
            return GetOccurrences(
                recurrable, 
                new iCalDateTime(dt.Local.Date), 
                new iCalDateTime(dt.Local.Date.AddDays(1).AddSeconds(-1)));
        }

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime periodStart, IDateTime periodEnd)
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
                    DateUtil.GetSimpleDateTimeData(periodEnd));

                foreach (IPeriod p in periods)
                {
                    // Filter the resulting periods to only contain those between
                    // startTime and endTime.
                    if (p.StartTime.GreaterThanOrEqual(periodStart) &&
                        p.StartTime.LessThanOrEqual(periodEnd))
                        occurrences.Add(new Occurrence(recurrable, p));
                }

                occurrences.Sort();
            }
            return occurrences;
        }
    }
}
