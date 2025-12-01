// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class RecurrenceYearlyByMonthOffsetsTests
{
    /// <summary>
    /// Helper: nth weekday in a given month (n > 0 => nth, n < 0 => -nth from end)
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static CalDateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek dow, int n)
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
    private static CalDateTime GetNthWeekdayOfYear(int year, DayOfWeek dow, int n)
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

    [Test, Category("Recurrence")]
    public void Yearly_ByMonth_With_NumericByDay_Offset_Produces_NthWeekdayPerMonth()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2026, 6, 1, 9, 0, 0),
            Duration = Duration.FromHours(1)
        };

        // 2nd Monday of each BYMONTH
        evt.RecurrenceRules.Add(new RecurrencePattern("FREQ=YEARLY;BYMONTH=6,9;BYDAY=2MO;COUNT=4"));

        var cal = new Calendar();
        cal.Events.Add(evt);

        var occ = cal.GetOccurrences(new CalDateTime(2026, 1, 1)).Take(4).Select(o => o.Period.StartTime).ToList();

        var expected =
        new[]
        {
            GetNthWeekdayOfMonth(2026, 6, DayOfWeek.Monday, 2),
            GetNthWeekdayOfMonth(2026, 9, DayOfWeek.Monday, 2),
            GetNthWeekdayOfMonth(2027, 6, DayOfWeek.Monday, 2),
            GetNthWeekdayOfMonth(2027, 9, DayOfWeek.Monday, 2),
        };

        Assert.That(occ, Is.EqualTo(expected));
    }

    [Test, Category("Recurrence")]
    public void Yearly_Without_ByMonth_NumericByDay_Interpreted_As_NthWeekdayOfYear()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2026, 1, 1, 9, 0, 0),
            Duration = Duration.FromHours(1)
        };

        //  20th Monday of the YEAR
        evt.RecurrenceRules.Add(new RecurrencePattern("FREQ=YEARLY;BYDAY=20MO;COUNT=3"));

        var cal = new Calendar();
        cal.Events.Add(evt);

        var occ = cal.GetOccurrences(new CalDateTime(1997, 1, 1)).Take(3).Select(o => o.Period.StartTime).ToList();

        var expected =
        new[]
        {
            GetNthWeekdayOfYear(2026, DayOfWeek.Monday, 20),
            GetNthWeekdayOfYear(2027, DayOfWeek.Monday, 20),
            GetNthWeekdayOfYear(2028, DayOfWeek.Monday, 20),
        };

        Assert.That(occ, Is.EqualTo(expected));
    }

    [Test, Category("Recurrence")]
    public void Yearly_ByMonth_With_NegativeNumericByDay_Returns_LastWeekdayOfEachMonth()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2026, 6, 1, 9, 0, 0),
            Duration = Duration.FromHours(1)
        };

        // last Sunday of each BYMONTH
        evt.RecurrenceRules.Add(new RecurrencePattern("FREQ=YEARLY;BYMONTH=6,9;BYDAY=-1SU;COUNT=2"));

        var cal = new Calendar();
        cal.Events.Add(evt);

        var occ = cal.GetOccurrences(new CalDateTime(2026, 1, 1)).Take(2).Select(o => o.Period.StartTime).ToList();

        var expected =
        new[]
        {
            GetNthWeekdayOfMonth(2026, 6, DayOfWeek.Sunday, -1),
            GetNthWeekdayOfMonth(2026, 9, DayOfWeek.Sunday, -1),
        };

        Assert.That(occ, Is.EqualTo(expected));
    }
}
