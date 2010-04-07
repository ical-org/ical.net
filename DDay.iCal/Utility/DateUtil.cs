using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace DDay.iCal
{
    public class DateUtil
    {
        static private System.Globalization.Calendar _Calendar;

        static DateUtil()
        {
            _Calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        }

        public static IDateTime StartOfDay(IDateTime dt)
        {
            return dt.
                AddHours(-dt.Hour).
                AddMinutes(-dt.Minute).
                AddSeconds(-dt.Second);
        }

        public static IDateTime EndOfDay(IDateTime dt)
        {
            return StartOfDay(dt).AddDays(1).AddTicks(-1);
        }     

        public static DateTime GetSimpleDateTimeData(IDateTime dt)
        {
            return DateTime.SpecifyKind(dt.Value, dt.IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local);
        }

        public static DateTime SimpleDateTimeToMatch(IDateTime dt, IDateTime toMatch)
        {
            if (toMatch.IsUniversalTime && dt.IsUniversalTime)
                return dt.Value;
            else if (toMatch.IsUniversalTime)
                return dt.Value.ToUniversalTime();
            else if (dt.IsUniversalTime)
                return dt.Value.ToLocalTime();
            else
                return dt.Value;
        }

        public static IDateTime MatchTimeZone(IDateTime dt1, IDateTime dt2)
        {
            Debug.Assert(dt1 != null && dt2 != null);

            // Associate the date/time with the first.
            dt2.AssociateWith(dt1);

            // If the dt1 time does not occur in the same time zone as the
            // dt2 time, then let's convert it so they can be used in the
            // same context (i.e. evaluation).
            if (dt1.TZID != null)
            {
                ITimeZoneInfo tzi = dt1.TimeZoneObservance != null ?
                    dt1.TimeZoneObservance.Value.TimeZoneInfo :
                    null;
                if (!string.Equals(dt1.TZID, dt2.TZID))
                    return (tzi != null) ? dt2.ToTimeZone(tzi) : dt2.ToTimeZone(dt1.TZID);
                else return dt2.Copy<IDateTime>();
            }
            else if (dt1.IsUniversalTime)
            {
                // The first date/time is in UTC time, convert!
                return new iCalDateTime(dt2.UTC);
            }
            else
            {
                // The first date/time is in local time, convert!
                return new iCalDateTime(dt2.Local);
            }
        }
    }
}
