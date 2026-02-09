//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.Calendars;
using NodaTime.Extensions;

namespace Ical.Net.Evaluation;

internal sealed class RecurrencePatternEvaluator
{
    private readonly ByRuleValues _rule;
    private readonly CalDateTime _referenceDate;
    private readonly Instant? _periodStart;
    private readonly EvaluationOptions? _options;

    private readonly int? _count;
    private readonly int _interval;
    private readonly Instant? _until;
    private readonly FrequencyType _frequency;
    private readonly IsoDayOfWeek _firstDayOfWeek;

    private readonly IWeekYearRule _weekYearRule;
    private readonly ZonedDateTime _zonedReferenceDate;
    private readonly int _referenceWeekNo;

    private ZonedDateTime _seed;
    private int _unmatchedIncrementCount;

    public RecurrencePatternEvaluator(
        RecurrencePattern pattern,
        CalDateTime referenceDate,
        DateTimeZone timeZone,
        Instant? periodStart,
        EvaluationOptions? options)
    {
        _referenceDate = referenceDate;
        _periodStart = periodStart;
        _options = options;

        // Copy pattern values
        _frequency = pattern.Frequency;
        _until = pattern.Until?.ToZonedDateTime(timeZone).ToInstant();
        _count = pattern.Count;
        _interval = Math.Max(1, pattern.Interval);
        _rule = ByRuleValues.From(pattern);

        _firstDayOfWeek = pattern.FirstDayOfWeek.ToIsoDayOfWeek();
        _weekYearRule = WeekYearRules.ForMinDaysInFirstWeek(4, _firstDayOfWeek);
        _zonedReferenceDate = referenceDate.AsZonedOrDefault(timeZone);
        _referenceWeekNo = _weekYearRule.GetWeekOfWeekYear(_zonedReferenceDate.Date);
    }

    public RecurrencePatternEvaluator(
        RecurrencePattern pattern,
        CalDateTime referenceDate,
        ZonedDateTime periodStart,
        EvaluationOptions? options)
        : this(pattern, referenceDate, periodStart.Zone, periodStart.ToInstant(), options) { }

    public IEnumerable<EvaluationPeriod> Evaluate()
    {
        var evaluatedValuesCount = 0;

        // Determine where to start evaluation
        _seed = _zonedReferenceDate;
        if (_periodStart != null && _count == null)
        {
            _seed = SkipToByInterval(_seed, _periodStart.Value.InZone(_seed.Zone));
        }

        // Instant that all results must be greater than. Start with
        // the reference date minus 1s (minimum date resolution).
        var valueThreshold = _zonedReferenceDate
            .ToInstant()
            .Minus(NodaTime.Duration.FromSeconds(1));

        foreach (var value in BySetPosition())
        {
            var valueInstant = value.ToInstant();
            if (valueInstant > _until)
            {
                break;
            }

            if (valueInstant <= valueThreshold)
            {
                continue;
            }

            yield return new(value);

            // Update threshold to prevent duplicate values
            // caused by daylight saving transitions.
            valueThreshold = valueInstant;

            if (++evaluatedValuesCount >= _count)
            {
                yield break;
            }

            _unmatchedIncrementCount = 0;
        }
    }

