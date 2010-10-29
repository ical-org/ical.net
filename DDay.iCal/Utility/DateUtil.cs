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
            IDateTime copy = dt2.Copy<IDateTime>();
            copy.AssociateWith(dt1);

            // If the dt1 time does not occur in the same time zone as the
            // dt2 time, then let's convert it so they can be used in the
            // same context (i.e. evaluation).
            if (dt1.TZID != null)
            {
                if (!string.Equals(dt1.TZID, copy.TZID))
                    return (dt1.TimeZoneObservance != null) ? copy.ToTimeZone(dt1.TimeZoneObservance.Value) : copy.ToTimeZone(dt1.TZID);
                else return copy;
            }
            else if (dt1.IsUniversalTime)
            {
                // The first date/time is in UTC time, convert!
                return new iCalDateTime(copy.UTC);
            }
            else
            {
                // The first date/time is in local time, convert!
                return new iCalDateTime(copy.Local);
            }
        }

        public static DateTime AddWeeks(System.Globalization.Calendar calendar, DateTime dt, int interval, DayOfWeek firstDayOfWeek)
        {
            // How the week increments depends on the WKST indicated (defaults to Monday)
            // So, basically, we determine the week of year using the necessary rules,
            // and we increment the day until the week number matches our "goal" week number.
            // So, if the current week number is 36, and our interval is 2, then our goal
            // week number is 38.
            // NOTE: fixes WeeklyUntilWkst2() eval.
            int current = calendar.GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek),
                lastLastYear = calendar.GetWeekOfYear(new DateTime(dt.Year - 1, 12, 31, 0, 0, 0, DateTimeKind.Local), System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek),
                last = calendar.GetWeekOfYear(new DateTime(dt.Year, 12, 31, 0, 0, 0, DateTimeKind.Local), System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek),
                goal = current + interval;

            // If the goal week is greater than the last week of the year, wrap it!
            if (goal > last)
                goal = goal - last;
            else if (goal <= 0)
                goal = lastLastYear + goal;

            int i = interval > 0 ? 7 : -7;
            while (current != goal)
            {
                dt = dt.AddDays(i);
                current = calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
            }
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
    }
}
