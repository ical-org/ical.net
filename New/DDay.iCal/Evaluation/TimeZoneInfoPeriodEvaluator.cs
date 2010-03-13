using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class TimeZoneInfoPeriodEvaluator :
        RecurringComponentPeriodEvaluator
    {
        #region Protected Properties

        protected ITimeZoneInfo TimeZoneInfo
        {
            get { return Component as ITimeZoneInfo; }
            set { Component = value; }
        }

        #endregion

        #region Constructors

        public TimeZoneInfoPeriodEvaluator(ITimeZoneInfo tzi) : base(tzi)
        {
        } 

        #endregion

        #region Overrides

        public override IList<Period> Evaluate(iCalDateTime startTime, iCalDateTime fromTime, iCalDateTime toTime)
        {
            // Add the initial specified date/time for the time zone entry
            IList<Period> periods = base.Evaluate(startTime, fromTime, toTime);
            Period startPeriod = new Period(Component.Start, default(iCalDateTime));
            if (!periods.Contains(startPeriod))
                periods.Insert(0, startPeriod);

            return periods;
        }

        #endregion
    }
}