    /// <summary>
    /// Get the datetime before or equal to the limit datetime
    /// using the recurrence interval.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    /// <exception cref="EvaluationException"></exception>
    private ZonedDateTime SkipToByInterval(ZonedDateTime start, ZonedDateTime limit)
    {
        var diff = limit - start;

        if (diff <= NodaTime.Duration.Zero)
        {
            return start;
        }

        // Handle time
        NodaTime.Duration time;
        switch (_frequency)
        {
            case FrequencyType.Secondly:
                time = NodaTime.Duration.FromSeconds(FromInterval((long) diff.TotalSeconds));
                return start.Plus(time);
            case FrequencyType.Minutely:
                time = NodaTime.Duration.FromMinutes(FromInterval((long) diff.TotalMinutes));
                return start.Plus(time);
            case FrequencyType.Hourly:
                time = NodaTime.Duration.FromHours(FromInterval((long) diff.TotalHours));
                return start.Plus(time);
        }

        // Handle nominal
        NodaTime.Period nominalDiff;
        int nominalByInterval;
        switch (_frequency)
        {
            case FrequencyType.Daily:
                nominalDiff = NodaTime.Period.Between(start.Date, limit.Date, PeriodUnits.Days);
                nominalByInterval = FromIntervalInt(nominalDiff.Days);
                return start.LocalDateTime.PlusDays(nominalByInterval)
                    .InZoneLeniently(start.Zone);
            case FrequencyType.Weekly:
                nominalDiff = NodaTime.Period.Between(start.Date, limit.Date, PeriodUnits.Weeks);
                nominalByInterval = FromIntervalInt(nominalDiff.Weeks);
                return start.LocalDateTime.PlusWeeks(nominalByInterval)
                    .InZoneLeniently(start.Zone);
            case FrequencyType.Monthly:
                nominalDiff = NodaTime.Period.Between(start.Date, limit.Date, PeriodUnits.Months);
                nominalByInterval = FromIntervalInt(nominalDiff.Months);
                return start.Date.PlusMonths(nominalByInterval)
                    .AtNearestDayOfMonth(_zonedReferenceDate.Day)
                    .At(start.TimeOfDay)
                    .InZoneLeniently(start.Zone);
            case FrequencyType.Yearly:
                nominalDiff = NodaTime.Period.Between(start.Date, limit.Date, PeriodUnits.Years);
                nominalByInterval = FromIntervalInt(nominalDiff.Years);
                return start.Date.PlusYears(nominalByInterval)
                    .AtNearestDayOfMonth(_zonedReferenceDate.Day)
                    .At(start.TimeOfDay)
                    .InZoneLeniently(start.Zone);
        }

        throw new EvaluationException($"Invalid frequency type {_frequency}");

        long FromInterval(long value) => _interval * (value / _interval);

        int FromIntervalInt(int value) => _interval * (value / _interval);
    }

    private IEnumerable<ZonedDateTime> BySetPosition()
    {
        if (!_rule.BySetPosition)
        {
            return StartByRules();
        }

        if (_rule.HasNegativeSetPos)
        {
            return LimitSetPosition();
        }

        return LimitSetPositionPositiveOnly();
    }

    /// <summary>
    /// Limit by set position. This supports positive and negative values.
    /// </summary>
    private IEnumerable<ZonedDateTime> LimitSetPosition()
    {
        var recurrenceSet = StartByRules();

        while (true)
        {
            // Evaluate the entire set so that negative offsets can be handled
            var values = recurrenceSet.ToList();

            if (values.Count > 0)
            {
                // Generate the set positions based on the number of items in the set
                var setPositions = _rule.GetSetPositions(values.Count);

                // Yield the values from each set position
                foreach (var pos in setPositions)
                {
                    yield return values[pos - 1];
                }
            }

            // Increment before enumerating the next set
            IncrementSeed();
        }
    }

    /// <summary>
    /// Limit by set position with positive values only. Ignoring negative
    /// set positions allows enumeration to end early once a set is finished.
    /// </summary>
    private IEnumerable<ZonedDateTime> LimitSetPositionPositiveOnly()
    {
        var recurrenceSet = StartByRules();

        // Set positions are all positive, so the set positions do
        // not change based on the size of the set. This allows
        // the set to yield one at a time.
        var setPositions = _rule.PositiveSetPos;

        while (true)
        {
            foreach (var value in FilterBySetPosition(recurrenceSet, setPositions))
            {
                yield return value;
            }

            // Increment before enumerating the next set
            IncrementSeed();
        }

        // Enumerates values and yields only when set position matches,
        // ending early if there are no more set positions. Only positive
        // set positions are supported.
        static IEnumerable<ZonedDateTime> FilterBySetPosition(IEnumerable<ZonedDateTime> values, int[] setPositions)
        {
            var x = 0;

            using var valueEnumerator = values.GetEnumerator();

            foreach (var pos in setPositions)
            {
                // Move to the next position in the set
                do
                {
                    if (!valueEnumerator.MoveNext())
                    {
                        yield break;
                    }
                } while (++x < pos);

                // Set position reached
                yield return valueEnumerator.Current;
            }
        }
    }

