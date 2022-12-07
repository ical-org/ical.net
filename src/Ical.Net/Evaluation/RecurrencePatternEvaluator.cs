using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation
{
    /// <summary>
    /// Much of this code comes from iCal4j, as Ben Fortuna has done an
    /// excellent job with the recurrence pattern evaluation there.
    /// 
    /// Here's the iCal4j license:
    /// ==================
    ///  iCal4j - License
    ///  ==================
    ///  
    /// Copyright (c) 2009, Ben Fortuna
    /// All rights reserved.
    /// 
    /// Redistribution and use in source and binary forms, with or without
    /// modification, are permitted provided that the following conditions
    /// are met:
    /// 
    /// o Redistributions of source code must retain the above copyright
    /// notice, this list of conditions and the following disclaimer.
    /// 
    /// o Redistributions in binary form must reproduce the above copyright
    /// notice, this list of conditions and the following disclaimer in the
    /// documentation and/or other materials provided with the distribution.
    /// 
    /// o Neither the name of Ben Fortuna nor the names of any other contributors
    /// may be used to endorse or promote products derived from this software
    /// without specific prior written permission.
    /// 
    /// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
    /// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
    /// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
    /// A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
    /// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
    /// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
    /// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
    /// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
    /// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
    /// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    /// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    /// </summary>
    public class RecurrencePatternEvaluator : Evaluator
    {
        // FIXME: in ical4j this is configurable.
        const int _maxIncrementCount = 1000;

        protected RecurrencePattern Pattern { get; set; }

        public RecurrencePatternEvaluator(RecurrencePattern pattern)
        {
            Pattern = pattern;
        }

        RecurrencePattern ProcessRecurrencePattern(IDateTime referenceDate)
        {
            var r = new RecurrencePattern();
            r.CopyFrom(Pattern);

            // Convert the UNTIL value to one that matches the same time information as the reference date
            if (r.Until != DateTime.MinValue)
            {
                r.Until = referenceDate.MatchTimeZone(new CalDateTime(r.Until, referenceDate.TzId)).Value;
            }

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

        static void EnforceEvaluationRestrictions(RecurrencePattern pattern)
        {
            RecurrenceEvaluationModeType? evaluationMode = pattern.EvaluationMode;
            RecurrenceRestrictionType? evaluationRestriction = pattern.RestrictionType;

            if (evaluationRestriction == RecurrenceRestrictionType.NoRestriction)
            {
                return;
            }

            switch (evaluationMode)
            {
                case RecurrenceEvaluationModeType.AdjustAutomatically:
                    switch (pattern.Frequency)
                    {
                        case FrequencyType.Secondly:
                        {
                            switch (evaluationRestriction)
                            {
                                case RecurrenceRestrictionType.Default:
                                case RecurrenceRestrictionType.RestrictSecondly:
                                    pattern.Frequency = FrequencyType.Minutely;
                                    break;
                                case RecurrenceRestrictionType.RestrictMinutely:
                                    pattern.Frequency = FrequencyType.Hourly;
                                    break;
                                case RecurrenceRestrictionType.RestrictHourly:
                                    pattern.Frequency = FrequencyType.Daily;
                                    break;
                            }
                        }
                            break;
                        case FrequencyType.Minutely:
                        {
                            pattern.Frequency = evaluationRestriction switch
                            {
                                RecurrenceRestrictionType.RestrictMinutely => FrequencyType.Hourly,
                                RecurrenceRestrictionType.RestrictHourly => FrequencyType.Daily,
                                _ => pattern.Frequency
                            };
                        }
                            break;
                        case FrequencyType.Hourly:
                        {
                            pattern.Frequency = evaluationRestriction switch
                            {
                                RecurrenceRestrictionType.RestrictHourly => FrequencyType.Daily,
                                _ => pattern.Frequency
                            };
                        }
                            break;
                    }
                    break;
                case RecurrenceEvaluationModeType.ThrowException:
                case RecurrenceEvaluationModeType.Default:
                    switch (pattern.Frequency)
                    {
                        case FrequencyType.Secondly:
                        {
                            switch (evaluationRestriction)
                            {
                                case RecurrenceRestrictionType.Default:
                                case RecurrenceRestrictionType.RestrictSecondly:
                                case RecurrenceRestrictionType.RestrictMinutely:
                                case RecurrenceRestrictionType.RestrictHourly:
                                    throw new ArgumentException();
                            }
                        }
                            break;
                        case FrequencyType.Minutely:
                        {
                            switch (evaluationRestriction)
                            {
                                case RecurrenceRestrictionType.RestrictMinutely:
                                case RecurrenceRestrictionType.RestrictHourly:
                                    throw new ArgumentException();
                            }
                        }
                            break;
                        case FrequencyType.Hourly:
                        {
                            switch (evaluationRestriction)
                            {
                                case RecurrenceRestrictionType.RestrictHourly:
                                    throw new ArgumentException();
                            }
                        }
                            break;
                    }
                    break;
            }
        }

        /// <summary> Returns a list of <paramref name="maxCount"/> dates of this recurrence in the specified period. </summary>
        /// <remarks>
        /// This method includes a <see cref="seed"/> date argument,
        /// which indicates the start of the fist occurrence of this recurrence.
        /// The base date is used to inject default values to return a set of dates in the correct format.
        ///
        /// For example, if the search start date (start) is Wed, Mar 23, 12:19PM,
        /// but the recurrence is Mon - Fri, 9:00AM - 5:00PM,
        /// the start dates returned should all be at 9:00AM, and not 12:19PM.
        /// </remarks>
        HashSet<DateTime> GetDates(IDateTime seed, DateTime periodStart, DateTime periodEnd
            , int maxCount, RecurrencePattern pattern, bool includeReferenceDateInResults)
        {
            var dates = new HashSet<DateTime>();
            var originalDate = seed.GetSimpleDateTimeData();
            var seedCopy = seed.GetSimpleDateTimeData();

            if (includeReferenceDateInResults)
            {
                dates.Add(seedCopy);
            }

            // optimize the start time for selecting candidates
            // (only applicable where a COUNT is not specified)
            if (pattern.Count == int.MinValue)
            {
                var incremented = seedCopy;
                pattern.IncrementDate(ref incremented);
                while (incremented < periodStart)
                {
                    seedCopy = incremented;
                    pattern.IncrementDate(ref incremented);
                }
            }

            var expandBehavior = RecurrenceUtil.GetExpandBehaviorList(pattern);

            var noCandidateIncrementCount = 0;
            var candidate = DateTime.MinValue;
            while (maxCount < 0 || dates.Count < maxCount)
            {
                if (pattern.Until != DateTime.MinValue && candidate != DateTime.MinValue && candidate > pattern.Until)
                {
                    break;
                }

                if (candidate != DateTime.MinValue && candidate > periodEnd)
                {
                    break;
                }
                
                if (pattern.Count >= 1 && dates.Count >= pattern.Count)
                {
                    break;
                }

                var candidates = GetCandidates_(seedCopy, pattern, expandBehavior);
                if (candidates.Count > 0)
                {
                    noCandidateIncrementCount = 0;

                    foreach (var t in candidates.OrderBy(c => c).Where(t => t >= originalDate))
                    {
                        candidate = t;

                        // candidates MAY occur before periodStart
                        // For example, FREQ=YEARLY;BYWEEKNO=1 could return dates
                        // from the previous year.
                        //
                        // candidates exclusive of periodEnd..
                        if (pattern.Count >= 1 && dates.Count >= pattern.Count)
                        {
                            break;
                        }

                        if (candidate >= periodEnd)
                        {
                            continue;
                        }

                        if (pattern.Until == DateTime.MinValue || candidate <= pattern.Until)
                        {
                            dates.Add(candidate);
                        }
                    }
                }
                else
                {
                    noCandidateIncrementCount++;
                    if (_maxIncrementCount > 0 && noCandidateIncrementCount > _maxIncrementCount)
                    {
                        break;
                    }
                }

                pattern.IncrementDate(ref seedCopy);
            }

            return dates;
        }

        /// <summary> Returns a list of possible dates generated from the applicable BY* rules, using the <paramref name="seedDate"/>. </summary>
        List<DateTime> GetCandidates_(DateTime seedDate, RecurrencePattern pattern, IReadOnlyList<bool?> expandBehaviors)
        {
            var dates = new List<DateTime> {seedDate};
            dates = GetMonthVariants(dates, pattern, expandBehaviors[0]);
            dates = GetWeekNoVariants(dates, pattern, expandBehaviors[1]);
            dates = GetYearDayVariants(dates, pattern, expandBehaviors[2]);
            dates = GetMonthDayVariants(dates, pattern, expandBehaviors[3]);
            dates = GetDayVariants(dates, pattern, expandBehaviors[4]);
            dates = GetHourVariants(dates, pattern, expandBehaviors[5]);
            dates = GetMinuteVariants(dates, pattern, expandBehaviors[6]);
            dates = GetSecondVariants(dates, pattern, expandBehaviors[7]);
            dates = ApplySetPosRules(dates, pattern);
            return dates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.BySetPosition"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        /// <summary> Selects only the  positions from the <paramref name="dates"/> list. </summary>
        /// <remarks>
        /// Invalid positions are ignored. 
        /// </remarks>
        static List<DateTime> ApplySetPosRules(List<DateTime> dates, RecurrencePattern pattern)
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
                .Where(p => p > 0 && p <= size
                             || p < 0 && p >= -size)  //Protect against out of range access
                .Select(p => p > 0
                    ? dates[p - 1] //starting at 1! 
                    : dates[size + p])  //starting at -1!
                .ToList();
            return setPosDates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.ByMonth"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        static List<DateTime> GetMonthVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
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

        /// <summary> Applies <see cref="RecurrencePattern.ByWeekNo"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        List<DateTime> GetWeekNoVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
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
            foreach (var t in dates)
            {
                foreach (var weekNo in pattern.ByWeekNo)
                {
                    var date = t;
                    // Determine our current week number
                    var currWeekNo = Calendar.GetIso8601WeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);
                    while (currWeekNo > weekNo)
                    {
                        // If currWeekNo > weekNo, then we're likely at the start of a year
                        // where currWeekNo could be 52 or 53.  If we simply step ahead 7 days
                        // we should be back to week 1, where we can easily make the calculation
                        // to move to weekNo.
                        date = date.AddDays(7);
                        currWeekNo = Calendar.GetIso8601WeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);
                    }

                    // Move ahead to the correct week of the year
                    date = date.AddDays((weekNo - currWeekNo) * 7);

                    // Step backward single days until we're at the correct DayOfWeek
                    while (date.DayOfWeek != pattern.FirstDayOfWeek)
                    {
                        date = date.AddDays(-1);
                    }

                    for (var k = 0; k < 7; k++)
                    {
                        weekNoDates.Add(date);
                        date = date.AddDays(1);
                    }
                }
            }
            return weekNoDates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.ByYearDay"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        static List<DateTime> GetYearDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByYearDay.Count == 0)
            {
                return dates;
            }

            if (expand.Value)
            {
                var yearDayDates = new List<DateTime>(dates.Count);
                foreach (var date in dates)
                {
                    var date1 = date;
                    yearDayDates.AddRange(pattern.ByYearDay.Select(yearDay => yearDay > 0
                        ? date1.AddDays(-date1.DayOfYear + yearDay)
                        : date1.AddDays(-date1.DayOfYear + 1).AddYears(1).AddDays(yearDay)));
                }
                return yearDayDates;
            }
            // Limit behavior
            for (var i = dates.Count - 1; i >= 0; i--)
            {
                var date = dates[i];
                for (var j = 0; j < pattern.ByYearDay.Count; j++)
                {
                    var yearDay = pattern.ByYearDay[j];

                    var newDate = yearDay > 0
                        ? date.AddDays(-date.DayOfYear + yearDay)
                        : date.AddDays(-date.DayOfYear + 1).AddYears(1).AddDays(yearDay);

                    if (newDate.DayOfYear == date.DayOfYear)
                    {
                        goto Next;
                    }
                }

                dates.RemoveAt(i);
                Next:
                ;
            }

            return dates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.ByMonthDay"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        List<DateTime> GetMonthDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByMonthDay.Count == 0)
            {
                return dates;
            }

            if (expand.Value)
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
                return monthDayDates;
            }
            // Limit behavior
            for (var i = dates.Count - 1; i >= 0; i--)
            {
                var date = dates[i];
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
                        goto Next;
                    }
                }

                Next:
                dates.RemoveAt(i);
            }

            return dates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.ByDay"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        List<DateTime> GetDayVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByDay.Count == 0)
            {
                return dates;
            }

            if (expand.Value)
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

                return weekDayDates;
            }

            // Limit behavior
            for (var i = dates.Count - 1; i >= 0; i--)
            {
                var date = dates[i];
                for (var j = 0; j < pattern.ByDay.Count; j++)
                {
                    var weekDay = pattern.ByDay[j];
                    if (weekDay.DayOfWeek.Equals(date.DayOfWeek))
                    {
                        // If no offset is specified, simply test the day of week!
                        // FIXME: test with offset...
                        if (date.DayOfWeek.Equals(weekDay.DayOfWeek))
                        {
                            goto Next;
                        }
                    }
                }
                dates.RemoveAt(i);
                Next:
                ;
            }

            return dates;
        }

        /// <summary> Returns a list of times with <paramref name="pattern"/> frequency on the <paramref name="weekDay"/>. </summary>
        List<DateTime> GetAbsWeekDays(DateTime date, WeekDay weekDay, RecurrencePattern pattern)
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
                var weekNo = Calendar.GetIso8601WeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);

                // construct a list of possible week days..
                while (date.DayOfWeek != dayOfWeek)
                {
                    date = date.AddDays(1);
                }

                var nextWeekNo = Calendar.GetIso8601WeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);
                var currentWeekNo = Calendar.GetIso8601WeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);

                //When we manage weekly recurring pattern and we have boundary case:
                //Weekdays: Dec 31, Jan 1, Feb 1, Mar 1, Apr 1, May 1, June 1, Dec 31 - It's the 53th week of the year, but all another are 1st week number.
                //So we need an EXRULE for this situation, but only for weekly events
                while (currentWeekNo == weekNo || (nextWeekNo < weekNo && currentWeekNo == nextWeekNo && pattern.Frequency == FrequencyType.Weekly))
                {
                    if ((pattern.ByWeekNo.Count == 0 || pattern.ByWeekNo.Contains(currentWeekNo))
                        && (pattern.ByMonth.Count == 0 || pattern.ByMonth.Contains(date.Month)))
                    {
                        days.Add(date);
                    }

                    date = date.AddDays(7);
                    currentWeekNo = Calendar.GetIso8601WeekOfYear(date, CalendarWeekRule.FirstFourDayWeek,pattern.FirstDayOfWeek);
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

                while (date.Month == month)
                {
                    var currentWeekNo = Calendar.GetIso8601WeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);

                    if ((pattern.ByWeekNo.Count == 0 || pattern.ByWeekNo.Contains(currentWeekNo))
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
            return GetOffsetDate(days, weekDay.Offset);
        }

        /// <summary> Returns a single-element sublist containing <paramref name="dates"/>[<paramref name="offset"/>-1]. </summary>
        /// <remarks>
        /// Valid <paramref name="offset"/> are from -<paramref name="dates"/>.Length to <paramref name="dates"/>.Length.
        /// </remarks>
        /// <returns> an empty list, when an invalid offset is supplied. </returns>
        static List<DateTime> GetOffsetDate(List<DateTime> dates, int offset)
        {
            if (offset == int.MinValue)
            {
                return dates;
            }

            var offsetDates = new List<DateTime>();
            var size = dates.Count;
            if (offset < 0 && offset >= -size)
            {
                offsetDates.Add(dates[size + offset]);
            }
            else if (offset > 0 && offset <= size)
            {
                offsetDates.Add(dates[offset - 1]);
            }
            return offsetDates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.ByHour"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        static List<DateTime> GetHourVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
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
                for (var j = 0; j < pattern.ByHour.Count; j++)
                {
                    var hour = pattern.ByHour[j];
                    if (date.Hour == hour)
                    {
                        goto Next;
                    }
                }
                // Remove unmatched dates
                dates.RemoveAt(i);
                Next:
                ;
            }
            return dates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.ByMinute"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        static List<DateTime> GetMinuteVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
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
                for (var j = 0; j < pattern.ByMinute.Count; j++)
                {
                    var minute = pattern.ByMinute[j];
                    if (date.Minute == minute)
                    {
                        goto Next;
                    }
                }
                // Remove unmatched dates
                dates.RemoveAt(i);
                Next:
                ;
            }
            return dates;
        }

        /// <summary> Applies <see cref="RecurrencePattern.BySecond"/> rules specified in the <paramref name="pattern"/> to the <paramref name="dates"/> list. </summary>
        /// <returns>the full <paramref name="dates"/> when no Rules were specified. </returns>
        static List<DateTime> GetSecondVariants(List<DateTime> dates, RecurrencePattern pattern, bool? expand)
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
                for (var j = 0; j < pattern.BySecond.Count; j++)
                {
                    var second = pattern.BySecond[j];
                    if (date.Second == second)
                    {
                        goto Next;
                    }
                }
                // Remove unmatched dates
                dates.RemoveAt(i);
                Next:
                ;
            }
            return dates;
        }

        /// <summary> Create a period with unspecified Duration from the <paramref name="startDateTime"/>. </summary>
        static Period CreatePeriod(DateTime startDateTime, IDateTime referenceDate)
        {
            // Turn each resulting date/time into an IDateTime and associate it
            // with the reference date.
            IDateTime newDt = new CalDateTime(startDateTime, referenceDate.TzId);

            // NOTE: fixes bug #2938007 - hasTime missing
            newDt.HasTime = referenceDate.HasTime;

            newDt.AssociateWith(referenceDate);

            return new Period(newDt);
        }

        public override HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Create a recurrence pattern suitable for use during evaluation.
            var pattern = ProcessRecurrencePattern(referenceDate);

            // Enforce evaluation restrictions on the pattern.
            EnforceEvaluationRestrictions(pattern);
            Periods.Clear();

            var periodQuery = GetDates(referenceDate, periodStart, periodEnd, -1, pattern, includeReferenceDateInResults)
                .Select(dt => CreatePeriod(dt, referenceDate));

            Periods.UnionWith(periodQuery);

            return Periods;
        }
    }
}