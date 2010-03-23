using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class RecurrencePatternEvaluator :
        Evaluator,
        INextRecurrable
    {
        #region Private Fields

        IRecurrencePattern m_Pattern;

        #endregion

        #region Protected Properties

        protected IRecurrencePattern Pattern
        {
            get { return m_Pattern; }
            set { m_Pattern = value; }
        }

        #endregion

        #region Constructors

        public RecurrencePatternEvaluator(IRecurrencePattern pattern) : base(pattern)
        {
            Initialize(pattern);
        }

        void Initialize(IRecurrencePattern pattern)
        {
            Pattern = pattern;
        }

        #endregion        

        #region Protected Methods

        protected void EnsureByXXXValues(IDateTime startDate, ref RecurrencePattern r)
        {
            // If the frequency is weekly, and
            // no day of week is specified, use
            // the original date's day of week.
            // NOTE: fixes RRULE7 and RRULE8 handling
            if (r.Frequency == FrequencyType.Weekly &&
                r.ByDay.Count == 0)
                r.ByDay.Add(new DaySpecifier(startDate.Value.DayOfWeek));
            if (r.Frequency > FrequencyType.Secondly &&
                r.BySecond.Count == 0 &&
                startDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.BySecond.Add(startDate.Value.Second);
            if (r.Frequency > FrequencyType.Minutely &&
                r.ByMinute.Count == 0 &&
                startDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByMinute.Add(startDate.Value.Minute);
            if (r.Frequency > FrequencyType.Hourly &&
                r.ByHour.Count == 0 &&
                startDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByHour.Add(startDate.Value.Hour);
            // If neither BYDAY, BYMONTHDAY, or BYYEARDAY is specified,
            // default to the current day of month
            // NOTE: fixes RRULE23 handling, added BYYEARDAY exclusion
            // to fix RRULE25 handling
            if (r.Frequency > FrequencyType.Weekly &&
                r.ByMonthDay.Count == 0 &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0)
                r.ByMonthDay.Add(startDate.Value.Day);
            // If neither BYMONTH nor BYYEARDAY is specified, default to
            // the current month
            // NOTE: fixes RRULE25 handling
            if (r.Frequency > FrequencyType.Monthly &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0 &&
                r.ByMonth.Count == 0)
                r.ByMonth.Add(startDate.Value.Month);
        }

        protected void EnforceEvaluationRestrictions()
        {
            RecurrenceEvaluationModeType? evaluationMode = Pattern.EvaluationMode;
            RecurrenceRestrictionType? evaluationRestriction = Pattern.RestrictionType;
            FrequencyType frequency = Pattern.Frequency;

            if (evaluationRestriction != RecurrenceRestrictionType.NoRestriction)
            {
                switch (evaluationMode)
                {
                    case RecurrenceEvaluationModeType.AdjustAutomatically:
                        switch (frequency)
                        {
                            case FrequencyType.Secondly:
                                {
                                    switch (evaluationRestriction)
                                    {
                                        case RecurrenceRestrictionType.Default:
                                        case RecurrenceRestrictionType.RestrictSecondly: frequency = FrequencyType.Minutely; break;
                                        case RecurrenceRestrictionType.RestrictMinutely: frequency = FrequencyType.Hourly; break;
                                        case RecurrenceRestrictionType.RestrictHourly: frequency = FrequencyType.Daily; break;
                                    }
                                } break;
                            case FrequencyType.Minutely:
                                {
                                    switch (evaluationRestriction)
                                    {
                                        case RecurrenceRestrictionType.RestrictMinutely: frequency = FrequencyType.Hourly; break;
                                        case RecurrenceRestrictionType.RestrictHourly: frequency = FrequencyType.Daily; break;
                                    }
                                } break;
                            case FrequencyType.Hourly:
                                {
                                    switch (evaluationRestriction)
                                    {
                                        case RecurrenceRestrictionType.RestrictHourly: frequency = FrequencyType.Daily; break;
                                    }
                                } break;
                            default: break;
                        }

                        // Set a new frequency for the pattern.
                        Pattern.Frequency = frequency;
                        
                        break;
                    case RecurrenceEvaluationModeType.ThrowException:
                    case RecurrenceEvaluationModeType.Default:
                        switch (frequency)
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

        #region Calculating Occurrences

        protected List<IPeriod> GetOccurrences(IDateTime startTime, IDateTime endTime, IRecurrencePattern r)
        {
            List<IPeriod> periods = new List<IPeriod>();

            //// FIXME: remove?
            //// If the Recur is restricted by COUNT, we need to evaluate just
            //// after any static occurrences so it's correctly restricted to a
            //// certain number. NOTE: fixes bug #13 and bug #16
            //if (Pattern.Count > 0)
            //{
            //    fromTime = startTime;
            //    foreach (IDateTime dt in StaticOccurrences)
            //    {
            //        if (fromTime.LessThan(dt))
            //            fromTime = dt;
            //    }
            //}

            IDateTime until = r.Until;
            if (until != null && !until.HasTime)
            {
                // Handle "UNTIL" values that are date-only. If we didn't change values here, "UNTIL" would
                // exclude the day it specifies, instead of the inclusive behaviour it should exhibit.
                until = DateUtil.EndOfDay(until);
            }

            // Ignore recurrences that occur outside our time frame we're looking at
            if ((until != null && startTime.GreaterThan(until)) || endTime.LessThan(startTime))
                return periods;

            // Narrow down our time range further to avoid over-processing
            if (until != null && until.LessThan(endTime))
                endTime = until;

            //// FIXME: remove?
            //// If the interval is greater than 1, then we need to ensure that the StartDate occurs in one of the
            //// "active" days/weeks/months/years/etc. to ensure that we properly "step" through the interval.
            //// NOTE: Fixes bug #1741093 - WEEKLY frequency eval behaves strangely
            //{
            //    long difference = DateUtil.DateDiff(Pattern.Frequency, startTime, fromTime, Pattern.WeekStart);
            //    while (difference % Pattern.Interval > 0)
            //    {
            //        fromTime = DateUtil.AddFrequency(Pattern.Frequency, fromTime, -1);
            //        difference--;
            //    }
            //}

            //// FIXME: remove?
            //// If the start date has no time, then our "From" date should not either 
            //// NOTE: Fixes bug #1876582 - All-day holidays are sometimes giving incorrect times
            //if (!startTime.HasTime)
            //{
            //    // Ensure the time properties are consistent with the start time
            //    fromTime = new iCalDateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            //    fromTime.HasTime = false;
            //}

            IDateTime current = startTime.Copy<IDateTime>();
            while (
                current.LessThanOrEqual(endTime) &&
                (
                    r.Count == int.MinValue ||
                    periods.Count <= r.Count)
                )
            {
                // Retrieve occurrences that occur on our interval period
                IPeriod period = new Period(current);
                if (r.BySetPosition.Count == 0 && r.IsValidDate(current) && !periods.Contains(period))
                    periods.Add(period);

                // Retrieve "extra" occurrences that happen within our interval period
                if (r.Frequency > FrequencyType.Secondly)
                {
                    foreach (IDateTime dt in GetExtraOccurrences(current, endTime, r))
                    {
                        IPeriod p = new Period(dt);

                        // Don't add duplicates
                        if (!periods.Contains(p))
                            periods.Add(p);
                    }
                }

                IncrementDate(ref current, r, r.Interval);
            }

            return periods;
        }        

        #endregion

        #region Calculating Extra Occurrences

        protected List<IDateTime> GetExtraOccurrences(IDateTime currentTime, IDateTime absEndTime, IRecurrencePattern r)
        {
            // Determine the actual range we're looking at
            IDateTime endTime = new iCalDateTime(currentTime);
            IncrementDate(ref endTime, r, 1);
            endTime = endTime.AddTicks(-1);

            // Ensure the end of the range doesn't exceed our absolute end.
            if (endTime.GreaterThan(absEndTime))
                endTime = absEndTime;

            // Calculate child occurrences between the current time and the end time.
            return CalculateChildOccurrences(currentTime, endTime, r);
        }

        /// <summary>
        /// NOTE: fixes RRULE25
        /// </summary>
        /// <param name="TC"></param>
        protected void FillYearDays(TimeCalculation TC)
        {
            DateTime baseDate = new DateTime(TC.StartDate.Value.Year, 1, 1);
            foreach (int day in TC.YearDays)
            {
                DateTime currDate;
                if (day > 0)
                    currDate = baseDate.AddDays(day - 1);
                else if (day < 0)
                    currDate = baseDate.AddYears(1).AddDays(day);
                else throw new Exception("BYYEARDAY cannot contain 0");

                TC.Month = currDate.Month;
                TC.Day = currDate.Day;
                FillHours(TC);
            }
        }

        /// <summary>
        /// NOTE: fixes RRULE26
        /// </summary>
        /// <param name="TC"></param>
        protected void FillByDay(TimeCalculation TC)
        {
            // If BYMONTH is specified, offset each day into those months,
            // otherwise, use Jan. 1st as a reference date.
            // NOTE: fixes USHolidays.ics eval
            List<int> months = new List<int>();
            if (TC.Recur.ByMonth.Count == 0)
                months.Add(1);
            else months = TC.Months;

            foreach (int month in months)
            {
                DateTime baseDate = new DateTime(TC.StartDate.Value.Year, month, 1);
                foreach (DaySpecifier day in TC.ByDays)
                {
                    DateTime curr = baseDate;

                    int inc = (day.Num < 0) ? -1 : 1;
                    if (day.Num != int.MinValue &&
                        day.Num < 0)
                    {
                        // Start at end of year, or end of month if BYMONTH is specified
                        if (TC.Recur.ByMonth.Count == 0)
                            curr = curr.AddYears(1).AddDays(-1);
                        else curr = curr.AddMonths(1).AddDays(-1);
                    }

                    while (curr.DayOfWeek != day.DayOfWeek)
                        curr = curr.AddDays(inc);

                    if (day.Num != int.MinValue)
                    {
                        for (int i = 1; i < day.Num; i++)
                            curr = curr.AddDays(7 * inc);

                        TC.Month = curr.Month;
                        TC.Day = curr.Day;
                        FillHours(TC);
                    }
                    else
                    {
                        while (
                            (TC.Recur.Frequency == FrequencyType.Monthly &&
                            curr.Month == TC.StartDate.Value.Month) ||
                            (TC.Recur.Frequency == FrequencyType.Yearly &&
                            curr.Year == TC.StartDate.Value.Year))
                        {
                            TC.Month = curr.Month;
                            TC.Day = curr.Day;
                            FillHours(TC);
                            curr = curr.AddDays(7);
                        }
                    }
                }
            }
        }

        protected void FillMonths(TimeCalculation TC)
        {
            foreach (int month in TC.Months)
            {
                TC.Month = month;
                FillDays(TC);
            }
        }

        protected void FillDays(TimeCalculation TC)
        {
            foreach (int day in TC.Days)
            {
                TC.Day = day;
                FillHours(TC);
            }
        }

        protected void FillHours(TimeCalculation TC)
        {
            if (TC.Hours.Count > 0)
            {
                foreach (int hour in TC.Hours)
                {
                    //if (current.Hour > hour)
                    //    TC.CurrentDateTime = current.AddDays(1);
                    //else
                    //    TC.CurrentDateTime = current;

                    TC.Hour = hour;
                    FillMinutes(TC);
                }
            }
            else
            {
                TC.Hour = 0;
                FillMinutes(TC);
            }
        }

        protected void FillMinutes(TimeCalculation TC)
        {
            if (TC.Minutes.Count > 0)
            {
                foreach (int minute in TC.Minutes)
                {
                    TC.Minute = minute;
                    FillSeconds(TC);
                }
            }
            else
            {
                TC.Minute = 0;
                FillSeconds(TC);
            }
        }

        protected void FillSeconds(TimeCalculation TC)
        {
            if (TC.Seconds.Count > 0)
            {
                foreach (int second in TC.Seconds)
                {
                    TC.Second = second;
                    TC.Calculate();
                }
            }
            else
            {
                TC.Second = 0;
                TC.Calculate();
            }
        }

        protected List<IDateTime> CalculateChildOccurrences(IDateTime startDate, IDateTime endDate, IRecurrencePattern r)
        {
            TimeCalculation TC = new TimeCalculation(startDate, endDate, r);
            switch (r.Frequency)
            {
                case FrequencyType.Yearly:
                    FillYearDays(TC);
                    FillByDay(TC);
                    FillMonths(TC);
                    break;
                case FrequencyType.Weekly:
                    // Weeks can span across months, so we must
                    // fill months (Note: fixes RRULE10 eval)                    
                    FillMonths(TC);
                    break;
                case FrequencyType.Monthly:
                    FillDays(TC);
                    FillByDay(TC);
                    break;
                case FrequencyType.Daily:
                    // Ensures that ByHour values are properly processed for the upcoming day.
                    // NOTE: fixes the bug tested in TEST5() in RecurrenceTest.
                    // This bug occurs in a situation like the following:
                    // An event occurs at 11:15 PM, but recurs every day at
                    // 8:00 AM and 5:00 PM.  When checking for occurrences,
                    // normally only same-day BYHOUR values are calculated.
                    // In this case, the same-day values are out-of-range,
                    // and hence aren't included in the result set.  To solve
                    // this problem, the following day is also included in the
                    // result set, causing this special case to work as expected.
                    TC.Days.Add(TC.Day);

                    bool isCalculated = false;
                    int startHour = TC.StartDate.Hour;
                    foreach (int hour in r.ByHour)
                    {
                        if (startHour > hour)
                        {
                            if (TC.Day > 27)
                            {
                                // Account for the "next" day for any
                                // given month.
                                TC.Days.Add(29);
                                TC.Days.Add(30);
                                TC.Days.Add(31);
                                TC.Days.Add(1);

                                TC.Months.Add(TC.Month);
                                FillDays(TC);

                                if (TC.Month == 12)
                                {
                                    TC.Year++;
                                    TC.Month = 1;
                                }
                                else
                                {
                                    TC.Month++;
                                }

                                FillDays(TC);

                                isCalculated = true;
                            }
                            else TC.Days.Add(TC.Day + 1);
                            break;
                        }
                    }

                    if (!isCalculated)
                        FillDays(TC);
                    break;
                case FrequencyType.Hourly:
                    FillMinutes(TC);
                    break;
                case FrequencyType.Minutely:
                    FillSeconds(TC);
                    break;
                default:
                    throw new NotSupportedException("CalculateChildOccurrences() is not supported for a frequency of " + r.Frequency.ToString());
            }

            // Sort the Date/Time values in ascending order, to ensure they
            // occur in the correct order.  This is critical for BYSETPOS
            // values, such as BYSETPOS=-1.
            TC.DateTimes.Sort();

            // Apply the BYSETPOS to the list of child occurrences
            // We do this before the dates are filtered by Start and End date
            // so that the BYSETPOS calculates correctly.
            // NOTE: fixes RRULE33 eval
            if (r.BySetPosition.Count != 0)
            {
                List<IDateTime> newDateTimes = new List<IDateTime>();
                foreach (int pos in r.BySetPosition)
                {
                    if (Math.Abs(pos) <= TC.DateTimes.Count)
                    {
                        if (pos > 0)
                            newDateTimes.Add(TC.DateTimes[pos - 1]);
                        else if (pos < 0)
                            newDateTimes.Add(TC.DateTimes[TC.DateTimes.Count + pos]);
                    }
                }

                TC.DateTimes = newDateTimes;
            }

            // Filter dates by Start and End date
            for (int i = TC.DateTimes.Count - 1; i >= 0; i--)
            {
                if (TC.DateTimes[i].LessThan(startDate) ||
                    TC.DateTimes[i].GreaterThan(endDate))
                    TC.DateTimes.RemoveAt(i);
            }

            return TC.DateTimes;
        }

        #endregion

        #endregion        

        #region Overrides

        public override IList<IPeriod> Evaluate(
            IDateTime startDate,
            IDateTime fromDate,
            IDateTime toDate)
        {
            // Associate the date/time values with the calendar
            Associate(startDate, fromDate, toDate);

            // Add any static occurrences to our list of periods
            List<IPeriod> periods = new List<IPeriod>();
            foreach (IDateTime dt in StaticOccurrences)
                periods.Add(new Period(dt));

            // Create a temporary recurrence for populating 
            // missing information using the 'StartDate'.
            RecurrencePattern r = new RecurrencePattern();
            r.CopyFrom(Pattern);
            r.AssociatedObject = Pattern.AssociatedObject;

            // Enforce evaluation engine rules
            EnforceEvaluationRestrictions();

            // Fill in missing, necessary ByXXX values
            EnsureByXXXValues(startDate, ref r);

            // Get the occurrences
            foreach (IPeriod p in GetOccurrences(startDate, toDate, r))
            {
                // NOTE:
                // Used to be DateTimes.AddRange(r.GetOccurrences(FromDate.Copy(), ToDate, r.Count))
                // By doing it this way, fixes bug #19.
                if (!periods.Contains(p))
                    periods.Add(p);
            }

            // Limit the count of returned recurrences
            if (r.Count != int.MinValue &&
                periods.Count > r.Count)
                periods.RemoveRange(r.Count, periods.Count - r.Count);

            // Ensure that DateTimes have an assigned time if they occur less than daily
            if (r.Frequency < FrequencyType.Daily)
            {
                foreach (IPeriod p in periods)
                    p.StartTime.HasTime = true;
            }

            return periods;
        }

        #endregion

        #region Helper Classes

        protected class TimeCalculation
        {
            public IDateTime StartDate;
            public IDateTime EndDate;
            public IRecurrencePattern Recur;
            public int Year;
            public List<IDaySpecifier> ByDays;
            public List<int> YearDays;
            public List<int> Months;
            public List<int> Days;
            public List<int> Hours;
            public List<int> Minutes;
            public List<int> Seconds;
            public int Month;
            public int Day;
            public int Hour;
            public int Minute;
            public int Second;
            public List<IDateTime> DateTimes;

            #region Public Properties

            public IDateTime CurrentDateTime
            {
                get
                {
                    IDateTime dt;

                    DateTimeKind kind = StartDate.IsUniversalTime ? DateTimeKind.Utc : DateTimeKind.Local;
                    // Account for negative days of month (count backwards from the end of the month)
                    // NOTE: fixes RRULE18 evaluation
                    if (Day > 0)
                        // Changed from DateTimeKind.Local to StartDate.Kind
                        // NOTE: fixes bug #20
                        dt = new iCalDateTime(new DateTime(Year, Month, Day, Hour, Minute, Second, kind));
                    else
                        dt = new iCalDateTime(new DateTime(Year, Month, 1, Hour, Minute, Second, kind).AddMonths(1).AddDays(Day));

                    // Inherit time zone info, from the start date and
                    // associate it with the same object.
                    dt.TZID = StartDate.TZID;
                    dt.AssociatedObject = StartDate.AssociatedObject;

                    return dt;
                }
                set
                {
                    Year = value.Year;
                    Month = value.Month;
                    Day = value.Day;
                    Hour = value.Hour;
                    Minute = value.Minute;
                    Second = value.Second;
                }
            }

            #endregion

            #region Constructor

            public TimeCalculation(IDateTime startDate, IDateTime endDate, IRecurrencePattern recur)
            {
                this.StartDate = startDate;
                this.EndDate = endDate;
                this.Recur = recur;

                CurrentDateTime = startDate;

                YearDays = new List<int>(recur.ByYearDay);
                ByDays = new List<IDaySpecifier>(recur.ByDay);
                Months = new List<int>(recur.ByMonth);
                Days = new List<int>(recur.ByMonthDay);
                Hours = new List<int>(recur.ByHour);
                Minutes = new List<int>(recur.ByMinute);
                Seconds = new List<int>(recur.BySecond);
                DateTimes = new List<IDateTime>();

                // Only check what months and days are possible for
                // the week's period of time we're evaluating
                // NOTE: fixes RRULE10 evaluation
                if (recur.Frequency == FrequencyType.Weekly)
                {
                    // Weekly patterns can at most affect
                    // 7 days worth of scheduling.
                    // NOTE: fixes bug #2912657 - missing occurrences
                    IDateTime dt = startDate;
                    for (int i = 0; i < 7; i++)
                    {
                        if (!Months.Contains(dt.Month))
                            Months.Add(dt.Month);
                        if (!Days.Contains(dt.Day))
                            Days.Add(dt.Day);

                        dt = dt.AddDays(1);
                    }
                }
                else if (recur.Frequency > FrequencyType.Daily)
                {
                    if (Months.Count == 0) Months.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
                    if (Days.Count == 0) Days.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 });
                }
            }

            #endregion

            #region Public Methods

            public void Calculate()
            {
                try
                {
                    // Make sure our day falls in the valid date range
                    if (Recur.IsValidDate(CurrentDateTime) &&
                        // Ensure the DateTime hasn't already been calculated (NOTE: fixes RRULE34 eval)
                        !DateTimes.Contains(CurrentDateTime))
                        DateTimes.Add(CurrentDateTime);
                }
                catch (ArgumentOutOfRangeException ex) { }
            }

            #endregion
        }

        #endregion

        #region INextRecurrable Members

        /// <summary>
        /// Returns the next occurrence of the pattern,
        /// given a valid previous occurrence, <paramref name="lastOccurrence"/>.
        /// As long as the recurrence pattern is valid, and
        /// <paramref name="lastOccurrence"/> is a valid previous 
        /// occurrence within the pattern, this will return the
        /// next occurrence.  NOTE: This will not give accurate results
        /// when COUNT or BYSETVAL are used.
        /// </summary>
        virtual public IPeriod GetNextOccurrence(IDateTime lastOccurrence)
        {
            if (lastOccurrence != null)
            {
                IPeriod lastPeriod = new Period(lastOccurrence);
                IDateTime fromDate = lastOccurrence.Copy<IDateTime>();
                IDateTime toDate = null;

                RecurrencePattern r = new RecurrencePattern();
                r.CopyFrom(Pattern);
                r.AssociatedObject = Pattern.AssociatedObject;

                // Enforce evaluation engine rules
                EnforceEvaluationRestrictions();

                switch (r.Frequency)
                {
                    case FrequencyType.Yearly:
                        toDate = DateUtil.FirstDayOfYear(fromDate).AddYears(r.Interval + 1);
                        break;
                    case FrequencyType.Monthly:
                        // Determine how far into the future we need to scan
                        // to get the next occurrence.
                        int yearsByInterval = (int)Math.Ceiling((double)r.Interval / 12.0);
                        if (r.ByMonthDay.Count > 0)
                            toDate = DateUtil.FirstDayOfYear(fromDate).AddYears(yearsByInterval + 1);
                        else if (r.ByMonth.Count > 0)
                            toDate = fromDate.AddYears(yearsByInterval);
                        else
                            toDate = DateUtil.FirstDayOfMonth(fromDate).AddMonths(r.Interval + 1);
                        break;
                    case FrequencyType.Weekly:
                        toDate = fromDate.AddDays((r.Interval + 1) * 7);
                        break;
                    case FrequencyType.Daily:
                        if (r.ByDay.Count > 0)
                            toDate = fromDate.AddDays(7);
                        else
                            toDate = fromDate.AddDays(r.Interval + 1);
                        break;
                    case FrequencyType.Hourly:
                        if (r.ByHour.Count > 0)
                        {
                            int daysByInterval = (int)Math.Ceiling((double)r.Interval / 24.0);
                            toDate = fromDate.AddDays(daysByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddHours(r.Interval + 1);
                        break;
                    case FrequencyType.Minutely:
                        if (r.ByMinute.Count > 0)
                        {
                            int hoursByInterval = (int)Math.Ceiling((double)r.Interval / 60.0);
                            toDate = fromDate.AddHours(hoursByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddMinutes(r.Interval + 1);
                        break;
                    case FrequencyType.Secondly:
                        if (r.BySecond.Count > 0)
                        {
                            int minutesByInterval = (int)Math.Ceiling((double)r.Interval / 60.0);
                            toDate = fromDate.AddMinutes(minutesByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddMinutes(r.Interval + 1);
                        break;
                }

                if (toDate != null)
                {
                    // Get the first occurence within the interval we just evaluated, if available.
                    IList<IPeriod> periods = Evaluate(lastOccurrence, fromDate, toDate);
                    if (periods != null &&
                        periods.Count > 0)
                    {
                        // NOTE: the lastOccurrence may or may not be contained in the
                        // result list of occurrence.  If it is, grab the next occurence
                        // if it is available.
                        if (periods[0].Equals(lastPeriod))
                        {
                            if (periods.Count > 1)
                                return periods[1];
                        }
                        else return periods[0];
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
