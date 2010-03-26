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

        public override IList<IPeriod> Evaluate(IDateTime referenceTime, DateTime periodStart, DateTime periodEnd)
        {
            // Time zones must include an effective start date/time
            // and must provide an evaluator.
            if (TimeZoneInfo != null)
            {
                IList<IPeriod> periods = base.Evaluate(referenceTime, periodStart, periodEnd);
                
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
