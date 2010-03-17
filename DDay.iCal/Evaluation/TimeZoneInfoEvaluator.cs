using System;
using System.Collections.Generic;
using System.Text;

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

        public override IList<IPeriod> Evaluate(IDateTime startTime, IDateTime fromTime, IDateTime toTime)
        {
            // Add the initial specified date/time for the time zone entry
            IList<IPeriod> periods = base.Evaluate(startTime, fromTime, toTime);
            IPeriod startPeriod = new Period(Recurrable.Start, null);
            if (!periods.Contains(startPeriod))
                periods.Insert(0, startPeriod);

            return periods;
        }

        #endregion
    }
}
