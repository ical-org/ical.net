using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Tests.TestHelpers;

public static class CalCalc
{
    /// <summary>
    /// Helper: nth weekday in a given month (n > 0 => nth, n < 0 => -nth from end)
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static CalDateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek dow, int n)
    {
        var firstOfMonth = new CalDateTime(year, month, 1, 9, 0, 0);
        var d = firstOfMonth;
        while (d.DayOfWeek != dow && d.Month == month)
        {
            d = d.AddDays(1);
        }

        var list = new List<CalDateTime>();
        while (d.Month == month)
        {
            list.Add(d);
            d = d.AddDays(7);
        }

        if (n > 0)
        {
            return (n <= list.Count) ? list[n - 1] : throw new InvalidOperationException("Offset out of range");
        }

        var idx = list.Count + n; // n negative
        return (idx >= 0 && idx < list.Count) ? list[idx] : throw new InvalidOperationException("Offset out of range");
    }

    /// <summary>
    /// Helper: nth weekday of the year (n > 0)
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static CalDateTime GetNthWeekdayOfYear(int year, DayOfWeek dow, int n)
    {
        var d = new CalDateTime(year, 1, 1, 9, 0, 0);
        while (d.DayOfWeek != dow)
        {
            d = d.AddDays(1);
        }

        var result = d.AddDays((n - 1) * 7);
        if (result.Year != year)
            throw new InvalidOperationException("Offset out of range for year");
        return result;
    }
}