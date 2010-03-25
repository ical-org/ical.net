using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DDay.iCal
{
    public class TimeZoneInfoEvaluator :
        RecurringEvaluator
    {
        #region Protected Properties

        protected ITimeZoneInfo TimeZoneInfo
        {
            get { return Recurrable as ITimeZoneInfo; }
            set { Recurrable = value; }
        }

        #endregion

        #region Constructors

        public TimeZoneInfoEvaluator(ITimeZoneInfo tzi) : base(tzi)
        {
        }

        #endregion       
 
        #region Overrides

        public override IList<IPeriod> Evaluate(IDateTime referenceTime, DateTime startTime, DateTime fromTime, DateTime toTime)
        {
            // Time zones must include an effective start date/time
            // and must provide an evaluator.
            if (TimeZoneInfo != null)
            {
                // Normalize the date/time values because we don't want
                // to be doing time zone lookups *while* we're evaluating
                // time zone occurrences.  This could cause an infinite loop.

                IUTCOffset offset = TimeZoneInfo.OffsetTo;
                Debug.Assert(offset != null);
                if (string.Equals(referenceTime.TZID, TimeZoneInfo.TZID))
                {
                    startTime = offset.Offset(startTime);
                    fromTime = offset.Offset(fromTime);
                    toTime = offset.Offset(toTime);
                }

                IList<IPeriod> periods = base.Evaluate(referenceTime, startTime, fromTime, toTime);
                
                // Add the initial specified date/time for the time zone entry
                IPeriod startPeriod = new Period(Recurrable.Start, null);
                if (!periods.Contains(startPeriod))
                    periods.Insert(0, startPeriod);

                return periods;
            }

            return new List<IPeriod>();            
        }

        #endregion
    }
}
