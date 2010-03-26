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

        /**
         * Returns a list of start dates in the specified period represented by this recur. Any date fields not specified by
         * this recur are retained from the period start, and as such you should ensure the period start is initialised
         * correctly.
         * @param periodStart the start of the period
         * @param periodEnd the end of the period
         * @param value the type of dates to generate (i.e. date/date-time)
         * @return a list of dates
         */
        private List<DateTime> GetDates(DateTime startDate, DateTime endDate)
        {
            return GetDates(new iCalDateTime(startDate), startDate, endDate, -1);
        }

        /**
         * Convenience method for retrieving recurrences in a specified period.
         * @param seed a seed date for generating recurrence instances
         * @param period the period of returned recurrence dates
         * @param value type of dates to generate
         * @return a list of dates
         */
        private List<DateTime> GetDates(IDateTime seed, IPeriod period)
        {
            return GetDates(seed, DateUtil.GetSimpleDateTimeData(period.StartTime), DateUtil.GetSimpleDateTimeData(period.EndTime), -1);
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
        private List<DateTime> GetDates(IDateTime seed, DateTime startDate, DateTime endDate)
        {
             return GetDates(seed, startDate, endDate, -1);
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
        private List<DateTime> GetDates(IDateTime seed, DateTime periodStart, DateTime periodEnd, int maxCount)
        {
            IRecurrencePattern r = Pattern;
            List<DateTime> dates = new List<DateTime>();

            DateTime seedCopy = DateUtil.GetSimpleDateTimeData(seed);
            // optimize the start time for selecting candidates
            // (only applicable where a COUNT is not specified)
            if (Pattern.Count == int.MinValue)
            {
                while (seedCopy < periodStart)
                    IncrementDate(ref seedCopy, r, r.Interval);
            }

            int invalidCandidateCount = 0;
            int noCandidateIncrementCount = 0;
            DateTime candidate = DateTime.MinValue;
            while ((maxCount < 0) || (dates.Count < maxCount))
            {
                if (r.Until != null && candidate != DateTime.MinValue && candidate > r.Until.Value)
                    break;

                if (periodEnd != null && candidate != DateTime.MinValue && candidate > periodEnd)
                    break;

                if (r.Count >= 1 && (dates.Count + invalidCandidateCount) >= r.Count) 
                    break;

                List<DateTime> candidates = GetCandidates(seedCopy);
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
                            else if (r.Count >= 1 && (dates.Count + invalidCandidateCount) >= r.Count)
                            {
                                break;
                            }
                            else if (!(r.Until != null && candidate > r.Until.Value))
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

                IncrementDate(ref seedCopy, r, r.Interval);
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
        private DateTime? GetNextDate(IDateTime seed, DateTime startDate)
        {
            IRecurrencePattern r = Pattern;
            DateTime seedCopy = DateUtil.GetSimpleDateTimeData(seed);
            // optimize the start time for selecting candidates
            // (only applicable where a COUNT is not specified)
            if (Pattern.Count == int.MinValue)
            {
                while (seedCopy < startDate)
                    IncrementDate(ref seedCopy, r, r.Interval);
            }

            int invalidCandidateCount = 0;
            int noCandidateIncrementCount = 0;
            DateTime candidate = DateTime.MinValue;            
            
            while (true)
            {
                if (r.Until != null && candidate != DateTime.MinValue && candidate > r.Until.Value)
                    break;
                
                if (r.Count > 0 && invalidCandidateCount >= r.Count)
                    break;

                List<DateTime> candidates = GetCandidates(seedCopy);
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
                            else if (r.Count > 0 && invalidCandidateCount >= r.Count)
                            {
                                break;
                            }
                            else if (!(r.Until != null && candidate > r.Until.Value))
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

                IncrementDate(ref seedCopy, r, r.Interval);
            }
            return null;
        }

        /**
         * Returns a list of possible dates generated from the applicable BY* rules, using the specified date as a seed.
         * @param date the seed date
         * @param value the type of date list to return
         * @return a DateList
         */
        private List<DateTime> GetCandidates(DateTime date)
        {
            List<DateTime> dates = new List<DateTime>();
            dates.Add(date);
            dates = GetMonthVariants(dates);
            dates = GetWeekNoVariants(dates);
            dates = GetYearDayVariants(dates);
            dates = GetMonthDayVariants(dates);
            dates = GetDayVariants(dates);
            dates = GetHourVariants(dates);
            dates = GetMinuteVariants(dates);
            dates = GetSecondVariants(dates);
            dates = ApplySetPosRules(dates);
            return dates;
        }

        /**
         * Applies BYSETPOS rules to <code>dates</code>. Valid positions are from 1 to the size of the date list. Invalid
         * positions are ignored.
         * @param dates
         */
        private List<DateTime> ApplySetPosRules(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;

            // return if no SETPOS rules specified..
            if (r.BySetPosition.Count == 0)
                return dates;

            // sort the list before processing..
            dates.Sort();

            List<DateTime> setPosDates = new List<DateTime>();
            int size = dates.Count;

            for (int i = 0; i < r.BySetPosition.Count; i++)
            {
                int pos = r.BySetPosition[i];
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
        private List<DateTime> GetMonthVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;
            if (r.ByMonth.Count == 0)
                return dates;

            List<DateTime> monthlyDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.ByMonth.Count; j++)
                {
                    int month = r.ByMonth[j];
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
        private List<DateTime> GetWeekNoVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;
            if (r.ByWeekNo.Count == 0)
                return dates;

            List<DateTime> weekNoDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.ByWeekNo.Count; j++)
                {
                    // FIXME: not sure we're supposed to be doing an if() statement here to check.
                    // From the original code, looks like we're supposed to simply change the week number
                    // of 'date' and add it to weekNoDates.
                    int weekNo = r.ByWeekNo[j];
                    if (Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, r.FirstDayOfWeek) == weekNo)
                        weekNoDates.Add(date);
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
        private List<DateTime> GetYearDayVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;
            if (r.ByYearDay.Count == 0)
                return dates;

            List<DateTime> yearDayDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.ByYearDay.Count; j++)
                {
                    int yearDay = r.ByYearDay[j];

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
        private List<DateTime> GetMonthDayVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;
            if (r.ByMonthDay.Count == 0)
                return dates;

            List<DateTime> monthDayDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.ByMonthDay.Count; j++)
                {
                    int monthDay = r.ByMonthDay[j];
                        
                    int daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
                    if (monthDay > daysInMonth)
                        throw new ArgumentException("Invalid day of month: " + date + " (day " + monthDay + ")");

                    DateTime newDate = new DateTime(date.Year, date.Month, monthDay, date.Hour, date.Minute, date.Second, date.Kind);
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
        private List<DateTime> GetDayVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;
            if (r.ByDay.Count == 0)
                return dates;

            List<DateTime> weekDayDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.ByDay.Count; j++)
                {
                    IWeekDay weekDay = r.ByDay[j];
                    
                    // if BYYEARDAY or BYMONTHDAY is specified filter existing
                    // list..
                    if (r.ByYearDay.Count > 0 || r.ByMonthDay.Count > 0)
                    {
                        if (weekDay.Equals((WeekDay)date))
                            weekDayDates.Add(date);
                    }
                    else
                    {
                        weekDayDates.AddRange(GetAbsWeekDays(date, weekDay));
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
        private List<DateTime> GetAbsWeekDays(DateTime date, IWeekDay weekDay)
        {
            IRecurrencePattern r = Pattern;
            List<DateTime> days = new List<DateTime>();

            DayOfWeek dayOfWeek = weekDay.DayOfWeek;
            if (r.Frequency == FrequencyType.Daily)
            {
                if (date.DayOfWeek == dayOfWeek)
                    days.Add(date);
            }
            else if (r.Frequency == FrequencyType.Weekly || r.ByWeekNo.Count > 0)
            {
                int weekNo = Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, r.FirstDayOfWeek);

                // construct a list of possible week days..
                while (date.DayOfWeek != dayOfWeek)
                    date = date.AddDays(1);
                
                while (Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, r.FirstDayOfWeek) == weekNo)
                {
                    days.Add(date);
                    date = date.AddDays(7);
                }
            }
            else if (r.Frequency == FrequencyType.Monthly || r.ByMonth.Count > 0)
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
            else if (r.Frequency == FrequencyType.Yearly)
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
        private List<DateTime> GetHourVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;

            if (r.ByHour.Count == 0)
                return dates;

            List<DateTime> hourlyDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.ByHour.Count; j++)
                {
                    int hour = r.ByHour[j];
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
        private List<DateTime> GetMinuteVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;

            if (r.ByMinute.Count == 0)
                return dates;
            
            List<DateTime> minutelyDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.ByMinute.Count; j++)
                {
                    int minute = r.ByMinute[j];
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
        private List<DateTime> GetSecondVariants(List<DateTime> dates)
        {
            IRecurrencePattern r = Pattern;

            if (r.BySecond.Count == 0)
                return dates;

            List<DateTime> secondlyDates = new List<DateTime>();
            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                for (int j = 0; j < r.BySecond.Count; j++)
                {
                    int second = r.BySecond[j];
                    date = date.AddSeconds(-date.Second + second);
                    secondlyDates.Add(date);
                }
            }
            return secondlyDates;
        }

        #endregion

        #region Overrides

        public override IList<IPeriod> Evaluate(IDateTime referenceDate, DateTime startDate, DateTime fromDate, DateTime toDate)
        {
            Periods.Clear();
            foreach (DateTime dt in GetDates(referenceDate, fromDate, toDate))
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
