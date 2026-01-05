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

    private static readonly Func<int, int> _identityFunc = e => e;

    private readonly IsoDayOfWeek _firstDayOfWeek;
    private DayOfWeekComparer? _dayOfWeekComparer;

    private readonly int[] _normalMonths;
    private readonly int[] _normalHours;
    private readonly int[] _normalMinutes;
    private readonly int[] _normalSeconds;

    private readonly int[] _byWeekNo;
    private Dictionary<int, int[]>? _weeks;

    private readonly int[] _byYearDay;
    private Dictionary<int, int[]>? _yearDays;

    private readonly int[] _byMonthDay;
    private Dictionary<int, int[]>? _monthDays;

    private readonly int[] _bySetPos;
    private Dictionary<int, int[]>? _setPos;

    private readonly WeekDayValue[] _byDay;
    private IsoDayOfWeek[]? _normalDaysOfWeek;
    private WeekDayValue[]? _daysOfWeekWithOffset;

    /// <summary>
    /// Constructor for static Empty
    /// </summary>
    private ByRuleValues()
    {
        _normalMonths = [];
        _normalHours = [];
        _normalMinutes = [];
        _normalSeconds = [];
        _byWeekNo = [];
        _byYearDay = [];
        _byMonthDay = [];
        _byDay = [];
        _bySetPos = [];
    }

    private ByRuleValues(RecurrencePattern pattern)
    {
        _firstDayOfWeek = pattern.FirstDayOfWeek.ToIsoDayOfWeek();

        _normalMonths = SortValues(pattern.ByMonth);
        _normalHours = SortValues(pattern.ByHour);
        _normalMinutes = SortValues(pattern.ByMinute);
        _normalSeconds = SortValues(pattern.BySecond);

        _byWeekNo = [.. pattern.ByWeekNo];
        _byYearDay = [.. pattern.ByYearDay];
        _byMonthDay = [.. pattern.ByMonthDay];
        _bySetPos = [.. pattern.BySetPosition];

        _byDay = [.. pattern.ByDay.Select(static x => new WeekDayValue(x))];

        HasByDayOffsets = _byDay.Any(static x => x.Offset != null);
        HasNegativeSetPos = _bySetPos.Any(static x => x < 0);
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
    public int[] Months => _normalMonths;

    /// <summary>
    /// Normalized BYHOUR values.
    /// </summary>
    public int[] Hours => _normalHours;

    /// <summary>
    /// Normalized BYMINUTE values.
    /// </summary>
    public int[] Minutes => _normalMinutes;

    /// <summary>
    /// Normalized BYSECOND values.
    /// </summary>
    public int[] Seconds => _normalSeconds;

    public bool HasByDayOffsets { get; }

    public bool HasNegativeSetPos { get; }

    #region BY rule exists
    public bool ByMonth => _normalMonths.Length > 0;

    public bool ByWeekNo => _byWeekNo.Length > 0;

    public bool ByYearDay => _byYearDay.Length > 0;

    public bool ByMonthDay => _byMonthDay.Length > 0;

    public bool ByDay => _byDay.Length > 0;

    public bool ByHour => _normalHours.Length > 0;

    public bool ByMinute => _normalMinutes.Length > 0;

    public bool BySecond => _normalSeconds.Length > 0;

    public bool BySetPosition => _bySetPos.Length > 0;
    #endregion

    public WeekDayValue[] DaysOfWeek => _byDay;

    /// <summary>
    /// Normalized days of the week that do not have an offset.
    /// </summary>
    public IsoDayOfWeek[] DaysOfWeekWithoutOffset
    {
        get
        {
            _dayOfWeekComparer ??= new DayOfWeekComparer(_firstDayOfWeek);
            _normalDaysOfWeek ??= NormalizeDaysWithoutOffset(_byDay, _dayOfWeekComparer);
            return _normalDaysOfWeek;
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
            _daysOfWeekWithOffset ??= _byDay
                .Where(x => x.Offset != null)
                .OrderBy(x => x.Offset)
                .ToArray();

            return _daysOfWeekWithOffset;
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
        _weeks ??= [];

        if (!_weeks.TryGetValue(weeksInWeekYear, out var normalWeeks))
        {
            normalWeeks = NormalizeWeekNo(_byWeekNo, weeksInWeekYear);
            _weeks[weeksInWeekYear] = normalWeeks;
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

        _yearDays ??= [];

        if (!_yearDays.TryGetValue(daysInYear, out var normalYearDays))
        {
            normalYearDays = NormalizeYearDays(_byYearDay, daysInYear);
            _yearDays[daysInYear] = normalYearDays;
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

        _monthDays ??= [];

        if (!_monthDays.TryGetValue(daysInMonth, out var normalMonthDays))
        {
            normalMonthDays = NormalizeMonthDay(_byMonthDay, daysInMonth);
            _monthDays[daysInMonth] = normalMonthDays;
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
        _setPos ??= [];

        if (!_setPos.TryGetValue(count, out var normalSetPos))
        {
            normalSetPos = NormalizeSetPos(_bySetPos, count);
            _setPos[count] = normalSetPos;
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
            .OrderBy(_identityFunc)
            .OrderedDistinct()
            .ToArray();
    }

    private static int[] NormalizeYearDays(int[] yearDays, int daysInYear)
    {
        return yearDays
            .Select(yearDay => (yearDay > 0) ? yearDay : (daysInYear + yearDay + 1))
            .OrderBy(_identityFunc)
            .OrderedDistinct()
            .ToArray();
    }

    private static int[] NormalizeMonthDay(int[] byMonthDay, int daysInMonth)
    {
        return byMonthDay
            .Select(monthDay => (monthDay > 0) ? monthDay : (daysInMonth + monthDay + 1))
            .Where(day => day > 0 && day <= daysInMonth)
            .OrderBy(_identityFunc)
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
            return bySetPos.OrderBy(_identityFunc).Distinct().ToArray();
        }

        return bySetPos
            .Select(setPos => (setPos < 0) ? setPos + count + 1 : setPos)
            .Where(setPos => 0 < setPos && setPos <= count)
            .OrderBy(_identityFunc)
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
        private const int _max = (int)IsoDayOfWeek.Sunday;

        public int Compare(IsoDayOfWeek x, IsoDayOfWeek y)
            => DayValue(x).CompareTo(DayValue(y));

        private int DayValue(IsoDayOfWeek value)
            => (value + _max - firstDayOfWeek) % _max;
    }
}
