﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

public class RecurrencePatternEvaluator : Evaluator
{
    private const int MaxIncrementCount = 1000;

    protected RecurrencePattern Pattern { get; set; }

    public RecurrencePatternEvaluator(RecurrencePattern pattern)
    {
        Pattern = pattern;
    }

    private RecurrencePattern ProcessRecurrencePattern(CalDateTime referenceDate)
    {
        var r = new RecurrencePattern();
        r.CopyFrom(Pattern);

        if (referenceDate.HasTime)
        {
            if (r.Frequency > FrequencyType.Secondly && r.BySecond.Count == 0 && referenceDate.HasTime
                /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
            {
                r.BySecond.Add(referenceDate.Second);
            }
            if (r.Frequency > FrequencyType.Minutely && r.ByMinute.Count == 0 && referenceDate.HasTime
                /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
            {
                r.ByMinute.Add(referenceDate.Minute);
            }
            if (r.Frequency > FrequencyType.Hourly && r.ByHour.Count == 0 && referenceDate.HasTime
                /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
            {
                r.ByHour.Add(referenceDate.Hour);
            }
        }
        else
        {
            // The BYSECOND, BYMINUTE and BYHOUR rule parts MUST NOT be specified
            // when the associated "DTSTART" property has a DATE value type.
            // These rule parts MUST be ignored in RECUR value that violate the
            // above requirement(e.g., generated by applications that pre - date
            // this revision of iCalendar).
            r.BySecond.Clear();
            r.BySecond.Add(0);
            r.ByMinute.Clear();
            r.ByMinute.Add(0);
            r.ByHour.Clear();
            r.ByHour.Add(0);
        }

        // If BYDAY, BYYEARDAY, or BYWEEKNO is specified, then
        // we don't default BYDAY, BYMONTH or BYMONTHDAY
        if (r.ByDay.Count == 0)
        {
            // If the frequency is weekly, use the original date's day of week.
            // NOTE: fixes WeeklyCount1() and WeeklyUntil1() handling
            // If BYWEEKNO is specified and BYMONTHDAY/BYYEARDAY is not specified,
            // then let's add BYDAY to BYWEEKNO.
            // NOTE: fixes YearlyByWeekNoX() handling
            if (r.Frequency == FrequencyType.Weekly || (r.ByWeekNo.Count > 0 && r.ByMonthDay.Count == 0 && r.ByYearDay.Count == 0))
            {
                r.ByDay.Add(new WeekDay(referenceDate.DayOfWeek));
            }

            // If BYMONTHDAY is not specified,
            // default to the current day of month.
            // NOTE: fixes YearlyByMonth1() handling, added BYYEARDAY exclusion
            // to fix YearlyCountByYearDay1() handling
            if (r.Frequency > FrequencyType.Weekly && r.ByWeekNo.Count == 0 && r.ByYearDay.Count == 0 && r.ByMonthDay.Count == 0)
            {
                r.ByMonthDay.Add(referenceDate.Day);
            }

            // If BYMONTH is not specified, default to
            // the current month.
            // NOTE: fixes YearlyCountByYearDay1() handling
            if (r.Frequency > FrequencyType.Monthly && r.ByWeekNo.Count == 0 && r.ByYearDay.Count == 0 && r.ByMonth.Count == 0)
            {
                r.ByMonth.Add(referenceDate.Month);
            }
        }

        return r;
    }

    /// <summary>
    /// Returns a list of start dates in the specified period represented by this recurrence pattern.
    /// This method includes a base date argument, which indicates the start of the first occurrence of this recurrence.
    /// The base date is used to inject default values to return a set of dates in the correct format.
    /// For example, if the search start date (start) is Wed, Mar 23, 12:19PM, but the recurrence is Mon - Fri, 9:00AM - 5:00PM,
    /// the start dates returned should all be at 9:00AM, and not 12:19PM.
    /// </summary>
    private IEnumerable<DateTime> GetDates(CalDateTime seed, CalDateTime? periodStart, CalDateTime? periodEnd, int maxCount, RecurrencePattern pattern,
         bool includeReferenceDateInResults)
    {
        // In the first step, we work with DateTime values, so we need to convert the CalDateTime to DateTime
        var originalDate = seed.Value;
        var seedCopy = seed.Value;
        var periodStartDt = periodStart?.ToTimeZone(seed.TzId)?.Value;
        var periodEndDt = periodEnd?.ToTimeZone(seed.TzId)?.Value;

        if ((pattern.Frequency == FrequencyType.Yearly) && (pattern.ByWeekNo.Count != 0))
        {
            // Dates in the first or last week of the year could belong weeks that belong to
            // the prev/next year, in which case we must adjust that year. This is necessary
            // to get the invervals right.
            IncrementDate(ref seedCopy, pattern, Calendar.GetIso8601YearOfWeek(seedCopy, pattern.FirstDayOfWeek) - seedCopy.Year);
        }

        // optimize the start time for selecting candidates
        // (only applicable where a COUNT is not specified)
        if (pattern.Count is null)
        {
            var incremented = seedCopy;
            while (incremented < periodStartDt)
            {
                seedCopy = incremented;
                IncrementDate(ref incremented, pattern, pattern.Interval);
            }
        } else
        {
            if (pattern.Count < 1)
                throw new Exception("Count must be greater than 0");
        }

        // Do the enumeration in a separate method, as it is a generator method that is
        // only executed after enumeration started. In order to do most validation upfront,
        // do as many steps outside the generator as possible.
        return EnumerateDates(originalDate, seedCopy, periodStartDt, periodEndDt, maxCount, pattern);
    }

    private IEnumerable<DateTime> EnumerateDates(DateTime originalDate, DateTime seedCopy, DateTime? periodStart, DateTime? periodEnd, int maxCount, RecurrencePattern pattern)
    {
        var expandBehavior = RecurrenceUtil.GetExpandBehaviorList(pattern);

        // This value is only used for performance reasons to stop incrementing after
        // until is passed, even if no recurrences are being found.
        // As a safe heuristic we add 1d to the UNTIL value to cover any time shift and DST changes.
        // It's just important that we don't miss any recurrences, not that we stop exactly at UNTIL.
        // Precise UNTIL handling is done outside this method after TZ conversion.
        var coarseUntil = pattern.Until?.Value.AddDays(1);

        var noCandidateIncrementCount = 0;
        DateTime? candidate = null;
        var dateCount = 0;
        while (maxCount < 0 || dateCount < maxCount)
        {
            if (seedCopy > coarseUntil)
            {
                break;
            }

            if (candidate > periodEnd)
            {
                break;
            }

            if (dateCount >= pattern.Count)
            {
                break;
            }

            //No need to continue if the seed is after the periodEnd
            if (seedCopy > periodEnd)
            {
                break;
            }

            var candidates = GetCandidates(seedCopy, pattern, expandBehavior);
            if (candidates.Count > 0)
            {
                noCandidateIncrementCount = 0;

                foreach (var t in candidates.Where(t => t >= originalDate))
                {
                    candidate = t;

                    // candidates MAY occur before periodStart
                    // For example, FREQ=YEARLY;BYWEEKNO=1 could return dates
                    // from the previous year.
                    //
                    // exclude candidates that start at the same moment as periodEnd if the period is a range but keep them if targeting a specific moment
                    if (dateCount >= pattern.Count)
                    {
                        break;
                    }

                    if ((candidate >= periodEnd && periodStart != periodEnd) || candidate > periodEnd && periodStart == periodEnd)
                    {
                        continue;
                    }

                    // UNTIL is applied outside of this method, after TZ conversion has been applied.

                    yield return candidate.Value;
                    dateCount++;
                }
            }
            else
            {
                noCandidateIncrementCount++;
                if (noCandidateIncrementCount > MaxIncrementCount)
                {
                    break;
                }
            }

            IncrementDate(ref seedCopy, pattern, pattern.Interval);
        }
    }

    private struct ExpandContext
    {
        /// <summary>
        /// Indicates whether the dates have been fully expanded. If true, subsequent parts should only limit, not expand.
        /// </summary>
        /// <remarks>
        /// This makes a difference in case of BYWEEKNO, which might span months and years. After it was applied (BYWEEKNO would
        /// always expand), the subsequent parts mustn't expand.
        /// </remarks>
        public bool DatesFullyExpanded { get; set; }
    }

    /// <summary>
    /// Returns a list of possible dates generated from the applicable BY* rules, using the specified date as a seed.
    /// </summary>
    /// <param name="date">The seed date.</param>
    /// <param name="pattern"></param>
    /// <param name="expandBehaviors"></param>
    /// <returns>A list of possible dates.</returns>
    private ISet<DateTime> GetCandidates(DateTime date, RecurrencePattern pattern, bool?[] expandBehaviors)
    {
        var expandContext = new ExpandContext() { DatesFullyExpanded = false };

        var dates = new List<DateTime> { date };
        dates = GetMonthVariants(dates, pattern, expandBehaviors[0]);
        dates = GetWeekNoVariants(dates, pattern, expandBehaviors[1], ref expandContext);
        dates = GetYearDayVariants(dates, pattern, expandBehaviors[2], ref expandContext);
        dates = GetMonthDayVariants(dates, pattern, expandBehaviors[3], ref expandContext);
        dates = GetDayVariants(dates, pattern, expandBehaviors[4], ref expandContext);
        dates = GetHourVariants(dates, pattern, expandBehaviors[5]);
        dates = GetMinuteVariants(dates, pattern, expandBehaviors[6]);
        dates = GetSecondVariants(dates, pattern, expandBehaviors[7]);
        dates = ApplySetPosRules(dates, pattern);
        return new SortedSet<DateTime>(dates);
    }

    /// <summary>
    /// Applies BYSETPOS rules to <paramref name="dates"/>. Valid positions are from 1 to the size of the date list. Invalid
    /// positions are ignored.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYSETPOS rules will be applied.</param>
    /// <param name="pattern"></param>
    private List<DateTime> ApplySetPosRules(List<DateTime> dates, RecurrencePattern pattern)
    {
        // return if no SETPOS rules specified..
        if (pattern.BySetPosition.Count == 0)
        {
            return dates;
        }

        // sort the list before processing..
        dates.Sort();

        var size = dates.Count;
        var setPosDates = pattern.BySetPosition
            .Where(p => p > 0 && p <= size || p < 0 && p >= -size)  //Protect against out of range access
            .Select(p => p > 0 && p <= size
                ? dates[p - 1]
                : dates[size + p])
            .ToList();
        return setPosDates;
    }

    /// <summary>
    /// Applies BYMONTH rules specified in this Recur instance to the specified date list. 
    /// If no BYMONTH rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYMONTH rules will be applied.</param>
    /// <param name="pattern"></param>
    /// <param name="expand"></param>
    /// <returns>The modified list of dates after applying the BYMONTH rules.</returns>
    private List<DateTime> GetMonthVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
    {
        if (expand == null || pattern.ByMonth.Count == 0)
        {
            return dates;
        }

        if (expand.Value)
        {
            // Expand behavior
            return dates
                .SelectMany(d => pattern.ByMonth.Select(month => d.AddMonths(month - d.Month)))
                .ToList();
        }

        // Limit behavior
        var dateSet = new HashSet<DateTime>(dates);
        dateSet.ExceptWith(dates.Where(date => pattern.ByMonth.All(t => t != date.Month)));
        return dateSet.ToList();
    }

    /// <summary>
    /// Applies BYWEEKNO rules specified in this Recur instance to the specified date list. 
    /// If no BYWEEKNO rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYWEEKNO rules will be applied.</param>
    /// <returns>The modified list of dates after applying the BYWEEKNO rules.</returns>
    private List<DateTime> GetWeekNoVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand, ref ExpandContext expandContext)
    {
        if (expand == null || pattern.ByWeekNo.Count == 0)
        {
            return dates;
        }

        if (!expand.Value)
        {
            return new List<DateTime>();
        }

        // Expand behavior
        var weekNoDates = new List<DateTime>();
        foreach ((var t, var weekNo) in dates.SelectMany(t => GetByWeekNoForYearNormalized(pattern, t.Year), (t, weekNo) => (t, weekNo)))
        {
            var date = t;

            // Make sure we start from a reference date that is in a week that belongs to the current year.
            // Its not important that the date lies in a certain week, but that the week belongs to the
            // current year and that the week day is preserved.
            if (date.Month == 1)
                date = date.AddDays(7);
            else if (date.Month >= 12)
                date = date.AddDays(-7);

            // Determine our current week number
            var currWeekNo = Calendar.GetIso8601WeekOfYear(date, pattern.FirstDayOfWeek);

            // Move ahead to the correct week of the year
            date = date.AddDays((weekNo - currWeekNo) * 7);

            // Ignore the week if it doesn't belong to the current year.
            if (Calendar.GetIso8601YearOfWeek(date, pattern.FirstDayOfWeek) != t.Year)
                continue;

            // Step backward single days until we're at the correct DayOfWeek
            date = GetFirstDayOfWeekDate(date, pattern.FirstDayOfWeek);

            weekNoDates.AddRange(Enumerable.Range(0, 7).Select(i => date.AddDays(i)));
        }

        // subsequent parts should only limit, not expand
        expandContext.DatesFullyExpanded = true;

        // Apply BYMONTH limit behavior, as we might have expanded over month/year boundaries
        // in this method and BYMONTH has already been applied before, so wouldn't be again.
        weekNoDates = GetMonthVariants(weekNoDates, pattern, expand: false);

        return weekNoDates;
    }

    private static DateTime GetFirstDayOfWeekDate(DateTime date, DayOfWeek firstDayOfWeek)
        => date.AddDays(-((int) date.DayOfWeek + 7 - (int) firstDayOfWeek) % 7);

    /// <summary>
    /// Normalize the BYWEEKNO values to be positive integers.
    /// </summary>
    private List<int> GetByWeekNoForYearNormalized(RecurrencePattern pattern, int year)
    {
        var weeksInYear = new Lazy<int>(() => Calendar.GetIso8601WeeksInYear(year, pattern.FirstDayOfWeek));
        return pattern.ByWeekNo
            .Select(weekNo => weekNo >= 0 ? weekNo : weeksInYear.Value + weekNo + 1)
            .ToList();
    }

    /// <summary>
    /// Applies BYYEARDAY rules specified in this Recur instance to the specified date list. 
    /// If no BYYEARDAY rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYYEARDAY rules will be applied.</param>
    /// <returns>The modified list of dates after applying the BYYEARDAY rules.</returns>
    private static List<DateTime> GetYearDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand, ref ExpandContext expandContext)
    {
        if (expand == null || pattern.ByYearDay.Count == 0)
        {
            return dates;
        }

        if (expand.Value && !expandContext.DatesFullyExpanded)
        {
            var yearDayDates = new List<DateTime>(dates.Count);
            foreach (var date in dates)
            {
                var date1 = date;
                yearDayDates.AddRange(pattern.ByYearDay.Select(yearDay => yearDay > 0
                    ? date1.AddDays(-date1.DayOfYear + yearDay)
                    : date1.AddDays(-date1.DayOfYear + 1).AddYears(1).AddDays(yearDay))
                    // Ignore the BY values that don't fit into the current year (i.e. +-366 in non-leap-years).
                    .Where(d => d.Year == date1.Year));
            }

            expandContext.DatesFullyExpanded = true;
            return yearDayDates;
        }
        // Limit behavior
        for (var i = dates.Count - 1; i >= 0; i--)
        {
            var date = dates[i];
            var keepDate = false;
            for (var j = 0; j < pattern.ByYearDay.Count; j++)
            {
                var yearDay = pattern.ByYearDay[j];

                var newDate = yearDay > 0
                    ? date.AddDays(-date.DayOfYear + yearDay)
                    : date.AddDays(-date.DayOfYear + 1).AddYears(1).AddDays(yearDay);

                if (newDate.Date == date.Date)
                {
                    keepDate = true;
                    break;
                }
            }

            if (!keepDate)
            {
                dates.RemoveAt(i);
            }
        }

        return dates;
    }

    /// <summary>
    /// Applies BYMONTHDAY rules specified in this Recur instance to the specified date list. 
    /// If no BYMONTHDAY rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYMONTHDAY rules will be applied.</param>
    /// <returns>The modified list of dates after applying the BYMONTHDAY rules.</returns>
    private List<DateTime> GetMonthDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand, ref ExpandContext expandContext)
    {
        if (expand == null || pattern.ByMonthDay.Count == 0)
        {
            return dates;
        }

        if (expand.Value && !expandContext.DatesFullyExpanded)
        {
            var monthDayDates = new List<DateTime>();
            foreach (var date in dates)
            {
                monthDayDates.AddRange(
                    from monthDay in pattern.ByMonthDay
                    let daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month)
                    where Math.Abs(monthDay) <= daysInMonth
                    select monthDay > 0
                        ? date.AddDays(-date.Day + monthDay)
                        : date.AddDays(-date.Day + 1).AddMonths(1).AddDays(monthDay)
                );
            }

            expandContext.DatesFullyExpanded = true;
            return monthDayDates;
        }

