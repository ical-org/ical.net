//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.Extensions;

namespace Ical.Net.Evaluation;

/// <summary>
/// Handles normalizing all BY rules of a recurrence rule.
/// </summary>
internal sealed class ByRuleValues
{
    public static readonly ByRuleValues Empty = new();

    private static readonly Func<int, int> IdentityFunc = e => e;

    private readonly IsoDayOfWeek firstDayOfWeek;
    private DayOfWeekComparer? dayOfWeekComparer;

    private readonly int[] normalMonths;
    private readonly int[] normalHours;
    private readonly int[] normalMinutes;
    private readonly int[] normalSeconds;

    private readonly int[] byWeekNo;
    private Dictionary<int, int[]>? weeks;

    private readonly int[] byYearDay;
    private Dictionary<int, int[]>? yearDays;

    private readonly int[] byMonthDay;
    private Dictionary<int, int[]>? monthDays;

    private readonly int[] bySetPos;
    private Dictionary<int, int[]>? setPos;

    private readonly WeekDayValue[] byDay;
    private IsoDayOfWeek[]? normalDaysOfWeek;
    private WeekDayValue[]? daysOfWeekWithOffset;

    /// <summary>
    /// Constructor for static Empty
    /// </summary>
    private ByRuleValues()
    {
        normalMonths = [];
        normalHours = [];
        normalMinutes = [];
        normalSeconds = [];
        byWeekNo = [];
        byYearDay = [];
        byMonthDay = [];
        byDay = [];
        bySetPos = [];
    }

    private ByRuleValues(RecurrencePattern pattern)
    {
        firstDayOfWeek = pattern.FirstDayOfWeek.ToIsoDayOfWeek();

        normalMonths = SortValues(pattern.ByMonth);
        normalHours = SortValues(pattern.ByHour);
        normalMinutes = SortValues(pattern.ByMinute);
        normalSeconds = SortValues(pattern.BySecond);

        byWeekNo = [.. pattern.ByWeekNo];
        byYearDay = [.. pattern.ByYearDay];
        byMonthDay = [.. pattern.ByMonthDay];
        bySetPos = [.. pattern.BySetPosition];

        // If all values are positive, just sort once
        HasNegativeSetPos = bySetPos.Any(static x => x < 0);

        byDay = [.. pattern.ByDay.Select(static x => new WeekDayValue(x))];
        HasByDayOffsets = byDay.Any(static x => x.Offset != null);
    }

    /// <summary>
    /// Normalizes BY rules for evaluation. BY rule values that change
    /// based on the date are cached for repeated use.
    /// </summary>
    /// <param name="pattern"></param>
    public static ByRuleValues From(RecurrencePattern pattern)
        => pattern.HasByRules() ? new(pattern) : Empty;

    /// <summary>
    /// Normalized BYMONTH values.
    /// </summary>
    public int[] Months => normalMonths;

    /// <summary>
    /// Normalized BYHOUR values.
    /// </summary>
    public int[] Hours => normalHours;

    /// <summary>
    /// Normalized BYMINUTE values.
    /// </summary>
    public int[] Minutes => normalMinutes;

    /// <summary>
    /// Normalized BYSECOND values.
    /// </summary>
    public int[] Seconds => normalSeconds;

    public bool HasByDayOffsets { get; }

    public bool HasNegativeSetPos { get; }

    #region BY rule exists
    public bool ByMonth => normalMonths.Length > 0;

    public bool ByWeekNo => byWeekNo.Length > 0;

    public bool ByYearDay => byYearDay.Length > 0;

    public bool ByMonthDay => byMonthDay.Length > 0;

    public bool ByDay => byDay.Length > 0;

    public bool ByHour => normalHours.Length > 0;

    public bool ByMinute => normalMinutes.Length > 0;

    public bool BySecond => normalSeconds.Length > 0;

    public bool BySetPosition => bySetPos.Length > 0;
    #endregion

    public WeekDayValue[] DaysOfWeek => byDay;

    /// <summary>
    /// Normalized days of the week that do not have an offset.
    /// </summary>
    public IsoDayOfWeek[] DaysOfWeekWithoutOffset
    {
        get
        {
            dayOfWeekComparer ??= new DayOfWeekComparer(firstDayOfWeek);
            normalDaysOfWeek ??= NormalizeDaysWithoutOffset(byDay, dayOfWeekComparer);
            return normalDaysOfWeek;
        }
    }

    /// <summary>
    /// Days of the week with offsets. Values are NOT sorted
    /// and can have both positive and negative offsets.
    /// </summary>
    public WeekDayValue[] DaysOfWeekWithOffset
    {
        get
        {
            // Sorting by offset once here makes evaluated dates
            // more likely to sort faster.
            daysOfWeekWithOffset ??= byDay
                .Where(x => x.Offset != null)
                .OrderBy(x => x.Offset)
                .ToArray();

            return daysOfWeekWithOffset;
        }
    }

