using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DDay.iCal
{
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

            // If the frequency is weekly, and
            // no day of week is specified, use
            // the original date's day of week.
            // NOTE: fixes RRULE7 and RRULE8 handling
            if (r.Frequency == FrequencyType.Weekly &&
                r.ByDay.Count == 0)
                r.ByDay.Add(new WeekDay(referenceDate.DayOfWeek));
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
            // If neither BYDAY, BYMONTHDAY, or BYYEARDAY is specified,
            // default to the current day of month
            // NOTE: fixes RRULE23 handling, added BYYEARDAY exclusion
            // to fix RRULE25 handling
            if (r.Frequency > FrequencyType.Weekly &&
                r.ByMonthDay.Count == 0 &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0)
                r.ByMonthDay.Add(referenceDate.Day);
            // If neither BYMONTH nor BYYEARDAY is specified, default to
            // the current month
            // NOTE: fixes RRULE25 handling
            if (r.Frequency > FrequencyType.Monthly &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0 &&
                r.ByMonth.Count == 0)
                r.ByMonth.Add(referenceDate.Month);

            return r;
        }

        /**
         * Returns a list of start dates in the specified period represented by this recur. Any date fields not specified by
         * this recur are retained from the period start, and as such you should ensure the period start is initialised
         * correctly.
         * @param periodStart the start of the period
         * @param periodEnd the end of the period
         * @param value the type of dates to generate (i.e. date/date-time)
         * @return a list of dates
         */
        private List<DateTime> GetDates(DateTime periodStart, DateTime periodEnd, IRecurrencePattern pattern)
        {
            return GetDates(new iCalDateTime(periodStart), periodStart, periodEnd, -1, pattern);
        }

        /**
         * Convenience method for retrieving recurrences in a specified period.
         * @param seed a seed date for generating recurrence instances
         * @param period the period of returned recurrence dates
         * @param value type of dates to generate
         * @return a list of dates
         */
        private List<DateTime> GetDates(IDateTime seed, IPeriod period, IRecurrencePattern pattern)
        {
            return GetDates(seed, DateUtil.GetSimpleDateTimeData(period.StartTime), DateUtil.GetSimpleDateTimeData(period.EndTime), -1, pattern);
        }

        /**
         * Returns a list of start dates in the specified period represented by this recur. This method includes a base date
         * argument, which indicates the start of the fist occurrence of this recurrence. The base date is used to inject
         * default values to return a set of dates in the correct format. For example, if the search start date (start) is
         * Wed, Mar 23, 12:19PM, but the recurrence is Mon - Fri, 9:00AM - 5:00PM, the start dates returned should all be at
         * 9:00AM, and not 12:19PM.
         * @return a list of dates represented by this recur instance
         * @param seed the start date of this Recurrence's first instance
         * @param periodStart the start of the period
         * @param periodEnd the end of the period
         * @param value the type of dates to generate (i.e. date/date-time)
         */
        private List<DateTime> GetDates(IDateTime seed, DateTime periodStart, DateTime periodEnd, IRecurrencePattern pattern)
        {
             return GetDates(seed, periodStart, periodEnd, -1, pattern);
        }

        /**
         * Returns a list of start dates in the specified period represented by this recur. This method includes a base date
         * argument, which indicates the start of the fist occurrence of this recurrence. The base date is used to inject
         * default values to return a set of dates in the correct format. For example, if the search start date (start) is
         * Wed, Mar 23, 12:19PM, but the recurrence is Mon - Fri, 9:00AM - 5:00PM, the start dates returned should all be at
         * 9:00AM, and not 12:19PM.
         * @return a list of dates represented by this recur instance
         * @param seed the start date of this Recurrence's first instance
         * @param periodStart the start of the period
         * @param periodEnd the end of the period
         * @param value the type of dates to generate (i.e. date/date-time)
         * @param maxCount limits the number of instances returned. Up to one years
         *       worth extra may be returned. Less than 0 means no limit
         */
        private List<DateTime> GetDates(IDateTime seed, DateTime periodStart, DateTime periodEnd, int maxCount, IRecurrencePattern pattern)
        {            
            List<DateTime> dates = new List<DateTime>();

            DateTime seedCopy = DateUtil.GetSimpleDateTimeData(seed);
            // optimize the start time for selecting candidates
            // (only applicable where a COUNT is not specified)
            if (Pattern.Count == int.MinValue)
            {
                while (seedCopy < periodStart)
                    IncrementDate(ref seedCopy, pattern, pattern.Interval);
            }

            int invalidCandidateCount = 0;
            int noCandidateIncrementCount = 0;
            DateTime candidate = DateTime.MinValue;
            while ((maxCount < 0) || (dates.Count < maxCount))
            {
                if (pattern.Until != null && candidate != DateTime.MinValue && candidate > pattern.Until.Value)
                    break;

                if (periodEnd != null && candidate != DateTime.MinValue && candidate > periodEnd)
                    break;

                if (pattern.Count >= 1 && (dates.Count + invalidCandidateCount) >= pattern.Count) 
                    break;

                List<DateTime> candidates = GetCandidates(seedCopy, pattern);
                if (candidates.Count > 0)
                {
                    noCandidateIncrementCount = 0;

                    // sort candidates for identifying when UNTIL date is exceeded..
                    candidates.Sort();

                    for (int i = 0; i < candidates.Count; i++)
                    {
                        candidate = candidates[i];

                        // don't count candidates that occur before the seed date..
                        if (candidate >= seedCopy)
                        {
                            // candidates exclusive of periodEnd..
                            if (candidate < periodStart || candidate >= periodEnd)
                            {
                                invalidCandidateCount++;
                            } 
                            else if (pattern.Count >= 1 && (dates.Count + invalidCandidateCount) >= pattern.Count)
                            {
                                break;
                            }
                            else if (!(pattern.Until != null && candidate > pattern.Until.Value))
                            {
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
        
        /**
         * Returns the the next date of this recurrence given a seed date
         * and start date.  The seed date indicates the start of the fist 
         * occurrence of this recurrence. The start date is the
         * starting date to search for the next recurrence.  Return null
         * if there is no occurrence date after start date.
         * @return the next date in the recurrence series after startDate
         * @param seed the start date of this Recurrence's first instance
         * @param startDate the date to start the search
         */
        private DateTime? GetNextDate(IDateTime seed, DateTime startDate, IRecurrencePattern pattern)
        {            
            DateTime seedCopy = DateUtil.GetSimpleDateTimeData(seed);
            // optimize the start time for selecting candidates
            // (only applicable where a COUNT is not specified)
            if (Pattern.Count == int.MinValue)
            {
                while (seedCopy < startDate)
                    IncrementDate(ref seedCopy, pattern, pattern.Interval);
            }

            int invalidCandidateCount = 0;
            int noCandidateIncrementCount = 0;
            DateTime candidate = DateTime.MinValue;            
            
            while (true)
            {
                if (pattern.Until != null && candidate != DateTime.MinValue && candidate > pattern.Until.Value)
                    break;
                
                if (pattern.Count > 0 && invalidCandidateCount >= pattern.Count)
                    break;

                List<DateTime> candidates = GetCandidates(seedCopy, pattern);
                if (candidates.Count > 0)
                {
                    noCandidateIncrementCount = 0;

                    // sort candidates for identifying when UNTIL date is exceeded..
                    candidates.Sort();

                    for (int i = 0; i < candidates.Count; i++)
                    {
                        candidate = candidates[i];

                        // don't count candidates that occur before the seed date..
                        if (candidate >= seedCopy)
                        {
                            // Candidate must be after startDate because
                            // we want the NEXT occurrence
                            if (candidate >= startDate)
                            {
                                invalidCandidateCount++;
                            }
                            else if (pattern.Count > 0 && invalidCandidateCount >= pattern.Count)
                            {
                                break;
                            }
                            else if (!(pattern.Until != null && candidate > pattern.Until.Value))
                            {
                                return candidate;
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
            return null;
        }

        /**
         * Returns a list of possible dates generated from the applicable BY* rules, using the specified date as a seed.
         * @param date the seed date
         * @param value the type of date list to return
         * @return a DateList
         */
        private List<DateTime> GetCandidates(DateTime date, IRecurrencePattern pattern)
        {
            List<DateTime> dates = new List<DateTime>();
            dates.Add(date);
            dates = GetMonthVariants(dates, pattern);
            dates = GetWeekNoVariants(dates, pattern);
            dates = GetYearDayVariants(dates, pattern);
            dates = GetMonthDayVariants(dates, pattern);
            dates = GetDayVariants(dates, pattern);
            dates = GetHourVariants(dates, pattern);
            dates = GetMinuteVariants(dates, pattern);
            dates = GetSecondVariants(dates, pattern);
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
        private List<DateTime> GetMonthVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {            
            if (pattern.ByMonth.Count == 0)
                return dates;

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

        /**
         * Applies BYWEEKNO rules specified in this Recur instance to the specified date list. If no BYWEEKNO rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetWeekNoVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {            
            if (pattern.ByWeekNo.Count == 0)
                return dates;

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

        /**
         * Applies BYYEARDAY rules specified in this Recur instance to the specified date list. If no BYYEARDAY rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetYearDayVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {            
            if (pattern.ByYearDay.Count == 0)
                return dates;

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

        /**
         * Applies BYMONTHDAY rules specified in this Recur instance to the specified date list. If no BYMONTHDAY rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetMonthDayVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {            
            if (pattern.ByMonthDay.Count == 0)
                return dates;

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

        /**
         * Applies BYDAY rules specified in this Recur instance to the specified date list. If no BYDAY rules are specified
         * the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetDayVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {            
            if (pattern.ByDay.Count == 0)
                return dates;

            List<DateTime> weekDayDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < pattern.ByDay.Count; j++)
                {
                    IWeekDay weekDay = pattern.ByDay[j];
                    
                    // if BYYEARDAY or BYMONTHDAY is specified filter existing
                    // list..
                    if (pattern.ByYearDay.Count > 0 || pattern.ByMonthDay.Count > 0)
                    {
                        if (weekDay.Equals((WeekDay)date))
                            weekDayDates.Add(date);
                    }
                    else
                    {
                        weekDayDates.AddRange(GetAbsWeekDays(date, weekDay, pattern));
                    }
                }
            }
            return weekDayDates;
        }

        /**
         * Returns a list of applicable dates corresponding to the specified week day in accordance with the frequency
         * specified by this recurrence rule.
         * @param date
         * @param weekDay
         * @return
         */
        private List<DateTime> GetAbsWeekDays(DateTime date, IWeekDay weekDay, IRecurrencePattern pattern)
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
        private List<DateTime> GetHourVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {
            if (pattern.ByHour.Count == 0)
                return dates;

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

        /**
         * Applies BYMINUTE rules specified in this Recur instance to the specified date list. If no BYMINUTE rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetMinuteVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {
            if (pattern.ByMinute.Count == 0)
                return dates;
            
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

        /**
         * Applies BYSECOND rules specified in this Recur instance to the specified date list. If no BYSECOND rules are
         * specified the date list is returned unmodified.
         * @param dates
         * @return
         */
        private List<DateTime> GetSecondVariants(List<DateTime> dates, IRecurrencePattern pattern)
        {
            if (pattern.BySecond.Count == 0)
                return dates;

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

        #endregion

        #region Overrides

        public override IList<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd)
        {
            IRecurrencePattern pattern = ProcessRecurrencePattern(referenceDate);

            Periods.Clear();
            foreach (DateTime dt in GetDates(referenceDate, periodStart, periodEnd, pattern))
            {
                IDateTime newDt = new iCalDateTime(dt, referenceDate.TZID);
                newDt.AssociateWith(referenceDate);

                IPeriod p = new Period(newDt);
                if (!Periods.Contains(p))
                    Periods.Add(p);
            }

            return Periods;
        }

        #endregion
    }
}