    /// <summary>
    /// Returns the BY rule to start at.
    ///
    /// RFC: BYSECOND, BYMINUTE and BYHOUR rule parts MUST NOT be specified
    /// when the associated "DTSTART" property has a DATE value type.
    /// </summary>
    private IEnumerable<ZonedDateTime> StartByRules()
        => _referenceDate.HasTime ? BySecond() : ByDay();

    private IEnumerable<ZonedDateTime> BySecond()
    {
        if (!_rule.BySecond)
        {
            return ByMinute();
        }

        if (_frequency > FrequencyType.Secondly)
        {
            return ExpandSecond();
        }

        return LimitSecond();
    }

    private IEnumerable<ZonedDateTime> LimitSecond()
    {
        foreach (var value in ByMinute())
        {
            if (_rule.Seconds.Contains(value.Second))
            {
                yield return value;
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandSecond()
    {
        foreach (var value in ByMinute())
        {
            foreach (var second in _rule.Seconds)
            {
                yield return value.Date
                    .At(new LocalTime(value.Hour, value.Minute, second))
                    .InZoneRelativeTo(value);
            }
        }
    }

    private IEnumerable<ZonedDateTime> ByMinute()
    {
        if (!_rule.ByMinute)
        {
            return ByHour();
        }

        if (_frequency > FrequencyType.Minutely)
        {
            return ExpandMinute();
        }

        return LimitMinute();
    }

    private IEnumerable<ZonedDateTime> LimitMinute()
    {
        foreach (var value in ByHour())
        {
            if (_rule.Minutes.Contains(value.Minute))
            {
                yield return value;
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandMinute()
    {
        foreach (var value in ByHour())
        {
            foreach (var minute in _rule.Minutes)
            {
                yield return value.Date
                    .At(new LocalTime(value.Hour, minute, value.Second))
                    .InZoneRelativeTo(value);
            }
        }
    }

    private IEnumerable<ZonedDateTime> ByHour()
    {
        if (!_rule.ByHour)
        {
            return ByDay();
        }

        if (_frequency > FrequencyType.Hourly)
        {
            return ExpandHour();
        }

        return LimitHour();
    }

    private IEnumerable<ZonedDateTime> LimitHour()
    {
        foreach (var value in ByDay())
        {
            if (_rule.Hours.Contains(value.Hour))
            {
                yield return value;
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandHour()
    {
        foreach (var value in ByDay())
        {
            foreach (var hour in _rule.Hours)
            {
                yield return value.Date
                    .At(new LocalTime(hour, value.Minute, value.Second))
                    .InZoneRelativeTo(value);
            }
        }
    }

    /// <summary>
    /// All values generated from here MUST represent a day (not weeks, months, or years).
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> ByDay()
    {
        if (_rule.ByDay)
        {
            return _frequency switch
            {
                FrequencyType.Weekly => ExpandDayFromWeekWithoutOffsets(),
                FrequencyType.Monthly => ByDaySpecialMonthly(),
                FrequencyType.Yearly => ByDaySpecialYearly(),
                _ => LimitDayOfWeek()
            };
        }

        if (_frequency < FrequencyType.Monthly
            || _rule.ByMonthDay
            || _rule.ByYearDay)
        {
            return ByMonthDay();
        }

        if (_rule.ByWeekNo)
        {
            return _rule.ByMonth ? LimitWeekByMonth() : ExpandWeekNo();
        }

        return LimitMonthByReferenceDay();
    }

    private IEnumerable<ZonedDateTime> ByDaySpecialMonthly()
        => _rule.ByMonthDay ? LimitDayOfMonth() : ExpandDayFromMonth();

    private IEnumerable<ZonedDateTime> ByDaySpecialYearly()
    {
        if (_rule.ByYearDay || _rule.ByMonthDay)
        {
            return _rule.ByMonth ? LimitDayOfMonth() : LimitDayOfYear();
        }

        if (_rule.ByWeekNo)
        {
            return ExpandDayFromWeek();
        }

        if (_rule.ByMonth)
        {
            return ExpandDayFromMonth();
        }

        return ExpandDayFromYear();
    }

    /// <summary>
    /// Limits weeks by month.
    /// </summary>
    private IEnumerable<ZonedDateTime> LimitWeekByMonth()
    {
        foreach (var value in ExpandWeekNo()) // NOSONAR
        {
            if (_rule.Months.Contains(value.Month))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Limits months by reference day.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> LimitMonthByReferenceDay()
    {
        var referenceDay = _zonedReferenceDate.Day;

        foreach (var value in ByMonth())
        {
            // Allow day to be different from the reference day
            // if there are fewer days in the month.
            if (value.Day == referenceDay
                || referenceDay <= CalendarSystem.Iso.GetDaysInMonth(value.Year, value.Month))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Limit by day of week. Offsets are not supported.
    /// Only used for DAILY or smaller.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="EvaluationException"></exception>
    private IEnumerable<ZonedDateTime> LimitDayOfWeek()
    {
        if (_rule.HasByDayOffsets)
        {
            throw new EvaluationException($"BYDAY offsets are invalid with frequency {_frequency}");
        }

        var daysOfWeek = _rule.DaysOfWeekWithoutOffset;

        foreach (var value in ByMonthDay())
        {
            if (daysOfWeek.Contains(value.DayOfWeek))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Limit by day of month.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> LimitDayOfMonth()
    {
        foreach (var value in ByMonthDay())
        {
            foreach (var weekDay in _rule.DaysOfWeek)
            {
                if (MatchesByDayOfMonth(weekDay, value.Date))
                {
                    yield return value;
                }
            }
        }

        static bool MatchesByDayOfMonth(WeekDayValue weekDay, LocalDate value)
        {
            if (weekDay.DayOfWeek != value.DayOfWeek)
            {
                return false;
            }

            if (weekDay.Offset == null)
            {
                return true;
            }

            // Check if offset matches
            if (weekDay.Offset > 0)
            {
                var offsetDate = new LocalDate(value.Year, value.Month, 1)
                    .CurrentOrNext(weekDay.DayOfWeek)
                    .PlusWeeks(weekDay.Offset.Value - 1);

                return offsetDate == value;
            }
            else if (weekDay.Offset < 0)
            {
                var offsetDate = new LocalDate(value.Year, value.Month, 1)
                    .PlusMonths(1)
                    .Previous(weekDay.DayOfWeek)
                    .PlusWeeks(weekDay.Offset.Value + 1);

                return offsetDate == value;
            }

            // Ignore invalid zero offset
            return false;
        }
    }

    /// <summary>
    /// Limit by day of year.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> LimitDayOfYear()
    {
        foreach (var value in ByMonthDay())
        {
            foreach (var weekDay in _rule.DaysOfWeek)
            {
                if (MatchesByDayOfYear(weekDay, value.Date))
                {
                    yield return value;
                }
            }
        }

        static bool MatchesByDayOfYear(WeekDayValue weekDay, LocalDate value)
        {
            if (weekDay.DayOfWeek != value.DayOfWeek)
            {
                return false;
            }

            if (weekDay.Offset == null)
            {
                return true;
            }

            // Check if offset matches
            if (weekDay.Offset > 0)
            {
                var offsetDate = new LocalDate(value.Year, 1, 1)
                    .CurrentOrNext(weekDay.DayOfWeek)
                    .PlusWeeks(weekDay.Offset.Value - 1);

                return offsetDate == value;
            }
            else if (weekDay.Offset < 0)
            {
                var offsetDate = new LocalDate(value.Year, 1, 1)
                    .PlusYears(1)
                    .Previous(weekDay.DayOfWeek)
                    .PlusWeeks(weekDay.Offset.Value + 1);

                return offsetDate == value;
            }

            // Ignore invalid zero offset
            return false;
        }
    }

    /// <summary>
    /// Expand by day of week. Offsets are not supported.
    /// Only used for WEEKLY.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> ExpandDayFromWeekWithoutOffsets()
    {
        // The value represents a week OR a week within the month
        foreach (var value in ByMonth())
        {
            var weekYear = _weekYearRule.GetWeekYear(value.Date);
            var week = _weekYearRule.GetWeekOfWeekYear(value.Date);

            foreach (var day in _rule.DaysOfWeekWithoutOffset)
            {
                var result = _weekYearRule.GetLocalDate(weekYear, week, day);

                // Limit by month if specified
                if (_rule.ByMonth && result.Month != value.Month)
                {
                    continue;
                }

                yield return result.At(value.TimeOfDay).InZoneLeniently(value.Zone);
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandDayFromWeek()
    {
        foreach (var value in ExpandWeekNo())
        {
            var start = value.Date.CurrentOrPrevious(_firstDayOfWeek);
            var end = start.PlusWeeks(1);

            foreach (var day in ExpandDayFromRange(start, end))
            {
                if (_rule.ByMonth && !_rule.Months.Contains(day.Month))
                {
                    continue;
                }

                yield return day.At(_zonedReferenceDate.TimeOfDay).InZoneLeniently(value.Zone);
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandDayFromMonth()
    {
        foreach (var value in ByMonth())
        {
            var start = new LocalDate(value.Year, value.Month, 1);
            var end = start.PlusMonths(1);

            foreach (var day in ExpandDayFromRange(start, end))
            {
                yield return day.At(_zonedReferenceDate.TimeOfDay).InZoneLeniently(value.Zone);
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandDayFromYear()
    {
        foreach (var value in Expand())
        {
            var start = new LocalDate(value.Year, 1, 1);
            var end = start.PlusYears(1);

            foreach (var day in ExpandDayFromRange(start, end))
            {
                yield return day.At(_zonedReferenceDate.TimeOfDay).InZoneLeniently(value.Zone);
            }
        }
    }

    /// <summary>
    /// Special expand by day based on the given range.
    /// </summary>
    /// <param name="start">Inclusive start of the range.</param>
    /// <param name="end">Exclusive end of the range.</param>
    /// <returns></returns>
    private IEnumerable<LocalDate> ExpandDayFromRange(LocalDate start, LocalDate end)
    {
        var daysWithoutOffset = _rule.DaysOfWeekWithoutOffset;

        // Expanding with and without offsets is done separately
        // because offsets require extra work that can be skipped
        // if there are no offsets.
        if (!_rule.HasByDayOffsets)
        {
            return ExpandDayOfWeekWithoutOffset(start, end, daysWithoutOffset);
        }

        // BYDAY offsets are a finite list of days that is likely small,
        // so it is more efficient to just calculate all of the days
        // and then sort instead of generating the days in order.
        IEnumerable<LocalDate> results = GetDayOfWeekWithOffset(start, end, _rule.DaysOfWeekWithOffset);

        if (daysWithoutOffset.Length > 0)
        {
            // There are days with and without offsets,
            // so they need to be merged together.
            results = ExpandDayOfWeekWithoutOffset(start, end, daysWithoutOffset)
                .OrderedMerge(results);
        }

        // An offset can overlap with another offset (e.g. 3WE,-3WE in
        // certain months are the same date), and can overlap with days
        // without offsets, so skip duplicates.
        return results.OrderedDistinct();
    }

    /// <summary>
    /// Expands by day of week within the given range.
    /// </summary>
    /// <param name="start">Inclusive start of the range</param>
    /// <param name="end">Exclusive end of the range</param>
    /// <param name="weekDays">Days of the week. Values MUST be sorted by first day of week.</param>
    /// <returns></returns>
    private static IEnumerable<LocalDate> ExpandDayOfWeekWithoutOffset(
        LocalDate start,
        LocalDate end,
        IsoDayOfWeek[] weekDays)
    {
        Debug.Assert(weekDays.Length > 0);

        var value = start;

        // Get the first date that matches a week day
        var i = Array.IndexOf(weekDays, value.DayOfWeek);
        if (i < 0)
        {
            value = value.Next(weekDays[0]);
            i = 0;
        }

        while (true)
        {
            if (value >= end)
            {
                yield break;
            }

            yield return value;

            i = (i + 1) % weekDays.Length;
            value = value.Next(weekDays[i]);
        }
    }

    /// <summary>
    /// Gets all values by day of week and offset within the given range.
    /// </summary>
    /// <param name="start">Inclusive start of the range</param>
    /// <param name="end">Exclusive end of the range</param>
    /// <param name="weekDays">Days of the week.</param>
    /// <returns>A sorted list of days within the range.</returns>
    private static List<LocalDate> GetDayOfWeekWithOffset(
        LocalDate start,
        LocalDate end,
        WeekDayValue[] weekDays)
    {
        var values = new List<LocalDate>(weekDays.Length);

        foreach (var weekDay in weekDays)
        {
            if (weekDay.Offset > 0)
            {
                var result = start
                    .CurrentOrNext(weekDay.DayOfWeek)
                    .PlusWeeks(weekDay.Offset.Value - 1);

                if (result < end)
                {
                    values.Add(result);
                }
            }
            else if (weekDay.Offset < 0)
            {
                // Context end is exclusive, so move directly
                // to previous day of week
                var result = end
                    .Previous(weekDay.DayOfWeek)
                    .PlusWeeks(weekDay.Offset.Value + 1);

                if (result >= start)
                {
                    values.Add(result);
                }
            }
        }

        values.Sort();

        return values;
    }

    private IEnumerable<ZonedDateTime> ByMonthDay()
    {
        if (!_rule.ByMonthDay)
        {
            return ByYearDay();
        }

        if (_frequency == FrequencyType.Weekly)
        {
            throw new EvaluationException("BYMONTHDAY is invalid with frequency WEEKLY");
        }

        if (_frequency > FrequencyType.Weekly)
        {
            if (_rule.ByYearDay)
            {
                // Values will be days, so just limit
                return LimitMonthDay();
            }

            if (_rule.ByWeekNo)
            {
                return ExpandMonthDayFromWeek();
            }

            return ExpandMonthDayFromMonth();
        }

        return LimitMonthDay();
    }

    private IEnumerable<ZonedDateTime> LimitMonthDay()
    {
        foreach (var value in ByYearDay())
        {
            if (_rule.GetMonthDays(value.Year, value.Month).Contains(value.Day))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Expand by day of month from month.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> ExpandMonthDayFromMonth()
    {
        foreach (var value in ByMonth())
        {
            foreach (var day in _rule.GetMonthDays(value.Year, value.Month))
            {
                yield return new LocalDate(value.Year, value.Month, day)
                    .At(value.TimeOfDay)
                    .InZoneLeniently(value.Zone);
            }
        }
    }

    /// <summary>
    /// Expand by day of month from week.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> ExpandMonthDayFromWeek()
    {
        foreach (var value in ExpandWeekNo())
        {
            // Start of the week
            var result = value.Date.CurrentOrPrevious(_firstDayOfWeek);
            var end = result.PlusWeeks(1);

            // Check each day for matches
            do
            {
                // Get month days each time because month may be different
                var monthDays = _rule.GetMonthDays(result.Year, result.Month);

                if (!monthDays.Contains(result.Day))
                {
                    continue;
                }

                if(_rule.ByMonth && !_rule.Months.Contains(result.Month))
                {
                    continue;
                }

                yield return result.At(value.TimeOfDay).InZoneLeniently(value.Zone);

            } while ((result = result.PlusDays(1)) != end);
        }
    }

    private IEnumerable<ZonedDateTime> ByYearDay()
    {
        if (!_rule.ByYearDay)
        {
            // BYWEEKNO should be handled by special-case
            // routes and never reach here.
            Debug.Assert(!_rule.ByWeekNo);

            return ByMonth();
        }

        if (_frequency == FrequencyType.Yearly)
        {
            if (_rule.ByWeekNo)
            {
                return ExpandYearDayFromWeek();
            }

            return ExpandYearDayFromMonth();
        }

        if (_frequency < FrequencyType.Daily)
        {
            return LimitYearDay();
        }

        throw new EvaluationException($"BYYEARDAY is invalid with frequency {_frequency}");
    }

    private IEnumerable<ZonedDateTime> LimitYearDay()
    {
        foreach (var value in ByMonth()) // NOSONAR
        {
            if (_rule.GetYearDays(value.Year).Contains(value.DayOfYear))
            {
                yield return value;
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandYearDayFromWeek()
    {
        foreach (var value in ExpandWeekNo())
        {
            // Start of week
            var result = value.Date.CurrentOrPrevious(_firstDayOfWeek);
            var end = result.PlusWeeks(1);

            // Check each day of the week for matches
            do
            {
                // Get year days each time because year may be different
                var yearDays = _rule.GetYearDays(result.Year);

                if (!yearDays.Contains(result.DayOfYear))
                {
                    continue;
                }

                if (_rule.ByMonth && !_rule.Months.Contains(result.Month))
                {
                    continue;
                }

                yield return result.At(value.TimeOfDay).InZoneLeniently(value.Zone);

            } while ((result = result.PlusDays(1)) != end);
        }
    }

    private IEnumerable<ZonedDateTime> ExpandYearDayFromMonth()
    {
        foreach (var value in ByMonth())
        {
            var startOfYear = new LocalDate(value.Year, 1, 1);

            foreach (var day in _rule.GetYearDays(value.Year))
            {
                var result = startOfYear.PlusDays(day - 1);

                // Ignore values outside the calendar year
                if (result.Year != value.Year)
                {
                    continue;
                }

                // If BYMONTH, limit year day to month
                if (_rule.ByMonth && result.Month != value.Month)
                {
                    continue;
                }

                yield return result.At(value.TimeOfDay).InZoneLeniently(value.Zone);
            }
        }
    }

    /// <summary>
    /// Expand by week number.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> ExpandWeekNo()
    {
        if (_frequency != FrequencyType.Yearly)
        {
            throw new EvaluationException($"BYWEEKNO is invalid with frequency {_frequency}");
        }

        // Expand weeks by interval instead of possible BYMONTH
        // so that weeks that span months/years are included.
        // BYMONTH is handled by the callers of this method.
        foreach (var value in Expand())
        {
            var weekYear = GetWeekYearBasedOnReferenceWeekYear(value.Date);

            var weeksInWeekYear = _weekYearRule.GetWeeksInWeekYear(weekYear);

            foreach (var weekNo in _rule.GetWeeks(weeksInWeekYear))
            {
                if (weekNo > weeksInWeekYear)
                {
                    continue;
                }

                yield return _weekYearRule
                    .GetLocalDate(weekYear, weekNo, _zonedReferenceDate.DayOfWeek)
                    .At(value.TimeOfDay)
                    .InZoneLeniently(value.Zone);
            }
        }
    }

    /// <summary>
    /// Determines the week year of the date relative to the
    /// reference date's week year.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private int GetWeekYearBasedOnReferenceWeekYear(LocalDate date)
    {
        // Make sure the month and day is as close as possible
        // to the reference month and day so that week numbers
        // can be compared accurately.
        if (date.Month != _zonedReferenceDate.Month || date.Day != _zonedReferenceDate.Day)
        {
            date = _zonedReferenceDate.Date
                .PlusYears(date.Year - _zonedReferenceDate.Year);
        }

        var weekYear = _weekYearRule.GetWeekYear(date);
        var valueWeekNo = _weekYearRule.GetWeekOfWeekYear(date);

        // Adjust week year to match reference week. Reference dates
        // in the first and last week of a year may result in different
        // week years than what YEARLY intends. Week numbers should have
        // an absolute difference greater than 1 only when one week
        // number is 1 and the other is 52 or 53.
        var weekNoDiff = valueWeekNo - _referenceWeekNo;
        if (weekNoDiff > 1)
        {
            weekYear += 1;
        }
        else if (weekNoDiff < -1)
        {
            weekYear -= 1;
        }

        return weekYear;
    }

    private IEnumerable<ZonedDateTime> ByMonth()
    {
        if (!_rule.ByMonth)
        {
            return Expand();
        }

        if (_frequency == FrequencyType.Yearly)
        {
            return ExpandMonth();
        }

        return LimitMonth();
    }

    private IEnumerable<ZonedDateTime> LimitMonth()
    {
        foreach (var value in Expand())
        {
            if (_rule.Months.Contains(value.Month))
            {
                yield return value;
            }
        }
    }

    private IEnumerable<ZonedDateTime> ExpandMonth()
    {
        foreach (var value in Expand())
        {
            foreach (var month in _rule.Months)
            {
                var daysInMonth = CalendarSystem.Iso.GetDaysInMonth(value.Year, month);
                var day = Math.Min(daysInMonth, value.Day);

                yield return new LocalDate(value.Year, month, day)
                    .At(value.TimeOfDay)
                    .InZoneLeniently(value.Zone);
            }
        }
    }

    /// <summary>
    /// Expands the seed value by frequency and interval.
    /// If BYSETPOS, only the current seed value is returned.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ZonedDateTime> Expand()
    {
        if (_rule.BySetPosition)
        {
            // Yield only one value so that a single set can be generated
            yield return _seed;
        }
        else
        {
            // Expand forever
            while (true)
            {
                yield return _seed;
                IncrementSeed();
            }
        }
    }

    /// <summary>
    /// Gets the next seed value based on the recurrence frequency
    /// and interval.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="EvaluationLimitExceededException"></exception>
    /// <exception cref="EvaluationException"></exception>
    private void IncrementSeed()
    {
        if (_unmatchedIncrementCount++ > _options?.MaxUnmatchedIncrementsLimit)
        {
            throw new EvaluationLimitExceededException();
        }

        _seed = _frequency switch
        {
            FrequencyType.Secondly => _seed.PlusSeconds(_interval),
            FrequencyType.Minutely => _seed.PlusMinutes(_interval),
            FrequencyType.Hourly => _seed.PlusHours(_interval),

            FrequencyType.Daily => _seed.LocalDateTime
                .PlusDays(_interval)
                .InZoneLeniently(_seed.Zone),

            FrequencyType.Weekly => _seed.LocalDateTime
                .PlusWeeks(_interval)
                .InZoneLeniently(_seed.Zone),

            FrequencyType.Monthly => _seed.Date
                .PlusMonths(_interval)
                .AtNearestDayOfMonth(_zonedReferenceDate.Day)
                .At(_seed.TimeOfDay)
                .InZoneLeniently(_seed.Zone),

            FrequencyType.Yearly => _seed.Date
                .PlusYears(_interval)
                .AtNearestDayOfMonth(_zonedReferenceDate.Day)
                .At(_seed.TimeOfDay)
                .InZoneLeniently(_seed.Zone),

            _ => throw new EvaluationException($"Invalid frequency ${_frequency}")
        };
    }
}
