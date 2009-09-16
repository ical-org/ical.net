using System;
using System.Data;
using System.Collections;
using System.Configuration;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents an RFC 2445 VTIMEZONE component.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "iCalTimeZone", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
    [KnownType(typeof(TZID))]
    [KnownType(typeof(iCalDateTime))]
    [KnownType(typeof(URI))]
    [KnownType(typeof(TimeZoneInfo))]
    [KnownType(typeof(List<TimeZoneInfo>))]
#else
    [Serializable]
#endif
    public partial class iCalTimeZone : ComponentBase
    {
        #region Static Public Methods

#if !SILVERLIGHT
        static public iCalTimeZone FromLocalTimeZone()
        {
            TimeZone timeZone = System.TimeZone.CurrentTimeZone;
            TimeSpan utcOffset = timeZone.GetUtcOffset(DateTime.Now);

            iCalTimeZone tz = new iCalTimeZone();
            tz.TZID = "UTC" + (utcOffset.TotalHours >= 0 ? "+" : "") + utcOffset.Hours.ToString("00") + utcOffset.Minutes.ToString("00");
            
            return tz;
        }
#endif

#if DATACONTRACT
        public static iCalTimeZone FromSystemTimeZone(System.TimeZoneInfo tzinfo)
        {
            var adjustment_rule = tzinfo.GetAdjustmentRules().Last();
            var utc_offset = tzinfo.BaseUtcOffset;
            var delta = adjustment_rule.DaylightDelta;
            var dday_tz = new DDay.iCal.Components.iCalTimeZone();

            dday_tz.TZID = tzinfo.Id;

            var dday_tzinfo_standard = new DDay.iCal.Components.TimeZoneInfo();
            dday_tzinfo_standard.Name = "STANDARD";
            dday_tzinfo_standard.TZOffsetFrom = new DDay.iCal.DataTypes.UTC_Offset(utc_offset + delta);
            dday_tzinfo_standard.TZOffsetTo = new DDay.iCal.DataTypes.UTC_Offset(utc_offset);

            RecurrencePattern standard_recurrence_pattern = new RecurrencePattern();
            var standard_day_specifier = new DaySpecifier(adjustment_rule.DaylightTransitionEnd.DayOfWeek, adjustment_rule.DaylightTransitionEnd.Week);
            standard_recurrence_pattern.ByDay = new List<DaySpecifier>() { standard_day_specifier };
            standard_recurrence_pattern.Frequency = DDay.iCal.DataTypes.FrequencyType.Yearly;
            standard_recurrence_pattern.ByMonth = new List<int>() { adjustment_rule.DaylightTransitionEnd.Month };

            dday_tzinfo_standard.RRule = new RecurrencePattern[1];
            dday_tzinfo_standard.RRule[0] = standard_recurrence_pattern;
            dday_tz.AddChild(dday_tzinfo_standard);

            var dday_tzinfo_daylight = new DDay.iCal.Components.TimeZoneInfo();
            dday_tzinfo_daylight.Name = "DAYLIGHT";
            dday_tzinfo_daylight.TZOffsetFrom = new DDay.iCal.DataTypes.UTC_Offset(utc_offset);
            dday_tzinfo_daylight.TZOffsetTo = new DDay.iCal.DataTypes.UTC_Offset(utc_offset + delta);

            RecurrencePattern daylight_recurrence_pattern = new RecurrencePattern();
            var daylight_day_specifier = new DaySpecifier(adjustment_rule.DaylightTransitionStart.DayOfWeek, adjustment_rule.DaylightTransitionStart.Week);
            daylight_recurrence_pattern.ByDay = new List<DaySpecifier>() { daylight_day_specifier };
            daylight_recurrence_pattern.Frequency = DDay.iCal.DataTypes.FrequencyType.Yearly;
            daylight_recurrence_pattern.ByMonth = new List<int>() { adjustment_rule.DaylightTransitionStart.Month };
            dday_tzinfo_daylight.RRule = new RecurrencePattern[1];
            dday_tzinfo_daylight.RRule[0] = daylight_recurrence_pattern;
            dday_tz.AddChild(dday_tzinfo_daylight);

            return dday_tz;
        }
#endif

        #endregion

        #region Private Fields

        private TZID m_TZID;
        private iCalDateTime m_Last_Modified;
        private URI m_TZUrl;
        private List<TimeZoneInfo> m_TimeZoneInfos = new List<TimeZoneInfo>();

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
        public List<TimeZoneInfo> TimeZoneInfos
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
            if (child is TimeZoneInfo)
                TimeZoneInfos.Add((TimeZoneInfo)child);

            base.AddChild(child);
        }

        public override void RemoveChild(iCalObject child)
        {
            if (child is TimeZoneInfo &&
                TimeZoneInfos.Contains((TimeZoneInfo)child))
                TimeZoneInfos.Remove((TimeZoneInfo)child);

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
        /// Retrieves the TimeZoneInfo object that contains information
        /// about the TimeZone, with the name of the current timezone,
        /// offset from UTC, etc.
        /// </summary>
        /// <param name="dt">The iCalDateTime object for which to retrieve the TimeZoneInfo</param>
        /// <returns>A TimeZoneInfo object for the specified iCalDateTime</returns>
        public TimeZoneInfo GetTimeZoneInfo(iCalDateTime dt)
        {
            TimeZoneInfo tzi = null;

            TimeSpan mostRecent = TimeSpan.MaxValue;
            foreach (TimeZoneInfo curr in TimeZoneInfos)
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
