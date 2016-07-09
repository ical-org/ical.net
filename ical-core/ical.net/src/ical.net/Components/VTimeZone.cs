using System;
using Ical.Net.Interfaces.Components;

namespace Ical.Net
{
    /// <summary>
    /// Represents an RFC 5545 VTIMEZONE component.
    /// </summary>
    public class VTimeZone : CalendarComponent, ITimeZone
    {
        public static VTimeZone FromLocalTimeZone()
        {
            return FromSystemTimeZone(TimeZoneInfo.Local);
        }

        public static VTimeZone FromLocalTimeZone(DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return FromSystemTimeZone(TimeZoneInfo.Local, earlistDateTimeToSupport, includeHistoricalData);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        {
            // Support date/times for January 1st of the previous year by default.
            return FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1), false);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return new VTimeZone {TzId = tzinfo.Id};
        }

        public VTimeZone()
        {
            Name = Components.Timezone;
        }

        public virtual string Id
        {
            get { return Properties.Get<string>("TZID"); }
            set { Properties.Set("TZID", value); }
        }

        public virtual string TzId
        {
            get { return Id; }
            set { Id = value; }
        }

        public virtual Uri Url
        {
            get { return Properties.Get<Uri>("TZURL"); }
            set { Properties.Set("TZURL", value); }
        }
    }
}