    /// <summary>
    /// Get normalized weeks based on the number of weeks in
    /// a week year.
    /// </summary>
    /// <param name="weeksInWeekYear"></param>
    /// <returns></returns>
    public int[] GetWeeks(int weeksInWeekYear)
    {
        weeks ??= [];

        if (!weeks.TryGetValue(weeksInWeekYear, out var normalWeeks))
        {
            normalWeeks = NormalizeWeekNo(byWeekNo, weeksInWeekYear);
            weeks[weeksInWeekYear] = normalWeeks;
        }

        return normalWeeks;
    }

    /// <summary>
    /// Get normalized yeardays based on the year.
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public int[] GetYearDays(int year)
    {
        var daysInYear = CalendarSystem.Iso.GetDaysInYear(year);

        yearDays ??= [];

        if (!yearDays.TryGetValue(daysInYear, out var normalYearDays))
        {
            normalYearDays = NormalizeYearDays(byYearDay, daysInYear);
            yearDays[daysInYear] = normalYearDays;
        }

        return normalYearDays;
    }

    /// <summary>
    /// Get normalized monthdays based on year and month.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    public int[] GetMonthDays(int year, int month)
    {
        var daysInMonth = CalendarSystem.Iso.GetDaysInMonth(year, month);

        monthDays ??= [];

        if (!monthDays.TryGetValue(daysInMonth, out var normalMonthDays))
        {
            normalMonthDays = NormalizeMonthDay(byMonthDay, daysInMonth);
            monthDays[daysInMonth] = normalMonthDays;
        }

        return normalMonthDays;
    }

    /// <summary>
    /// Get normalized set positions based on set count.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public int[] GetSetPositions(int count = 0)
    {
        setPos ??= [];

        if (!setPos.TryGetValue(count, out var normalSetPos))
        {
            normalSetPos = NormalizeSetPos(bySetPos, count);
            setPos[count] = normalSetPos;
        }

        return normalSetPos;
    }

    private static int[] SortValues(List<int> byRule)
    {
        if (byRule.Count == 0)
        {
            // Return static empty array
            return [];
        }

        int[] values = [.. byRule];
        Array.Sort(values);
        return values;
    }

    private static int[] NormalizeWeekNo(int[] byWeekNo, int weeksInWeekYear)
    {
        return byWeekNo
            .Select(weekNo => (weekNo >= 0) ? weekNo : weeksInWeekYear + weekNo + 1)
            .OrderBy(IdentityFunc)
            .OrderedDistinct()
            .ToArray();
    }

    private static int[] NormalizeYearDays(int[] yearDays, int daysInYear)
    {
        return yearDays
            .Select(yearDay => (yearDay > 0) ? yearDay : (daysInYear + yearDay + 1))
            .OrderBy(IdentityFunc)
            .OrderedDistinct()
            .ToArray();
    }

    private static int[] NormalizeMonthDay(int[] byMonthDay, int daysInMonth)
    {
        return byMonthDay
            .Select(monthDay => (monthDay > 0) ? monthDay : (daysInMonth + monthDay + 1))
            .Where(day => day > 0 && day <= daysInMonth)
            .OrderBy(IdentityFunc)
            .OrderedDistinct()
            .ToArray();
    }

    private static IsoDayOfWeek[] NormalizeDaysWithoutOffset(WeekDayValue[] byDay, DayOfWeekComparer dayOfWeekComparer)
    {
        var normalizedDays = byDay
            .Where(x => x.Offset == null)
            .Select(x => x.DayOfWeek)
            .ToArray();

        Array.Sort(normalizedDays, dayOfWeekComparer);

        return normalizedDays;
    }

    private static int[] NormalizeSetPos(int[] bySetPos, int count)
    {
        if (count == 0)
        {
            return bySetPos.OrderBy(IdentityFunc).Distinct().ToArray();
        }

        return bySetPos
            .Select(setPos => (setPos < 0) ? setPos + count + 1 : setPos)
            .Where(setPos => 0 < setPos && setPos <= count)
            .OrderBy(IdentityFunc)
            .OrderedDistinct()
            .ToArray();
    }

    /// <summary>
    /// Compares day of week according to the
    /// specified first day of the week.
    /// </summary>
    /// <param name="firstDayOfWeek"></param>
    private sealed class DayOfWeekComparer(IsoDayOfWeek firstDayOfWeek) : IComparer<IsoDayOfWeek>
    {
        private const int max = (int)IsoDayOfWeek.Sunday;

        public int Compare(IsoDayOfWeek x, IsoDayOfWeek y)
            => DayValue(x).CompareTo(DayValue(y));

        private int DayValue(IsoDayOfWeek value)
            => (value + max - firstDayOfWeek) % max;
    }
}
