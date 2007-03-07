using System;
using System.Data;
using System.Collections;
using System.Configuration;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents an RFC 2445 VTIMEZONE component.
    /// </summary>
    public partial class TimeZone : ComponentBase
    {
        #region Private Fields

        private TZID m_TZID;        
        private Date_Time m_Last_Modified;        
        private URI m_TZUrl;        
        private ArrayList m_TimeZoneInfos = new ArrayList();        

        #endregion

        #region Public Properties

        [Serialized]
        public TZID TZID
        {
            get { return m_TZID; }
            set { m_TZID = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        public Date_Time Last_Modified
        {
            get { return m_Last_Modified; }
            set { m_Last_Modified = value; }
        }

        [Serialized]
        public URI TZUrl
        {
            get { return m_TZUrl; }
            set { m_TZUrl = value; }
        }

        [Serialized]
        public ArrayList TimeZoneInfos
        {
            get { return m_TimeZoneInfos; }
            set { m_TimeZoneInfos = value; }
        }

        #endregion

        #region Constructors

        public TimeZone() : base()
        {
            this.Name = ComponentBase.TIMEZONE;
        }
        public TimeZone(iCalObject parent) : base(parent)
        {
            this.Name = ComponentBase.TIMEZONE;
        }

        #endregion

        #region Overrides

        public override void AddChild(iCalObject child)
        {
            if (child is TimeZoneInfo)
                TimeZoneInfos.Add(child);

            base.AddChild(child);
        }

        public override void RemoveChild(iCalObject child)
        {
            if (child is TimeZoneInfo &&
                TimeZoneInfos.Contains(child))
                TimeZoneInfos.Remove(child);

            base.RemoveChild(child);
        }

        /// <summary>
        /// Returns a typed copy of the TimeZone object.
        /// </summary>
        /// <returns>A typed copy of the TimeZone object</returns>
        public TimeZone Copy()
        {
            return (TimeZone)base.Copy();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the TimeZoneInfo object that contains information
        /// about the TimeZone, with the name of the current timezone,
        /// offset from UTC, etc.
        /// </summary>
        /// <param name="dt">The Date_Time object for which to retrieve the TimeZoneInfo</param>
        /// <returns>A TimeZoneInfo object for the specified Date_Time</returns>
        public TimeZoneInfo GetTimeZoneInfo(Date_Time dt)
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
                Date_Time currStart = curr.Start.Copy();
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
                if (curr.Until != null &&
                    dtUTC > curr.Until.AddYears(1))
                    continue;

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
