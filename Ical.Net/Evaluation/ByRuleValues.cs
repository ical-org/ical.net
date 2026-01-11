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

    private readonly int[] _normalPositiveSetPos;

    private readonly NormalValues _byWeekNo;
    private readonly NormalValues _byYearDay;
    private readonly NormalValues _byMonthDay;
    private readonly NormalValues _bySetPos;

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
        _normalPositiveSetPos = [];
        _byDay = [];

        // Must initialize set arrays to []
        _byWeekNo = new();
        _byYearDay = new();
        _byMonthDay = new();
        _bySetPos = new();
    }

    private ByRuleValues(RecurrencePattern pattern)
    {
        _firstDayOfWeek = pattern.FirstDayOfWeek.ToIsoDayOfWeek();

        _normalMonths = SortValues(pattern.ByMonth);
        _normalHours = SortValues(pattern.ByHour);
        _normalMinutes = SortValues(pattern.ByMinute);
        _normalSeconds = SortValues(pattern.BySecond);

        _byWeekNo = new(pattern.ByWeekNo);
        _byYearDay = new(pattern.ByYearDay);
        _byMonthDay = new(pattern.ByMonthDay);
        _bySetPos = new(pattern.BySetPosition);

        _byDay = [.. pattern.ByDay.Select(static x => new WeekDayValue(x))];

        HasByDayOffsets = _byDay.Any(static x => x.Offset != null);
        HasNegativeSetPos = pattern.BySetPosition.Any(static x => x < 0);

        _normalPositiveSetPos = HasNegativeSetPos || pattern.BySetPosition.Count == 0
            ? [] : pattern.BySetPosition.OrderBy(_identityFunc).Distinct().ToArray();
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

    /// <summary>
    /// Normalized positive BYSETPOS values. This is only
    /// set when all values are positive.
    /// </summary>
    public int[] PositiveSetPos => _normalPositiveSetPos;

    /// <summary>
    /// True if there are any BYDAY values with offsets.
    /// Use this instead of checking the Length of <see cref="DaysOfWeekWithOffset"/>
    /// to prevent creating the array unless needed.
    /// </summary>
    public bool HasByDayOffsets { get; }

    /// <summary>
    /// True if BYSETPOS has negative values.
    /// </summary>
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

    /// <summary>
    /// Days of the week. Values are NOT normalized or sorted.
    /// </summary>
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
        => _byWeekNo.Normalize(weeksInWeekYear);

    /// <summary>
    /// Get normalized yeardays based on the year.
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public int[] GetYearDays(int year)
    {
        var daysInYear = CalendarSystem.Iso.GetDaysInYear(year);
        return _byYearDay.Normalize(daysInYear);
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
        return _byMonthDay.Normalize(daysInMonth);
    }

    /// <summary>
    /// Get normalized set positions based on set count.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public int[] GetSetPositions(int count) => _bySetPos.Normalize(count);

    /// <summary>
    /// Copies values to an array and sorts.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    private static int[] SortValues(List<int> values)
    {
        if (values.Count == 0)
        {
            // Return static empty array
            return [];
        }

        int[] result = [.. values];
        Array.Sort(result);

        return result;
    }

    /// <summary>
    /// Gets normalized days of the week that do not have offsets.
    /// </summary>
    /// <param name="byDay"></param>
    /// <param name="dayOfWeekComparer"></param>
    /// <returns></returns>
    private static IsoDayOfWeek[] NormalizeDaysWithoutOffset(WeekDayValue[] byDay, DayOfWeekComparer dayOfWeekComparer)
    {
        var normalizedDays = byDay
            .Where(x => x.Offset == null)
            .Select(x => x.DayOfWeek)
            .ToArray();

        Array.Sort(normalizedDays, dayOfWeekComparer);

        return normalizedDays;
    }

    private struct NormalValues
    {
        private readonly int[] _values;
        private Dictionary<int, int[]>? _cache;

        public readonly int Length => _values.Length;

        public NormalValues() => _values = [];

        public NormalValues(List<int> values)
        {
            _values = values.Count == 0 ? [] : [.. values];
        }

        public int[] Normalize(int max)
        {
            // Lazy create dictionary to reduce allocations
            _cache ??= [];

            // Cache normalized values using max values as the key
            if (!_cache.TryGetValue(max, out var result))
            {
                result = Normalize(_values, max);
                _cache[max] = result;
            }

            return result;
        }

        /// <summary>
        /// Gets normalized values based on the range of [1,max].
        /// </summary>
        /// <param name="values"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private static int[] Normalize(int[] values, int max)
        {
            return values
                .Select(x => (x > 0) ? x : x + max + 1)
                .Where(x => 0 < x && x <= max)
                .OrderBy(_identityFunc)
                .OrderedDistinct()
                .ToArray();
        }
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
