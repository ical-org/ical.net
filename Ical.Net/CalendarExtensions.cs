//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;

namespace Ical.Net;

public static class CalendarExtensions
{
    /// <summary>
    /// Calculate the week number according to ISO.8601, as required by RFC 5545.
    /// </summary>
    public static int GetIso8601WeekOfYear(this System.Globalization.Calendar calendar, DateTime time, DayOfWeek firstDayOfWeek)
    {
        // A week is defined as a
        // seven day period, starting on the day of the week defined to be
        // the week start(see WKST). Week number one of the calendar year
        // is the first week that contains at least four (4) days in that
        // calendar year.

        // We add 3 to make sure the test date is in the 'right' year, because
        // otherwise we might end up with week 53 in a year that only has 52.
        var tTest = GetStartOfWeek(time, firstDayOfWeek).AddDays(3);
        var res = calendar.GetWeekOfYear(tTest, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);

        return res;
    }

    /// <summary>
    /// Calculate and return the date that represents the first day of the week the given date is
    /// in, according to the week numbering required by RFC 5545.
    /// </summary>
    private static DateTime GetStartOfWeek(this DateTime t, DayOfWeek firstDayOfWeek)
    {
        var t0 = ((int) firstDayOfWeek) % 7;
        var tn = ((int) t.DayOfWeek) % 7;
        return t.AddDays(-((tn + 7 - t0) % 7));
    }
}