        // Limit behavior
        for (var i = dates.Count - 1; i >= 0; i--)
        {
            var date = dates[i];
            var keepDate = false;
            for (var j = 0; j < pattern.ByMonthDay.Count; j++)
            {
                var monthDay = pattern.ByMonthDay[j];

                var daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
                if (Math.Abs(monthDay) > daysInMonth)
                {
                    throw new ArgumentException("Invalid day of month: " + date + " (day " + monthDay + ")");
                }

                // Account for positive or negative numbers
                var newDate = monthDay > 0
                    ? date.AddDays(-date.Day + monthDay)
                    : date.AddDays(-date.Day + 1).AddMonths(1).AddDays(monthDay);

                if (newDate.Day.Equals(date.Day))
                {
                    keepDate = true;
                    break;
                }
            }

            if (!keepDate)
            {
                dates.RemoveAt(i);
            }
        }

        return dates;
    }

    /// <summary>
    /// Applies BYDAY rules specified in this Recur instance to the specified date list. 
    /// If no BYDAY rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which BYDAY rules will be applied.</param>
    /// <returns>The modified list of dates after applying BYDAY rules, or the original list if no BYDAY rules are specified.</returns>
    private List<DateTime> GetDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand, ref ExpandContext expandContext)
    {
        if (expand == null || pattern.ByDay.Count == 0)
        {
            return dates;
        }

        if (expand.Value && !expandContext.DatesFullyExpanded)
        {
            // Expand behavior
            var weekDayDates = new List<DateTime>();
            foreach (var date in dates)
            {
                foreach (var day in pattern.ByDay)
                {
                    weekDayDates.AddRange(GetAbsWeekDays(date, day, pattern));
                }
            }

            expandContext.DatesFullyExpanded = true;
            return weekDayDates;
        }

        // Limit behavior
        for (var i = dates.Count - 1; i >= 0; i--)
        {
            var date = dates[i];
            var keepDate = false;
            for (var j = 0; j < pattern.ByDay.Count; j++)
            {
                var weekDay = pattern.ByDay[j];
                if (weekDay.DayOfWeek.Equals(date.DayOfWeek))
                {
                    // If no offset is specified, simply test the day of week!
                    // FIXME: test with offset...
                    if (date.DayOfWeek.Equals(weekDay.DayOfWeek))
                    {
                        keepDate = true;
                        break;
                    }
                }
            }

            if (!keepDate)
            {
                dates.RemoveAt(i);
            }
        }

        return dates;
    }

    /// <summary>
    /// Returns a list of applicable dates corresponding to the specified week day in accordance with the frequency
    /// specified by this recurrence rule.
    /// </summary>
    /// <param name="date">The date to start the evaluation from.</param>
    /// <param name="weekDay">The week day to evaluate.</param>
    /// <returns>A list of applicable dates.</returns>
    private List<DateTime> GetAbsWeekDays(DateTime date, WeekDay weekDay, RecurrencePattern pattern)
    {
        var days = new List<DateTime>();

        var dayOfWeek = weekDay.DayOfWeek;
        if (pattern.Frequency == FrequencyType.Daily)
        {
            if (date.DayOfWeek == dayOfWeek)
            {
                days.Add(date);
            }
        }
        else if (pattern.Frequency == FrequencyType.Weekly || pattern.ByWeekNo.Count > 0)
        {
            var weekNo = Calendar.GetIso8601WeekOfYear(date, pattern.FirstDayOfWeek);

            // Go to the first day of the week
            date = date.AddDays(-GetWeekDayOffset(date, pattern.FirstDayOfWeek));

            // construct a list of possible week days..
            while (date.DayOfWeek != dayOfWeek)
            {
                date = date.AddDays(1);
            }

            var nextWeekNo = Calendar.GetIso8601WeekOfYear(date, pattern.FirstDayOfWeek);
            var currentWeekNo = Calendar.GetIso8601WeekOfYear(date, pattern.FirstDayOfWeek);
            var byWeekNoNormalized = GetByWeekNoForYearNormalized(pattern, Calendar.GetIso8601YearOfWeek(date, pattern.FirstDayOfWeek));

            //When we manage weekly recurring pattern and we have boundary case:
            //Weekdays: Dec 31, Jan 1, Feb 1, Mar 1, Apr 1, May 1, June 1, Dec 31 - It's the 53th week of the year, but all another are 1st week number.
            //So we need an EXRULE for this situation, but only for weekly events
            while (currentWeekNo == weekNo || (nextWeekNo < weekNo && currentWeekNo == nextWeekNo && pattern.Frequency == FrequencyType.Weekly))
            {
                if ((byWeekNoNormalized.Count == 0 || byWeekNoNormalized.Contains(currentWeekNo))
                    && (pattern.ByMonth.Count == 0 || pattern.ByMonth.Contains(date.Month)))
                {
                    days.Add(date);
                }

                date = date.AddDays(7);
                currentWeekNo = Calendar.GetIso8601WeekOfYear(date, pattern.FirstDayOfWeek);
            }
        }
        else if (pattern.Frequency == FrequencyType.Monthly || pattern.ByMonth.Count > 0)
        {
            var month = date.Month;

            // construct a list of possible month days..
            date = date.AddDays(-date.Day + 1);
            while (date.DayOfWeek != dayOfWeek)
            {
                date = date.AddDays(1);
            }

            var byWeekNoNormalized = GetByWeekNoForYearNormalized(pattern, Calendar.GetIso8601YearOfWeek(date, pattern.FirstDayOfWeek));
            while (date.Month == month)
            {
                var currentWeekNo = Calendar.GetIso8601WeekOfYear(date, pattern.FirstDayOfWeek);

                if ((byWeekNoNormalized.Count == 0 || byWeekNoNormalized.Contains(currentWeekNo))
                    && (pattern.ByMonth.Count == 0 || pattern.ByMonth.Contains(date.Month)))
                {
                    days.Add(date);
                }
                date = date.AddDays(7);
            }
        }
        else if (pattern.Frequency == FrequencyType.Yearly)
        {
            var year = date.Year;

            // construct a list of possible year days..
            date = date.AddDays(-date.DayOfYear + 1);
            while (date.DayOfWeek != dayOfWeek)
            {
                date = date.AddDays(1);
            }

            while (date.Year == year)
            {
                days.Add(date);
                date = date.AddDays(7);
            }
        }
        return GetOffsetDates(days, weekDay.Offset);
    }

    /// <summary>
    /// Returns the days since the start of the week, 0 if the date is on the first day of the week.
    /// </summary>
    private static int GetWeekDayOffset(DateTime date, DayOfWeek startOfWeek)
        => date.DayOfWeek + ((date.DayOfWeek < startOfWeek) ? 7 : 0) - startOfWeek;

    /// <summary>
    /// Returns a single-element sublist containing the element of <paramref name="dates"/> at <paramref name="offset"/>. 
    /// Valid offsets are from 1 to the size of the list. If an invalid offset is supplied, all elements from <paramref name="dates"/>
    /// are added to result.
    /// </summary>
    /// <param name="dates">The list from which to extract the element.</param>
    /// <param name="offset">The position of the element to extract.</param>
    private List<DateTime> GetOffsetDates(List<DateTime> dates, int? offset)
    {
        if (offset is null)
        {
            return dates;
        }

        var offsetDates = new List<DateTime>();
        var size = dates.Count;
        if (offset < 0 && offset >= -size)
        {
            offsetDates.Add(dates[size + offset.Value]);
        }
        else if (offset > 0 && offset <= size)
        {
            offsetDates.Add(dates[offset.Value - 1]);
        }
        return offsetDates;
    }

    /// <summary>
    /// Applies BYHOUR rules specified in this Recur instance to the specified date list. 
    /// If no BYHOUR rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYHOUR rules will be applied.</param>
    /// <param name="pattern"></param>
    /// <param name="expand"></param>
    /// <returns>The modified list of dates after applying the BYHOUR rules.</returns>
    private List<DateTime> GetHourVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
    {
        if (expand == null || pattern.ByHour.Count == 0)
        {
            return dates;
        }

        if (expand.Value)
        {
            // Expand behavior
            var hourlyDates = new List<DateTime>();
            for (var i = 0; i < dates.Count; i++)
            {
                var date = dates[i];
                for (var j = 0; j < pattern.ByHour.Count; j++)
                {
                    var hour = pattern.ByHour[j];
                    date = date.AddHours(-date.Hour + hour);
                    hourlyDates.Add(date);
                }
            }
            return hourlyDates;
        }
        // Limit behavior
        for (var i = dates.Count - 1; i >= 0; i--)
        {
            var date = dates[i];
            var keepDate = false;
            for (var j = 0; j < pattern.ByHour.Count; j++)
            {
                var hour = pattern.ByHour[j];
                if (date.Hour == hour)
                {
                    keepDate = true;
                    break;
                }
            }
            // Remove unmatched dates
            if (!keepDate)
            {
                dates.RemoveAt(i);
            }
        }
        return dates;
    }

    /// <summary>
    /// Applies BYMINUTE rules specified in this Recur instance to the specified date list. 
    /// If no BYMINUTE rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYMINUTE rules will be applied.</param>
    /// <param name="pattern"></param>
    /// <param name="expand"></param>
    /// <returns>The modified list of dates after applying the BYMINUTE rules.</returns>
    private List<DateTime> GetMinuteVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
    {
        if (expand == null || pattern.ByMinute.Count == 0)
        {
            return dates;
        }

        if (expand.Value)
        {
            // Expand behavior
            var minutelyDates = new List<DateTime>();
            for (var i = 0; i < dates.Count; i++)
            {
                var date = dates[i];
                for (var j = 0; j < pattern.ByMinute.Count; j++)
                {
                    var minute = pattern.ByMinute[j];
                    date = date.AddMinutes(-date.Minute + minute);
                    minutelyDates.Add(date);
                }
            }
            return minutelyDates;
        }
        // Limit behavior
        for (var i = dates.Count - 1; i >= 0; i--)
        {
            var date = dates[i];
            var keepDate = false;
            for (var j = 0; j < pattern.ByMinute.Count; j++)
            {
                var minute = pattern.ByMinute[j];
                if (date.Minute == minute)
                {
                    keepDate = true;
                }
            }
            // Remove unmatched dates
            if (!keepDate)
            {
                dates.RemoveAt(i);
            }
        }
        return dates;
    }

    /// <summary>
    /// Applies BYSECOND rules specified in this Recur instance to the specified date list. 
    /// If no BYSECOND rules are specified, the date list is returned unmodified.
    /// </summary>
    /// <param name="dates">The list of dates to which the BYSECOND rules will be applied.</param>
    /// <param name="pattern"></param>
    /// <param name="expand"></param>
    /// <returns>The modified list of dates after applying the BYSECOND rules.</returns>
    private List<DateTime> GetSecondVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
    {
        if (expand == null || pattern.BySecond.Count == 0)
        {
            return dates;
        }

        if (expand.Value)
        {
            // Expand behavior
            var secondlyDates = new List<DateTime>();
            for (var i = 0; i < dates.Count; i++)
            {
                var date = dates[i];
                for (var j = 0; j < pattern.BySecond.Count; j++)
                {
                    var second = pattern.BySecond[j];
                    date = date.AddSeconds(-date.Second + second);
                    secondlyDates.Add(date);
                }
            }
            return secondlyDates;
        }
        // Limit behavior
        for (var i = dates.Count - 1; i >= 0; i--)
        {
            var date = dates[i];
            var keepDate = false;
            for (var j = 0; j < pattern.BySecond.Count; j++)
            {
                var second = pattern.BySecond[j];
                if (date.Second == second)
                {
                    keepDate = true;
                    break;
                }
            }

            // Remove unmatched dates
            if (!keepDate)
            {
                dates.RemoveAt(i);
            }
        }

        return dates;
    }

    /// <summary>
    /// Creates a new period from the specified date/time,
    /// where the <see cref="CalDateTime.HasTime"/> is taken into account.
    /// when initializing the new period with a new <see cref="CalDateTime"/>.
    /// </summary>
    private static Period CreatePeriod(DateTime dateTime, CalDateTime referenceDate)
    {
        // Turn each resulting date/time into an CalDateTime and associate it
        // with the reference date.
        CalDateTime newDt = new CalDateTime(dateTime, null, referenceDate.HasTime);
        if (referenceDate.TzId != null) {
            // Adjust nonexistent recurrence instances according to RFC 5545 3.3.5
            newDt = newDt.ToTimeZone(referenceDate.TzId);
        }

        // Create a period from the new date/time.
        return new Period(newDt);
    }

    /// <summary>
    /// Evaluate the occurrences of this recurrence pattern.
    /// </summary>
    /// <param name="referenceDate">The reference date, i.e. DTSTART.</param>
    /// <param name="periodStart">Start (incl.) of the period occurrences are generated for.</param>
    /// <param name="periodEnd">End (excl.) of the period occurrences are generated for.</param>
    /// <param name="includeReferenceDateInResults">Whether the referenceDate itself should be returned. Ignored as the reference data MUST equal the first occurrence of an RRULE.</param>
    /// <returns></returns>
    public override IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd, bool includeReferenceDateInResults)
    {
        if (Pattern.Frequency != FrequencyType.None && Pattern.Frequency < FrequencyType.Daily && !referenceDate.HasTime)
        {
            // This case is not defined by RFC 5545. We handle it by evaluating the rule
            // as if referenceDate had a time (i.e. set to midnight).
            referenceDate = new CalDateTime(referenceDate.Date, new TimeOnly(), referenceDate.TzId);
        }

        // Create a recurrence pattern suitable for use during evaluation.
        var pattern = ProcessRecurrencePattern(referenceDate);

        var periodQuery = GetDates(referenceDate, periodStart, periodEnd, -1, pattern, includeReferenceDateInResults)
            .Select(dt => CreatePeriod(dt, referenceDate));

        if (pattern.Until is not null)
            periodQuery = periodQuery.TakeWhile(p => p.StartTime <= pattern.Until);

        return periodQuery;
    }
}
