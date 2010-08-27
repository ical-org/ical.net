using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DDay.iCal
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

    public class RecurrencePatternEvaluator :
        Evaluator
    {
        // FIXME: in ical4j this is configurable.
        private static int maxIncrementCount = 1000;

        #region Protected Properties

        protected IRecurrencePattern Pattern { get; set; }

        #endregion

        #region Constructors

        public RecurrencePatternEvaluator(IRecurrencePattern pattern)
	    {
            Pattern = pattern;
	    }

        #endregion

        #region Private Methods

        private IRecurrencePattern ProcessRecurrencePattern(IDateTime referenceDate)
        {
            RecurrencePattern r = new RecurrencePattern();
            r.CopyFrom(Pattern);

            // Convert the UNTIL value to one that matches the same time information as the reference date
            if (r.Until != DateTime.MinValue)
                r.Until = DateUtil.MatchTimeZone(referenceDate, new iCalDateTime(r.Until)).Value;

            if (r.Frequency > FrequencyType.Secondly &&
                r.BySecond.Count == 0 &&
                referenceDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.BySecond.Add(referenceDate.Second);
            if (r.Frequency > FrequencyType.Minutely &&
                r.ByMinute.Count == 0 &&
                referenceDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByMinute.Add(referenceDate.Minute);
            if (r.Frequency > FrequencyType.Hourly &&
                r.ByHour.Count == 0 &&
                referenceDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByHour.Add(referenceDate.Hour);

            // If BYDAY, BYYEARDAY, or BYWEEKNO is specified, then
            // we don't default BYDAY, BYMONTH or BYMONTHDAY
            if (r.ByDay.Count == 0)
            {
                // If the frequency is weekly, use the original date's day of week.
                // NOTE: fixes WeeklyCount1() and WeeklyUntil1() handling
                // If BYWEEKNO is specified and BYMONTHDAY/BYYEARDAY is not specified,
                // then let's add BYDAY to BYWEEKNO.
                // NOTE: fixes YearlyByWeekNoX() handling
                if (r.Frequency == FrequencyType.Weekly ||
                    (
                        r.ByWeekNo.Count > 0 &&
                        r.ByMonthDay.Count == 0 &&
                        r.ByYearDay.Count == 0
                    ))
                    r.ByDay.Add(new WeekDay(referenceDate.DayOfWeek));

                // If BYMONTHDAY is not specified,
                // default to the current day of month.
                // NOTE: fixes YearlyByMonth1() handling, added BYYEARDAY exclusion
                // to fix YearlyCountByYearDay1() handling
                if (r.Frequency > FrequencyType.Weekly &&
                    r.ByWeekNo.Count == 0 &&
                    r.ByYearDay.Count == 0 &&
                    r.ByMonthDay.Count == 0)
                    r.ByMonthDay.Add(referenceDate.Day);

                // If BYMONTH is not specified, default to
                // the current month.
                // NOTE: fixes YearlyCountByYearDay1() handling
                if (r.Frequency > FrequencyType.Monthly &&
                    r.ByWeekNo.Count == 0 &&
                    r.ByYearDay.Count == 0 &&
                    r.ByMonth.Count == 0)
                    r.ByMonth.Add(referenceDate.Month);
            }

            return r;
        }

        private void EnforceEvaluationRestrictions(IRecurrencePattern pattern)
        {
            RecurrenceEvaluationModeType? evaluationMode = pattern.EvaluationMode;
            RecurrenceRestrictionType? evaluationRestriction = pattern.RestrictionType;

            if (evaluationRestriction != RecurrenceRestrictionType.NoRestriction)
            {
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
                                        case RecurrenceRestrictionType.RestrictSecondly: pattern.Frequency = FrequencyType.Minutely; break;
                                        case RecurrenceRestrictionType.RestrictMinutely: pattern.Frequency = FrequencyType.Hourly; break;
                                        case RecurrenceRestrictionType.RestrictHourly: pattern.Frequency = FrequencyType.Daily; break;
                                    }
                                } break;
                            case FrequencyType.Minutely:
                                {
                                    switch (evaluationRestriction)
                                    {
                                        case RecurrenceRestrictionType.RestrictMinutely: pattern.Frequency = FrequencyType.Hourly; break;
                                        case RecurrenceRestrictionType.RestrictHourly: pattern.Frequency = FrequencyType.Daily; break;
                                    }
                                } break;
                            case FrequencyType.Hourly:
                                {
                                    switch (evaluationRestriction)
                                    {
                                        case RecurrenceRestrictionType.RestrictHourly: pattern.Frequency = FrequencyType.Daily; break;
                                    }
                                } break;
                            default: break;
                        } break;
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
                                            throw new EvaluationEngineException();
                                    }
                                } break;
                            case FrequencyType.Minutely:
                                {
                                    switch (evaluationRestriction)
                                    {
                                        case RecurrenceRestrictionType.RestrictMinutely:
                                        case RecurrenceRestrictionType.RestrictHourly:
                                            throw new EvaluationEngineException();
                                    }
                                } break;
                            case FrequencyType.Hourly:
                                {
                                    switch (evaluationRestriction)
                                    {
                                        case RecurrenceRestrictionType.RestrictHourly:
                                            throw new EvaluationEngineException();
                                    }
                                } break;
                            default: break;
                        } break;
                }
            }
        }

        /**
         * Returns a list of start dates in the specified period represented by this recur. This method includes a base date
         * argument, which indicates the start of the fist occurrence of this recurrence. The base date is used to inject
         * default values to return a set of dates in the correct format. For example, if the search start date (start) is
         * Wed, Mar 23, 12:19PM, but the recurrence is Mon - Fri, 9:00AM - 5:00PM, the start dates returned should all be at
         * 9:00AM, and not 12:19PM.
         */
        private List<DateTime> GetDates(IDateTime seed, DateTime periodStart, DateTime periodEnd, int maxCount, IRecurrencePattern pattern, bool includeReferenceDateInResults)
        {            
            List<DateTime> dates = new List<DateTime>();
            DateTime originalDate = DateUtil.GetSimpleDateTimeData(seed);
            DateTime seedCopy = DateUtil.GetSimpleDateTimeData(seed);

            if (includeReferenceDateInResults)
                dates.Add(seedCopy);

            // optimize the start time for selecting candidates
            // (only applicable where a COUNT is not specified)
            if (pattern.Count == int.MinValue)
            {
                DateTime incremented = seedCopy;
                IncrementDate(ref incremented, pattern, pattern.Interval);
                while (incremented < periodStart)
                {
                    seedCopy = incremented;
                    IncrementDate(ref incremented, pattern, pattern.Interval);
                }
            }
                        
            bool?[] expandBehavior = RecurrenceUtil.GetExpandBehaviorList(pattern);

            int invalidCandidateCount = 0;
            int noCandidateIncrementCount = 0;
            DateTime candidate = DateTime.MinValue;
            while ((maxCount < 0) || (dates.Count < maxCount))
            {
                if (pattern.Until != DateTime.MinValue && candidate != DateTime.MinValue && candidate > pattern.Until)
                    break;

                if (periodEnd != null && candidate != DateTime.MinValue && candidate > periodEnd)
                    break;
                                
                if (pattern.Count >= 1 && (dates.Count + invalidCandidateCount) >= pattern.Count)
                    break;                

                List<DateTime> candidates = GetCandidates(seedCopy, pattern, expandBehavior);
                if (candidates.Count > 0)
                {
                    noCandidateIncrementCount = 0;

                    // sort candidates for identifying when UNTIL date is exceeded..
                    candidates.Sort();

                    for (int i = 0; i < candidates.Count; i++)
                    {
                        candidate = candidates[i];

                        // don't count candidates that occur before the original date..
                        if (candidate >= originalDate)
                        {
                            // candidates MAY occur before periodStart
                            // For example, FREQ=YEARLY;BYWEEKNO=1 could return dates
                            // from the previous year.
                            //
                            // candidates exclusive of periodEnd..
                            if (candidate >= periodEnd)
                            {
                                invalidCandidateCount++;
                            }
                            else if (pattern.Count >= 1 && (dates.Count + invalidCandidateCount) >= pattern.Count)
                            {
                                break;
                            }
                            else if (pattern.Until == DateTime.MinValue || candidate <= pattern.Until)
                            {
                                if (!dates.Contains(candidate))
                                    dates.Add(candidate);                                
                            }
                        }
                    }
                } 
                else
                {
                    noCandidateIncrementCount++;
                    if ((maxIncrementCount > 0) && (noCandidateIncrementCount > maxIncrementCount))
                        break;
                }

                IncrementDate(ref seedCopy, pattern, pattern.Interval);
            }

            // sort final list..
            dates.Sort();
            return dates;
        }
        
        ///**
        // * Returns the the next date of this recurrence given a seed date
        // * and start date.  The seed date indicates the start of the fist 
        // * occurrence of this recurrence. The start date is the
        // * starting date to search for the next recurrence.  Return null
        // * if there is no occurrence date after start date.
        // * @return the next date in the recurrence series after startDate
        // * @param seed the start date of this Recurrence's first instance
        // * @param startDate the date to start the search
        // */
        //private DateTime? GetNextDate(IDateTime referenceDate, DateTime periodStart, IRecurrencePattern pattern)
        //{            
        //    DateTime seedCopy = DateUtil.GetSimpleDateTimeData(referenceDate);
        //    // optimize the start time for selecting candidates
        //    // (only applicable where a COUNT is not specified)
        //    if (Pattern.Count == int.MinValue)
        //    {
        //        DateTime incremented = seedCopy;
        //        IncrementDate(ref incremented, pattern, pattern.Interval);
        //        while (incremented < periodStart)
        //        {
        //            seedCopy = incremented;
        //            IncrementDate(ref incremented, pattern, pattern.Interval);
        //        }
        //    }
                        
        //    bool?[] expandBehaviors = RecurrenceUtil.GetExpandBehaviorList(pattern);

        //    int invalidCandidateCount = 0;
        //    int noCandidateIncrementCount = 0;
        //    DateTime candidate = DateTime.MinValue;            
            
        //    while (true)
        //    {
        //        if (pattern.Until != DateTime.MinValue && candidate != DateTime.MinValue && candidate > pattern.Until)
        //            break;

        //        if (pattern.Count > 0 && invalidCandidateCount >= pattern.Count)
        //            break;

        //        List<DateTime> candidates = GetCandidates(seedCopy, pattern, expandBehaviors);
        //        if (candidates.Count > 0)
        //        {
        //            noCandidateIncrementCount = 0;

        //            // sort candidates for identifying when UNTIL date is exceeded..
        //            candidates.Sort();

        //            for (int i = 0; i < candidates.Count; i++)
        //            {
        //                candidate = candidates[i];

        //                // don't count candidates that occur before the seed date..
        //                if (candidate >= seedCopy)
        //                {
        //                    // Candidate must be after startDate because
        //                    // we want the NEXT occurrence
        //                    if (candidate >= periodStart)
        //                    {
        //                        invalidCandidateCount++;
        //                    }
        //                    else if (pattern.Count > 0 && invalidCandidateCount >= pattern.Count)
        //                    {
        //                        break;
        //                    }
        //                    else if (pattern.Until == DateTime.MinValue || candidate <= pattern.Until)
        //                    {
        //                        return candidate;
        //                    }
        //                }
        //            }
        //        } 
        //        else 
        //        {
        //            noCandidateIncrementCount++;
        //            if ((maxIncrementCount > 0) && (noCandidateIncrementCount > maxIncrementCount)) 
        //                break;
        //        }

        //        IncrementDate(ref seedCopy, pattern, pattern.Interval);
        //    }
        //    return null;
        //}

        /**
         * Returns a list of possible dates generated from the applicable BY* rules, using the specified date as a seed.
         * @param date the seed date
         * @param value the type of date list to return
         * @return a DateList
         */
        private List<DateTime> GetCandidates(DateTime date, IRecurrencePattern pattern, bool?[] expandBehaviors)
        {
            List<DateTime> dates = new List<DateTime>();
            dates.Add(date);
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

        /**
         * Applies BYSETPOS rules to <code>dates</code>. Valid positions are from 1 to the size of the date list. Invalid
         * positions are ignored.
         * @param dates
         */
        private List<DateTime> ApplySetPosRules(List<DateTime> dates, IRecurrencePattern pattern)
        {
            // return if no SETPOS rules specified..
            if (pattern.BySetPosition.Count == 0)
                return dates;

            // sort the list before processing..
            dates.Sort();

            List<DateTime> setPosDates = new List<DateTime>();
            int size = dates.Count;

            for (int i = 0; i < pattern.BySetPosition.Count; i++)
            {
                int pos = pattern.BySetPosition[i];
                if (pos > 0 && pos <= size)
                {
                    setPosDates.Add(dates[pos - 1]);
                }
                else if (pos < 0 && pos >= -size)
                {
                    setPosDates.Add(dates[size + pos]);
                }
            }
            return setPosDates;
        }

        /**
         * Applies BYMONTH rules specified in this Recur instance to the specified date list. If no BYMONTH rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetMonthVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByMonth.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> monthlyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonth.Count; j++)
                    {
                        int month = pattern.ByMonth[j];
                        date = date.AddMonths(month - date.Month);
                        monthlyDates.Add(date);
                    }
                }
                return monthlyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonth.Count; j++)
                    {
                        if (date.Month == pattern.ByMonth[j])
                            goto Next;
                    }
                    dates.RemoveAt(i);
                Next: ;
                }
                return dates;
            }
        }

        /**
         * Applies BYWEEKNO rules specified in this Recur instance to the specified date list. If no BYWEEKNO rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetWeekNoVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByWeekNo.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> weekNoDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByWeekNo.Count; j++)
                    {
                        // Determine our target week number
                        int weekNo = pattern.ByWeekNo[j];

                        // Determine our current week number
                        int currWeekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);
                        while (currWeekNo > weekNo)
                        {
                            // If currWeekNo > weekNo, then we're likely at the start of a year
                            // where currWeekNo could be 52 or 53.  If we simply step ahead 7 days
                            // we should be back to week 1, where we can easily make the calculation
                            // to move to weekNo.
                            date = date.AddDays(7);
                            currWeekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);
                        }

                        // Move ahead to the correct week of the year
                        date = date.AddDays((weekNo - currWeekNo) * 7);

                        // Step backward single days until we're at the correct DayOfWeek
                        while (date.DayOfWeek != pattern.FirstDayOfWeek)
                            date = date.AddDays(-1);

                        for (int k = 0; k < 7; k++)
                        {
                            weekNoDates.Add(date);
                            date = date.AddDays(1);
                        }
                    }
                }
                return weekNoDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByWeekNo.Count; j++)
                    {
                        // Determine our target week number
                        int weekNo = pattern.ByWeekNo[j];

                        // Determine our current week number
                        int currWeekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);

                        if (weekNo == currWeekNo)
                            goto Next;
                    }

                    dates.RemoveAt(i);
                Next: ;
                }
                return dates;
            }
        }

        /**
         * Applies BYYEARDAY rules specified in this Recur instance to the specified date list. If no BYYEARDAY rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetYearDayVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByYearDay.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> yearDayDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByYearDay.Count; j++)
                    {
                        int yearDay = pattern.ByYearDay[j];

                        DateTime newDate;
                        if (yearDay > 0)
                            newDate = date.AddDays(-date.DayOfYear + yearDay);
                        else
                            newDate = date.AddDays(-date.DayOfYear + 1).AddYears(1).AddDays(yearDay);

                        yearDayDates.Add(newDate);
                    }
                }
                return yearDayDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByYearDay.Count; j++)
                    {
                        int yearDay = pattern.ByYearDay[j];

                        DateTime newDate;
                        if (yearDay > 0)
                            newDate = date.AddDays(-date.DayOfYear + yearDay);
                        else
                            newDate = date.AddDays(-date.DayOfYear + 1).AddYears(1).AddDays(yearDay);

                        if (newDate.DayOfYear == date.DayOfYear)
                            goto Next;
                    }

                    dates.RemoveAt(i);
                Next: ;                    
                }

                return dates;
            }
        }

        /**
         * Applies BYMONTHDAY rules specified in this Recur instance to the specified date list. If no BYMONTHDAY rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetMonthDayVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByMonthDay.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> monthDayDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonthDay.Count; j++)
                    {
                        int monthDay = pattern.ByMonthDay[j];

                        int daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
                        if (Math.Abs(monthDay) > daysInMonth)
                            throw new ArgumentException("Invalid day of month: " + date + " (day " + monthDay + ")");

                        // Account for positive or negative numbers
                        DateTime newDate;
                        if (monthDay > 0)
                            newDate = date.AddDays(-date.Day + monthDay);
                        else
                            newDate = date.AddDays(-date.Day + 1).AddMonths(1).AddDays(monthDay);

                        monthDayDates.Add(newDate);
                    }
                }
                return monthDayDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMonthDay.Count; j++)
                    {
                        int monthDay = pattern.ByMonthDay[j];

                        int daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
                        if (Math.Abs(monthDay) > daysInMonth)
                            throw new ArgumentException("Invalid day of month: " + date + " (day " + monthDay + ")");

                        // Account for positive or negative numbers
                        DateTime newDate;
                        if (monthDay > 0)
                            newDate = date.AddDays(-date.Day + monthDay);
                        else
                            newDate = date.AddDays(-date.Day + 1).AddMonths(1).AddDays(monthDay);

                        if (newDate.Day.Equals(date.Day))
                            goto Next;
                    }

                Next: ;
                    dates.RemoveAt(i);
                }                
            
                return dates;
            }
        }

        /**
         * Applies BYDAY rules specified in this Recur instance to the specified date list. If no BYDAY rules are specified
         * the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetDayVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByDay.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> weekDayDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByDay.Count; j++)
                    {
                        weekDayDates.AddRange(GetAbsWeekDays(date, pattern.ByDay[j], pattern, expand));
                    }
                }

                return weekDayDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByDay.Count; j++)
                    {
                        IWeekDay weekDay = pattern.ByDay[j];
                        if (weekDay.DayOfWeek.Equals(date.DayOfWeek))
                        {
                            // If no offset is specified, simply test the day of week!
                            // FIXME: test with offset...
                            if (date.DayOfWeek.Equals(weekDay.DayOfWeek))
                                goto Next;
                        }
                    }
                    dates.RemoveAt(i);
                Next: ;
                }

                return dates;
            }
        }

        /**
         * Returns a list of applicable dates corresponding to the specified week day in accordance with the frequency
         * specified by this recurrence rule.
         * @param date
         * @param weekDay
         * @return
         */
        private List<DateTime> GetAbsWeekDays(DateTime date, IWeekDay weekDay, IRecurrencePattern pattern, bool? expand)
        {
            List<DateTime> days = new List<DateTime>();

            DayOfWeek dayOfWeek = weekDay.DayOfWeek;
            if (pattern.Frequency == FrequencyType.Daily)
            {
                if (date.DayOfWeek == dayOfWeek)
                    days.Add(date);
            }
            else if (pattern.Frequency == FrequencyType.Weekly || pattern.ByWeekNo.Count > 0)
            {
                int weekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek);

                // construct a list of possible week days..
                while (date.DayOfWeek != dayOfWeek)
                    date = date.AddDays(1);
                
                while (Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, pattern.FirstDayOfWeek) == weekNo)
                {
                    days.Add(date);
                    date = date.AddDays(7);
                }
            }
            else if (pattern.Frequency == FrequencyType.Monthly || pattern.ByMonth.Count > 0)
            {
                int month = date.Month;

                // construct a list of possible month days..
                date = date.AddDays(-date.Day + 1);
                while (date.DayOfWeek != dayOfWeek)
                    date = date.AddDays(1);

                while (date.Month == month)
                {
                    days.Add(date);
                    date = date.AddDays(7);
                }
            }
            else if (pattern.Frequency == FrequencyType.Yearly)
            {
                int year = date.Year;
                
                // construct a list of possible year days..
                date = date.AddDays(-date.DayOfYear + 1);
                while (date.DayOfWeek != dayOfWeek)
                    date = date.AddDays(1);

                while (date.Year == year)
                {
                    days.Add(date);
                    date = date.AddDays(7);
                }
            }
            return GetOffsetDates(days, weekDay.Offset);
        }

        /**
         * Returns a single-element sublist containing the element of <code>list</code> at <code>offset</code>. Valid
         * offsets are from 1 to the size of the list. If an invalid offset is supplied, all elements from <code>list</code>
         * are added to <code>sublist</code>.
         * @param list
         * @param offset
         * @param sublist
         */
        private List<DateTime> GetOffsetDates(List<DateTime> dates, int offset)
        {
            if (offset == int.MinValue) 
                return dates;
            
            List<DateTime> offsetDates = new List<DateTime>();
            int size = dates.Count;
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

        /**
         * Applies BYHOUR rules specified in this Recur instance to the specified date list. If no BYHOUR rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetHourVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByHour.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> hourlyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByHour.Count; j++)
                    {
                        int hour = pattern.ByHour[j];
                        date = date.AddHours(-date.Hour + hour);
                        hourlyDates.Add(date);
                    }
                }
                return hourlyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByHour.Count; j++)
                    {
                        int hour = pattern.ByHour[j];
                        if (date.Hour == hour)
                            goto Next;
                    }
                    // Remove unmatched dates
                    dates.RemoveAt(i);
                Next: ;
                }
                return dates;
            }
        }

        /**
         * Applies BYMINUTE rules specified in this Recur instance to the specified date list. If no BYMINUTE rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetMinuteVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.ByMinute.Count == 0)
                return dates;

            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> minutelyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMinute.Count; j++)
                    {
                        int minute = pattern.ByMinute[j];
                        date = date.AddMinutes(-date.Minute + minute);
                        minutelyDates.Add(date);
                    }
                }
                return minutelyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.ByMinute.Count; j++)
                    {
                        int minute = pattern.ByMinute[j];
                        if (date.Minute == minute)
                            goto Next;
                    }
                    // Remove unmatched dates
                    dates.RemoveAt(i);
                Next: ;
                }
                return dates;
            }
        }

        /**
         * Applies BYSECOND rules specified in this Recur instance to the specified date list. If no BYSECOND rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetSecondVariants(List<DateTime> dates, IRecurrencePattern pattern, bool? expand)
        {
            if (expand == null || pattern.BySecond.Count == 0)
                return dates;
                        
            if (expand.HasValue && expand.Value)
            {
                // Expand behavior
                List<DateTime> secondlyDates = new List<DateTime>();
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.BySecond.Count; j++)
                    {
                        int second = pattern.BySecond[j];
                        date = date.AddSeconds(-date.Second + second);
                        secondlyDates.Add(date);
                    }
                }
                return secondlyDates;
            }
            else
            {
                // Limit behavior
                for (int i = dates.Count - 1; i >= 0; i--)
                {
                    DateTime date = dates[i];
                    for (int j = 0; j < pattern.BySecond.Count; j++)
                    {
                        int second = pattern.BySecond[j];
                        if (date.Second == second)
                            goto Next;
                    }
                    // Remove unmatched dates
                    dates.RemoveAt(i);
                Next: ;
                }
                return dates;
            }            
        }

        #endregion

        #region Private Methods

        IPeriod CreatePeriod(DateTime dt, IDateTime referenceDate)
        {
            // Turn each resulting date/time into an IDateTime and associate it
            // with the reference date.
            IDateTime newDt = new iCalDateTime(dt, referenceDate.TZID);

            // NOTE: fixes bug #2938007 - hasTime missing
            newDt.HasTime = referenceDate.HasTime;

            newDt.AssociateWith(referenceDate);

            // Create a period from the new date/time.
            return new Period(newDt);
        }

        #endregion

        #region Public Methods

        //virtual public IPeriod GetNext(IDateTime referenceDate)
        //{
        //    DateTime? dt = GetNextDate(referenceDate, referenceDate.Value, Pattern);
        //    if (dt != null)
        //    {
        //        // Create a period from the date/time.
        //        IPeriod p = CreatePeriod(dt.Value, referenceDate);

        //        if (!Periods.Contains(p))
        //            Periods.Add(p);
        //    }
        //    return null;
        //}

        #endregion

        #region Overrides

        public override IList<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            // Create a recurrence pattern suitable for use during evaluation.
            IRecurrencePattern pattern = ProcessRecurrencePattern(referenceDate);

            // Enforce evaluation restrictions on the pattern.
            EnforceEvaluationRestrictions(pattern);

            Periods.Clear();
            foreach (DateTime dt in GetDates(referenceDate, periodStart, periodEnd, -1, pattern, includeReferenceDateInResults))
            {                
                // Create a period from the date/time.
                IPeriod p = CreatePeriod(dt, referenceDate);
                
                if (!Periods.Contains(p))
                    Periods.Add(p);
            }

            return Periods;
        }

        #endregion
    }
}
