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
#if !SILVERLIGHT
    [Serializable]
#endif
    public partial class iCalTimeZone : 
        CalendarComponent,
        ITimeZone
    {
        #region Static Public Methods

#if !SILVERLIGHT
        static public iCalTimeZone FromLocalTimeZone()
        {
            return FromSystemTimeZone(System.TimeZoneInfo.Local);
        }
        static public iCalTimeZone FromLocalTimeZone(DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return FromSystemTimeZone(System.TimeZoneInfo.Local, earlistDateTimeToSupport, includeHistoricalData);
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
                if( transition.Week != 5 )
                    recurrence.ByDay.Add(new WeekDay(transition.DayOfWeek, transition.Week ));
                else
                    recurrence.ByDay.Add( new WeekDay( transition.DayOfWeek, -1 ) );
            }

            tzi.RecurrenceRules.Add(recurrence);
        }

        public static iCalTimeZone FromSystemTimeZone(System.TimeZoneInfo tzinfo)
        {
            // Support date/times for January 1st of the previous year by default.
            return FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1), false);
        }

        public static iCalTimeZone FromSystemTimeZone(System.TimeZoneInfo tzinfo, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            var adjustmentRules = tzinfo.GetAdjustmentRules();
            var utcOffset = tzinfo.BaseUtcOffset;
            var dday_tz = new iCalTimeZone();
            IDateTime earliest = new iCalDateTime(earlistDateTimeToSupport);

            foreach (var adjustmentRule in adjustmentRules)
            {
                // Only include historical data if asked to do so.  Otherwise,
                // use only the most recent adjustment rule available.
                if (!includeHistoricalData && adjustmentRule.DateEnd < earlistDateTimeToSupport)
                    continue;

                var delta = adjustmentRule.DaylightDelta;
                dday_tz.TZID = tzinfo.Id;

                var dday_tzinfo_standard = new DDay.iCal.iCalTimeZoneInfo();
                dday_tzinfo_standard.Name = "STANDARD";
                dday_tzinfo_standard.TimeZoneName = tzinfo.StandardName;
                dday_tzinfo_standard.Start = new iCalDateTime(adjustmentRule.DateStart);                
                if (dday_tzinfo_standard.Start.LessThan(earliest))
                    dday_tzinfo_standard.Start = dday_tzinfo_standard.Start.AddYears(earliest.Year - dday_tzinfo_standard.Start.Year);
                dday_tzinfo_standard.OffsetFrom = new UTCOffset(utcOffset + delta);
                dday_tzinfo_standard.OffsetTo = new UTCOffset(utcOffset);
                PopulateiCalTimeZoneInfo(dday_tzinfo_standard, adjustmentRule.DaylightTransitionEnd, adjustmentRule.DateStart.Year);
                                
                // Add the "standard" time rule to the time zone
                dday_tz.AddChild(dday_tzinfo_standard);

                if (tzinfo.SupportsDaylightSavingTime)
                {
                    var dday_tzinfo_daylight = new DDay.iCal.iCalTimeZoneInfo();
                    dday_tzinfo_daylight.Name = "DAYLIGHT";
                    dday_tzinfo_daylight.TimeZoneName = tzinfo.DaylightName;
                    dday_tzinfo_daylight.Start = new iCalDateTime(adjustmentRule.DateStart);
                    if (dday_tzinfo_daylight.Start.LessThan(earliest))
                        dday_tzinfo_daylight.Start = dday_tzinfo_daylight.Start.AddYears(earliest.Year - dday_tzinfo_daylight.Start.Year);
                    dday_tzinfo_daylight.OffsetFrom = new UTCOffset(utcOffset);
                    dday_tzinfo_daylight.OffsetTo = new UTCOffset(utcOffset + delta);
                    PopulateiCalTimeZoneInfo(dday_tzinfo_daylight, adjustmentRule.DaylightTransitionStart, adjustmentRule.DateStart.Year);

                    // Add the "daylight" time rule to the time zone
                    dday_tz.AddChild(dday_tzinfo_daylight);
                }                
            }

            // If no time zone information was recorded, at least
            // add a STANDARD time zone element to indicate the
            // base time zone information.
            if (dday_tz.TimeZoneInfos.Count == 0)
            {
                var dday_tzinfo_standard = new DDay.iCal.iCalTimeZoneInfo();
                dday_tzinfo_standard.Name = "STANDARD";
                dday_tzinfo_standard.TimeZoneName = tzinfo.StandardName;
                dday_tzinfo_standard.Start = earliest;                
                dday_tzinfo_standard.OffsetFrom = new UTCOffset(utcOffset);
                dday_tzinfo_standard.OffsetTo = new UTCOffset(utcOffset);

                // Add the "standard" time rule to the time zone
                dday_tz.AddChild(dday_tzinfo_standard);
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
            SetService(m_Evaluator);
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
