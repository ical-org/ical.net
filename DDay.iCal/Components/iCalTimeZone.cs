using System;
using System.Data;
using System.Collections;
using System.Configuration;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Globalization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents an RFC 2445 VTIMEZONE component.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "iCalTimeZone", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public partial class iCalTimeZone : ComponentBase
    {
        #region Static Public Methods

#if DATACONTRACT && !SILVERLIGHT
        static public iCalTimeZone FromLocalTimeZone()
        {
            return FromSystemTimeZone(System.TimeZoneInfo.Local);
        }

        static private void PopulateiCalTimeZoneInfo(iCalTimeZoneInfo tzi, System.TimeZoneInfo.TransitionTime transition, int year)
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

            tzi.AddRecurrencePattern(recurrence);
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

                var dday_tzinfo_standard = new DDay.iCal.Components.iCalTimeZoneInfo();
                dday_tzinfo_standard.Name = "STANDARD";
                dday_tzinfo_standard.Start = adjustmentRule.DateStart;
                if (dday_tzinfo_standard.Start.Year < 1800)
                    dday_tzinfo_standard.Start = dday_tzinfo_standard.Start.AddYears(1800 - dday_tzinfo_standard.Start.Year);
                dday_tzinfo_standard.TZOffsetFrom = new DDay.iCal.DataTypes.UTC_Offset(utcOffset + delta);
                dday_tzinfo_standard.TZOffsetTo = new DDay.iCal.DataTypes.UTC_Offset(utcOffset);
                PopulateiCalTimeZoneInfo(dday_tzinfo_standard, adjustmentRule.DaylightTransitionEnd, adjustmentRule.DateStart.Year);

                // Add the "standard" time rule to the time zone
                dday_tz.AddChild(dday_tzinfo_standard);

                var dday_tzinfo_daylight = new DDay.iCal.Components.iCalTimeZoneInfo();
                dday_tzinfo_daylight.Name = "DAYLIGHT";
                dday_tzinfo_daylight.Start = adjustmentRule.DateStart;
                if (dday_tzinfo_daylight.Start.Year < 1800)
                    dday_tzinfo_daylight.Start = dday_tzinfo_daylight.Start.AddYears(1800 - dday_tzinfo_daylight.Start.Year);
                dday_tzinfo_daylight.TZOffsetFrom = new DDay.iCal.DataTypes.UTC_Offset(utcOffset);
                dday_tzinfo_daylight.TZOffsetTo = new DDay.iCal.DataTypes.UTC_Offset(utcOffset + delta);
                PopulateiCalTimeZoneInfo(dday_tzinfo_daylight, adjustmentRule.DaylightTransitionStart, adjustmentRule.DateStart.Year);

                // Add the "daylight" time rule to the time zone
                dday_tz.AddChild(dday_tzinfo_daylight);                
            }

            return dday_tz;
        }
#endif

        #endregion

        #region Private Fields

        private TZID m_TZID;
        private iCalDateTime m_Last_Modified;
        private URI m_TZUrl;
        private List<iCalTimeZoneInfo> m_TimeZoneInfos = new List<iCalTimeZoneInfo>();

        #endregion

        #region Public Properties

        [Serialized]
#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public TZID TZID
        {
            get { return m_TZID; }
            set { m_TZID = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public iCalDateTime Last_Modified
        {
            get { return m_Last_Modified; }
            set { m_Last_Modified = value; }
        }

        [Serialized]
#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        public URI TZUrl
        {
            get { return m_TZUrl; }
            set { m_TZUrl = value; }
        }

        [Serialized]
#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        public List<iCalTimeZoneInfo> TimeZoneInfos
        {
            get { return m_TimeZoneInfos; }
            set { m_TimeZoneInfos = value; }
        }

        #endregion

        #region Constructors

        public iCalTimeZone()
            : base()
        {
            this.Name = ComponentBase.TIMEZONE;
        }
        public iCalTimeZone(iCalObject parent)
            : base(parent)
        {
            this.Name = ComponentBase.TIMEZONE;
        }

        #endregion

        #region Overrides

        public override void AddChild(iCalObject child)
        {
            if (child is iCalTimeZoneInfo)
                TimeZoneInfos.Add((iCalTimeZoneInfo)child);

            base.AddChild(child);
        }

        public override void RemoveChild(iCalObject child)
        {
            if (child is iCalTimeZoneInfo &&
                TimeZoneInfos.Contains((iCalTimeZoneInfo)child))
                TimeZoneInfos.Remove((iCalTimeZoneInfo)child);

            base.RemoveChild(child);
        }

        /// <summary>
        /// Returns a typed copy of the TimeZone object.
        /// </summary>
        /// <returns>A typed copy of the TimeZone object</returns>
        public new iCalTimeZone Copy()
        {
            return (iCalTimeZone)base.Copy();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the iCalTimeZoneInfo object that contains information
        /// about the TimeZone, with the name of the current timezone,
        /// offset from UTC, etc.
        /// </summary>
        /// <param name="dt">The iCalDateTime object for which to retrieve the iCalTimeZoneInfo.</param>
        /// <returns>A TimeZoneInfo object for the specified iCalDateTime</returns>
        public iCalTimeZoneInfo GetTimeZoneInfo(iCalDateTime dt)
        {
            iCalTimeZoneInfo tzi = null;

            TimeSpan mostRecent = TimeSpan.MaxValue;
            foreach (iCalTimeZoneInfo curr in TimeZoneInfos)
            {
                DateTime Start = new DateTime(dt.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime End = new DateTime(dt.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                DateTime dtUTC = dt.Value;
                dtUTC = DateTime.SpecifyKind(dtUTC, DateTimeKind.Utc);

                // Time zones must include an effective start date/time.
                if (curr.Start == null)
                    continue;

                // Make a copy of the current start value
                iCalDateTime currStart = curr.Start.Copy();
                if (curr.TZOffsetTo != null)
                {
                    int mult = curr.TZOffsetTo.Positive ? -1 : 1;
                    dtUTC = dtUTC.AddHours(curr.TZOffsetTo.Hours * mult);
                    dtUTC = dtUTC.AddMinutes(curr.TZOffsetTo.Minutes * mult);
                    dtUTC = dtUTC.AddSeconds(curr.TZOffsetTo.Seconds * mult);
                    // Offset the current start value to match our offset time...
                    currStart = currStart.AddHours(curr.TZOffsetTo.Hours * mult);
                    currStart = currStart.AddMinutes(curr.TZOffsetTo.Minutes * mult);
                    currStart = currStart.AddSeconds(curr.TZOffsetTo.Seconds * mult);
                }

                // Determine the UTC occurrences of the Time Zone changes                
                if (curr.EvalStart == null ||
                    curr.EvalEnd == null ||
                    dtUTC < curr.EvalStart.Value ||
                    dtUTC > curr.EvalEnd.Value)
                    curr.Evaluate(Start, End);

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

                foreach (Period p in curr.Periods)
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
