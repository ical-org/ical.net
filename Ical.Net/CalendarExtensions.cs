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

    /// <summary>
    /// Calculate the year, the given date's week belongs to according to ISO 8601, as required by RFC 5545.
    /// </summary>
    /// <remarks>
    /// A date's nominal year may be different from the year, the week belongs to that the date is in.
    /// I.e. the first and last week of the year may belong to a different year than the date's year.
    /// E.g. for `2019-12-31` with first day of the week being Monday, the method will return 2020,
    /// because the week that contains `2019-12-31` is the first week of 2020.
    /// </remarks>
    public static int GetIso8601YearOfWeek(this System.Globalization.Calendar calendar, DateTime time, DayOfWeek firstDayOfWeek)
    {
        var year = time.Year;
        if ((time.Month >= 12) && (calendar.GetIso8601WeekOfYear(time, firstDayOfWeek) == 1))
            year++;
        else if ((time.Month == 1) && (calendar.GetIso8601WeekOfYear(time, firstDayOfWeek) >= 52))
            year--;

        return year;
    }

    /// <summary>
    /// Calculate the number of weeks in the given year according to ISO 8601, as required by RFC 5545.
    /// </summary>
    public static int GetIso8601WeeksInYear(this System.Globalization.Calendar calendar, int year, DayOfWeek firstDayOfWeek)
    {
        // The last week of the year is the week that contains the 4th-last day of the year (which is the 28th of December in Gregorian Calendar).
        var testTime = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddDays(-4);
        return calendar.GetIso8601WeekOfYear(testTime, firstDayOfWeek);
    }
}
