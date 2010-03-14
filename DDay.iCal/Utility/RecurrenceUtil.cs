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

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, iCalDateTime dt)
        {
            return GetOccurrences(recurrable, dt.Local.Date, dt.Local.Date.AddDays(1).AddSeconds(-1));
        }

        static public IList<Occurrence> GetOccurrences(IRecurrable recurrable, iCalDateTime startTime, iCalDateTime endTime)
        {
            List<Occurrence> occurrences = new List<Occurrence>();

            IEvaluator evaluator = recurrable.GetService(typeof(IEvaluator)) as IEvaluator;
            if (evaluator != null)
            {
                IList<Period> periods = evaluator.Evaluate(recurrable.Start, startTime, endTime);

                foreach (Period p in periods)
                {
                    // Filter the resulting periods to only contain those between
                    // startTime and endTime.
                    if (p.StartTime >= startTime &&
                        p.StartTime <= endTime)
                        occurrences.Add(new Occurrence(recurrable, p));
                }

                occurrences.Sort();
            }
            return occurrences;
        }
    }
}
