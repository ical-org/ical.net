using System;
using System.Diagnostics;
using NodaTime;

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
            var copy = dt2.Copy<IDateTime>();
            copy.AssociateWith(dt1);

            // If the dt1 time does not occur in the same time zone as the
            // dt2 time, then let's convert it so they can be used in the
            // same context (i.e. evaluation).
            if (dt1.TzId != null)
            {
                if (!string.Equals(dt1.TzId, copy.TzId))
                    return (dt1.TimeZoneObservance != null) ? copy.ToTimeZone(dt1.TimeZoneObservance.Value) : copy.ToTimeZone(dt1.TzId);
                else return copy;
            }
            else if (dt1.IsUniversalTime)
            {
                // The first date/time is in UTC time, convert!
                return new iCalDateTime(copy.AsUtc);
            }
            else
            {
                // The first date/time is in local time, convert!
                return new iCalDateTime(copy.AsSystemLocal);
            }
        }

        public static DateTime AddWeeks(DateTime dt, int interval, DayOfWeek firstDayOfWeek)
        {
            // NOTE: fixes WeeklyUntilWkst2() eval.
            // NOTE: simplified the execution of this - fixes bug #3119920 - missing weekly occurences also
            dt = dt.AddDays(interval * 7);
            while (dt.DayOfWeek != firstDayOfWeek)
                dt = dt.AddDays(-1);

            return dt;
        }

        public static DateTime FirstDayOfWeek(DateTime dt, DayOfWeek firstDayOfWeek, out int offset)
        {
            offset = 0;
            while (dt.DayOfWeek != firstDayOfWeek)
            {
                dt = dt.AddDays(-1);
                offset++;
            }
            return dt;
        }

        public static DateTimeZone GetZone(string tzId)
        {
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            zone = DateTimeZoneProviders.Bcl.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            zone = DateTimeZoneProviders.Serialization.GetZoneOrNull(tzId);
            if (zone != null)
            {
                return zone;
            }

            var newTzId = tzId.Replace("/", "-");
            zone = DateTimeZoneProviders.Serialization.GetZoneOrNull(newTzId);
            if (zone != null)
            {
                return zone;
            }

            throw new ArgumentException($"{tzId} is not a valid time zone");
        }

        public static ZonedDateTime AddYears(ZonedDateTime zonedDateTime, int years)
        {
            var futureDate = zonedDateTime.Date.PlusYears(years);
            var futureLocalDateTime = new LocalDateTime(futureDate.Year, futureDate.Month, futureDate.Day, zonedDateTime.Hour, zonedDateTime.Minute,
                zonedDateTime.Second);
            var zonedFutureDate = new ZonedDateTime(futureLocalDateTime, zonedDateTime.Zone, zonedDateTime.Offset);
            return zonedFutureDate;
        }

        public static ZonedDateTime AddMonths(ZonedDateTime zonedDateTime, int months)
        {
            var futureDate = zonedDateTime.Date.PlusMonths(months);
            var futureLocalDateTime = new LocalDateTime(futureDate.Year, futureDate.Month, futureDate.Day, zonedDateTime.Hour, zonedDateTime.Minute,
                zonedDateTime.Second);
            var zonedFutureDate = new ZonedDateTime(futureLocalDateTime, zonedDateTime.Zone, zonedDateTime.Offset);
            return zonedFutureDate;
        }
    }
}
