using System;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Globalization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 5545 VTIMEZONE component.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "iCalTimeZone", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public partial class iCalTimeZone : 
        CalendarComponent,
        ITimeZone
    {
        #region Static Public Methods

#if DATACONTRACT && !SILVERLIGHT
        static public iCalTimeZone FromLocalTimeZone()
        {
            return FromSystemTimeZone(System.TimeZoneInfo.Local);
        }

        static private void PopulateiCalTimeZoneInfo(ITimeZoneInfo tzi, System.TimeZoneInfo.TransitionTime transition, int year)
        {
            Calendar c = CultureInfo.CurrentCulture.Calendar;

            RecurrencePattern recurrence = new RecurrencePattern();
            recurrence.Frequency = FrequencyType.Yearly;
            recurrence.ByMonth.Add(transition.Month);
            recurrence.ByHour.Add(transition.TimeOfDay.Hour);
            recurrence.ByMinute.Add(transition.TimeOfDay.Minute);

            if (transition.IsFixedDateRule)
            {
                recurrence.ByMonthDay.Add(transition.Day);
            }
            else
            {
                recurrence.ByDay.Add(new DaySpecifier(transition.DayOfWeek));
                int daysInMonth = c.GetDaysInMonth(year, transition.Month);
                int offset = (transition.Week * 7) - 7;
                if (offset + 7 > daysInMonth)
                    offset = daysInMonth - 7;

                // Add the possible days of the month this could occur.
                for (int i = 1; i <= 7; i++)
                    recurrence.ByMonthDay.Add(i + offset + (int)transition.DayOfWeek);
            }

            tzi.RecurrenceRules.Add(recurrence);
        }

        public static iCalTimeZone FromSystemTimeZone(System.TimeZoneInfo tzinfo)
        {
            var adjustmentRules = tzinfo.GetAdjustmentRules();
            var utcOffset = tzinfo.BaseUtcOffset;
            var dday_tz = new iCalTimeZone();

            foreach (var adjustmentRule in adjustmentRules)
            {                
                var delta = adjustmentRule.DaylightDelta;
                dday_tz.TZID = tzinfo.Id;                

                var dday_tzinfo_standard = new DDay.iCal.iCalTimeZoneInfo();
                dday_tzinfo_standard.Name = "STANDARD";
                dday_tzinfo_standard.Start = adjustmentRule.DateStart;
                if (dday_tzinfo_standard.Start.Year < 1800)
                    dday_tzinfo_standard.Start = dday_tzinfo_standard.Start.AddYears(1800 - dday_tzinfo_standard.Start.Year);
                dday_tzinfo_standard.OffsetFrom = new UTCOffset(utcOffset + delta);
                dday_tzinfo_standard.OffsetTo = new UTCOffset(utcOffset);
                PopulateiCalTimeZoneInfo(dday_tzinfo_standard, adjustmentRule.DaylightTransitionEnd, adjustmentRule.DateStart.Year);

                // Add the "standard" time rule to the time zone
                dday_tz.AddChild(dday_tzinfo_standard);

                var dday_tzinfo_daylight = new DDay.iCal.iCalTimeZoneInfo();
                dday_tzinfo_daylight.Name = "DAYLIGHT";
                dday_tzinfo_daylight.Start = adjustmentRule.DateStart;
                if (dday_tzinfo_daylight.Start.Year < 1800)
                    dday_tzinfo_daylight.Start = dday_tzinfo_daylight.Start.AddYears(1800 - dday_tzinfo_daylight.Start.Year);
                dday_tzinfo_daylight.OffsetFrom = new UTCOffset(utcOffset);
                dday_tzinfo_daylight.OffsetTo = new UTCOffset(utcOffset + delta);
                PopulateiCalTimeZoneInfo(dday_tzinfo_daylight, adjustmentRule.DaylightTransitionStart, adjustmentRule.DateStart.Year);

                // Add the "daylight" time rule to the time zone
                dday_tz.AddChild(dday_tzinfo_daylight);                
            }

            return dday_tz;
        }
#endif

        #endregion

        #region Private Fields

        private IList<ITimeZoneInfo> m_TimeZoneInfos;
        
        #endregion

        #region Constructors

        public iCalTimeZone()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Name = Components.TIMEZONE;

            m_TimeZoneInfos = new List<ITimeZoneInfo>();
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void AddChild(ICalendarObject child)
        {
            if (child is iCalTimeZoneInfo)
                TimeZoneInfos.Add((iCalTimeZoneInfo)child);

            base.AddChild(child);
        }

        public override void RemoveChild(ICalendarObject child)
        {
            if (child is iCalTimeZoneInfo &&
                TimeZoneInfos.Contains((iCalTimeZoneInfo)child))
                TimeZoneInfos.Remove((iCalTimeZoneInfo)child);

            base.RemoveChild(child);
        }

        #endregion

        #region ITimeZone Members

        virtual public string ID
        {
            get { return Properties.Get<string>("TZID"); }
            set { Properties.Set("TZID", value); }
        }

        virtual public string TZID
        {
            get { return ID; }
            set { ID = value; }
        }

        virtual public iCalDateTime LastModified
        {
            get { return Properties.Get<iCalDateTime>("LAST-MODIFIED"); }
            set { Properties.Set("LAST-MODIFIED", value); }
        }

        virtual public Uri Url
        {
            get { return Properties.Get<Uri>("TZURL"); }
            set { Properties.Set("TZURL", value); }
        }
        
        virtual public Uri TZUrl
        {
            get { return Url; }
            set { Url = value; }
        }

        virtual public IList<ITimeZoneInfo> TimeZoneInfos
        {
            get { return m_TimeZoneInfos; }
            set { m_TimeZoneInfos = value; }
        }

        /// <summary>
        /// Retrieves the iCalTimeZoneInfo object that contains information
        /// about the TimeZone, with the name of the current timezone,
        /// offset from UTC, etc.
        /// </summary>
        /// <param name="dt">The iCalDateTime object for which to retrieve the iCalTimeZoneInfo.</param>
        /// <returns>A TimeZoneInfo object for the specified iCalDateTime</returns>
        virtual public ITimeZoneInfo GetTimeZoneInfo(iCalDateTime dt)
        {
            ITimeZoneInfo tzi = null;

            TimeSpan mostRecent = TimeSpan.MaxValue;
            foreach (ITimeZoneInfo curr in TimeZoneInfos)
            {
                DateTime start = new DateTime(dt.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime end = new DateTime(dt.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                // NOTE: dt.Value should always be a UTC type.
                // This must be true for it to function properly here.
                DateTime dtUTC = dt.Value;

                // Time zones must include an effective start date/time.
                if (curr.Start == null)
                    continue;

                // Make a copy of the current start value
                iCalDateTime currStart = curr.Start;
                if (curr.OffsetTo != null)
                {
                    int mult = curr.OffsetTo.Positive ? -1 : 1;
                    dtUTC = dtUTC.AddHours(curr.OffsetTo.Hours * mult);
                    dtUTC = dtUTC.AddMinutes(curr.OffsetTo.Minutes * mult);
                    dtUTC = dtUTC.AddSeconds(curr.OffsetTo.Seconds * mult);
                    // Offset the current start value to match our offset time...
                    currStart = currStart.AddHours(curr.OffsetTo.Hours * mult);
                    currStart = currStart.AddMinutes(curr.OffsetTo.Minutes * mult);
                    currStart = currStart.AddSeconds(curr.OffsetTo.Seconds * mult);
                }

                // Determine the UTC occurrences of the Time Zone changes
                IEvaluator evaluator = curr.GetService(typeof(IEvaluator)) as IEvaluator;
                IList<Period> periods = evaluator.Evaluate(currStart, start, end);

                // If the date is past the last allowed date, then don't consider it!
                // NOTE: if this time zone ends as another begins, then there can
                // be up to a 1 year period of "unhandled" time unless we add a year
                // to the "Until" date.  For example, one time period "ends" in Oct. 2006
                // (the last occurrence), and the next begins in Apr. 2007.  If we didn't
                // add 1 year to the "Until" time, the 6 month period between Oct. 2006
                // and Apr. 2007 would be unhandled.

                // FIXME: this thinking may be flawed. We should try to find some other way...
                //
                //if (curr.Until != null &&
                //    dtUTC > curr.Until.AddYears(1))
                //    continue;

                foreach (Period p in periods)
                {
                    TimeSpan currentSpan = dtUTC - p.StartTime;
                    if (currentSpan.Ticks >= 0 &&
                        currentSpan.Ticks < mostRecent.Ticks)
                    {
                        mostRecent = currentSpan;
                        tzi = curr;
                    }
                }
            }

            return tzi;
        }

        #endregion
    }
}
