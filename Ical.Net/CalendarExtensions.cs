//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;

namespace Ical.Net
{
    public static class CalendarExtensions
    {
        /// <summary>
        /// https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/
        /// </summary>
        public static int GetIso8601WeekOfYear(this System.Globalization.Calendar calendar, DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            var day = calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return calendar.GetWeekOfYear(time, rule, firstDayOfWeek);
        }
    }
}
