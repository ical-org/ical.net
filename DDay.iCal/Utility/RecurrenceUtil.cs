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

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, IDateTime startTime, IDateTime endTime)
        {
            List<Occurrence> occurrences = new List<Occurrence>();

            IEvaluator evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator != null)
            {
                IList<IPeriod> periods = evaluator.Evaluate(
                    recurrable.Start,
                    DateUtil.GetSimpleDateTimeData(recurrable.Start),
                    startTime.UTC, 
                    endTime.UTC);

                foreach (IPeriod p in periods)
                {
                    // Filter the resulting periods to only contain those between
                    // startTime and endTime.
                    if (p.StartTime.GreaterThanOrEqual(startTime) &&
                        p.StartTime.LessThanOrEqual(endTime))
                        occurrences.Add(new Occurrence(recurrable, p));
                }

                occurrences.Sort();
            }
            return occurrences;
        }
    }
}
