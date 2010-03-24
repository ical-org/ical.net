using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DDay.iCal
{
    public class TimeZoneEvaluator :
        Evaluator
    {
        #region Private Fields

        private List<Occurrence> m_Occurrences;        

        #endregion

        #region Protected Properties

        protected ITimeZone TimeZone { get; set; }

        #endregion

        #region Public Properties

        virtual public List<Occurrence> Occurrences
        {
            get { return m_Occurrences; }
            set { m_Occurrences = value; }
        }

        #endregion

        #region Constructors

        public TimeZoneEvaluator(ITimeZone tz)
        {
            TimeZone = tz;
            m_Occurrences = new List<Occurrence>();
        }

        #endregion

        #region Private Methods

        void ProcessOccurrences()
        {
            // Sort the occurrences by start time
            m_Occurrences.Sort(
                delegate(Occurrence o1, Occurrence o2)
                {
                    if (o1.Period == null || o1.Period.StartTime == null)
                        return -1;
                    else if (o2.Period == null || o2.Period.StartTime == null)
                        return 1;
                    else return o1.Period.StartTime.CompareTo(o2.Period.StartTime);
                }
            );

            for (int i = 0; i < m_Occurrences.Count; i++)
            {
                Occurrence curr = m_Occurrences[i];
                Occurrence? next = i < m_Occurrences.Count - 1 ? (Occurrence?)m_Occurrences[i + 1] : null;

                // Determine end times for our periods
                if (curr.Period.EndTime == null)
                {
                    if (next != null &&
                        next.HasValue)
                    {
                        curr.Period.EndTime = next.Value.Period.StartTime.AddTicks(-1);
                    }
                    else
                    {
                        curr.Period.EndTime = new iCalDateTime(DateTime.MaxValue);
                    }
                }
            }
        }

        #endregion

        #region Overrides

        public override void Clear()
        {
            base.Clear();
            m_Occurrences.Clear();
        }

        public override IList<IPeriod> Evaluate(IDateTime startTime, IDateTime fromTime, IDateTime toTime)
        {
            List<ITimeZoneInfo> infos = new List<ITimeZoneInfo>(TimeZone.TimeZoneInfos);

            bool evaluated = false;
            for (int i = 0; i < infos.Count; i++)
            {
                ITimeZoneInfo curr = infos[i];
                ITimeZoneInfo next = i < infos.Count - 1 ? infos[i + 1] : null;

                IEvaluator evaluator = curr.GetService(typeof(IEvaluator)) as IEvaluator;
                Debug.Assert(curr.Start != null, "TimeZoneInfo.Start must not be null.");
                Debug.Assert(evaluator != null, "TimeZoneInfo.GetService(typeof(IEvaluator)) must not be null.");

                // Time zones must include an effective start date/time
                // and must provide an evaluator.
                if (curr.Start != null && evaluator != null)
                {
                    // Set the start bounds of our evaluation.
                    if (EvaluationStartBounds == null || curr.Start.LessThan(EvaluationStartBounds))
                        EvaluationStartBounds = curr.Start;

                    DateTime normalizedDt = curr.OffsetTo.Offset(startTime.Value);
                    IDateTime newDt = new iCalDateTime(normalizedDt);

                    if (EvaluationEndBounds == null || EvaluationEndBounds.LessThan(newDt))
                    {
                        // Determine the UTC occurrences of the Time Zone observances
                        IList<IPeriod> periods = evaluator.Evaluate(
                            curr.Start.Copy<IDateTime>(),
                            curr.Start.Copy<IDateTime>(),
                            newDt.Copy<IDateTime>());

                        foreach (IPeriod period in periods)
                        {
                            Periods.Add(period);

                            Occurrence o = new Occurrence(curr, period);
                            if (!m_Occurrences.Contains(o))
                                m_Occurrences.Add(o);
                        }

                        EvaluationEndBounds = newDt;
                        evaluated = true;
                    }
                }
            }

            if (evaluated)
                ProcessOccurrences();

            return Periods;
        }

        #endregion
    }
}
