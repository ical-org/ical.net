using System;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics;

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
                recurrence.ByDay.Add(new WeekDay(transition.DayOfWeek));
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
                dday_tzinfo_standard.Start = new iCalDateTime(adjustmentRule.DateStart);                
                if (dday_tzinfo_standard.Start.Year < 1800)
                    dday_tzinfo_standard.Start = dday_tzinfo_standard.Start.AddYears(1800 - dday_tzinfo_standard.Start.Year);
                dday_tzinfo_standard.OffsetFrom = new UTCOffset(utcOffset + delta);
                dday_tzinfo_standard.OffsetTo = new UTCOffset(utcOffset);
                PopulateiCalTimeZoneInfo(dday_tzinfo_standard, adjustmentRule.DaylightTransitionEnd, adjustmentRule.DateStart.Year);

                // Add the "standard" time rule to the time zone
                dday_tz.AddChild(dday_tzinfo_standard);

                var dday_tzinfo_daylight = new DDay.iCal.iCalTimeZoneInfo();
                dday_tzinfo_daylight.Name = "DAYLIGHT";
                dday_tzinfo_daylight.Start = new iCalDateTime(adjustmentRule.DateStart);
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

        TimeZoneEvaluator m_Evaluator;
        IFilteredCalendarObjectList<ITimeZoneInfo> m_TimeZoneInfos;
        
        #endregion

        #region Constructors

        public iCalTimeZone()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Name = Components.TIMEZONE;

            m_Evaluator = new TimeZoneEvaluator(this);
            m_TimeZoneInfos = new FilteredCalendarObjectList<ITimeZoneInfo>(this);
            ChildAdded += new EventHandler<ObjectEventArgs<ICalendarObject>>(iCalTimeZone_ChildAdded);
            ChildRemoved += new EventHandler<ObjectEventArgs<ICalendarObject>>(iCalTimeZone_ChildRemoved);
        }        

        #endregion

        #region Event Handlers

        void iCalTimeZone_ChildRemoved(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            m_Evaluator.Clear();
        }

        void iCalTimeZone_ChildAdded(object sender, ObjectEventArgs<ICalendarObject> e)
        {
            m_Evaluator.Clear();
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override object GetService(Type serviceType)
        {
            if (typeof(IEvaluator).IsAssignableFrom(serviceType))
                return m_Evaluator;
            return null;
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

        virtual public IDateTime LastModified
        {
            get { return Properties.Get<IDateTime>("LAST-MODIFIED"); }
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

        virtual public IFilteredCalendarObjectList<ITimeZoneInfo> TimeZoneInfos
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
        virtual public TimeZoneObservance? GetTimeZoneObservance(IDateTime dt)
        {
            Trace.TraceInformation("Getting time zone for '" + dt + "'...", "Time Zone");
            foreach (ITimeZoneInfo tzi in TimeZoneInfos)
            {
                TimeZoneObservance? observance = tzi.GetObservance(dt);
                if (observance != null && observance.HasValue)
                    return observance;
            }
            return null;
        }

        #endregion
    }
}
