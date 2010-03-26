using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DDay.iCal
{
    public class OldRecurrencePatternEvaluator :
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

        public OldRecurrencePatternEvaluator(IRecurrencePattern pattern) : base(pattern)
        {
            Initialize(pattern);
        }

        void Initialize(IRecurrencePattern pattern)
        {
            Pattern = pattern;
        }

        #endregion        

        #region Protected Methods

        protected void EnsureByXXXValues(IDateTime originDate, ref RecurrencePattern r)
        {
            // If the frequency is weekly, and
            // no day of week is specified, use
            // the original date's day of week.
            // NOTE: fixes RRULE7 and RRULE8 handling
            if (r.Frequency == FrequencyType.Weekly &&
                r.ByDay.Count == 0)
                r.ByDay.Add(new WeekDay(originDate.Value.DayOfWeek));
            if (r.Frequency > FrequencyType.Secondly &&
                r.BySecond.Count == 0 &&
                originDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.BySecond.Add(originDate.Value.Second);
            if (r.Frequency > FrequencyType.Minutely &&
                r.ByMinute.Count == 0 &&
                originDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByMinute.Add(originDate.Value.Minute);
            if (r.Frequency > FrequencyType.Hourly &&
                r.ByHour.Count == 0 &&
                originDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByHour.Add(originDate.Value.Hour);
            // If neither BYDAY, BYMONTHDAY, or BYYEARDAY is specified,
            // default to the current day of month
            // NOTE: fixes RRULE23 handling, added BYYEARDAY exclusion
            // to fix RRULE25 handling
            if (r.Frequency > FrequencyType.Weekly &&
                r.ByMonthDay.Count == 0 &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0)
                r.ByMonthDay.Add(originDate.Value.Day);
            // If neither BYMONTH nor BYYEARDAY is specified, default to
            // the current month
            // NOTE: fixes RRULE25 handling
            if (r.Frequency > FrequencyType.Monthly &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0 &&
                r.ByMonth.Count == 0)
                r.ByMonth.Add(originDate.Value.Month);
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

        protected List<DateTime> GetOccurrences(DateTime startTime, DateTime endTime, IRecurrencePattern r)
        {
            List<DateTime> dateTimes = new List<DateTime>();
            
            DateTime until = DateTime.MaxValue;
            if (r.Until != null)
            {
                // NOTE: RecurrencePattern's UNTIL is meant to be compared against
                // a time-zone qualified OR a UTC date/time value.  Since these date/time
                // values don't always qualify to be compared against UNTIL, we will only
                // do a rough comparison here, for the sole purpose of ensuring we don't
                // overprocess date/time ranges.
                //
                // Since a time zone or UTC difference can never account for more than 24
                // hours, a single day is enough to ensure we don't overprocess while ensuring
                // that the UNTIL value is properly handled here.

                //// FIXME: move this behavior elsewhere...
                //Debug.Assert(r.Until.IsUniversalTime);
                //if (!r.Until.HasTime)
                //{
                //    // Handle "UNTIL" values that are date-only. If we didn't change values here, "UNTIL" would
                //    // exclude the day it specifies, instead of the inclusive behaviour it should exhibit.
                //    until = DateUtil.EndOfDay(r.Until).Value;
                //}
                //else until = r.Until.Value;

                until = r.Until.Value.AddDays(1);
            }

            // Ignore recurrences that occur outside our time frame we're looking at
            if ((until != null && startTime > until) || endTime < startTime)
                return dateTimes;

            // Narrow down our time range further to avoid over-processing
            if (until != null && until < endTime)
                endTime = until;            

            //// FIXME: remove?
            //// If the 'startTime' has no time, then our 'fromTime' should not either 
            //// NOTE: Fixes bug #1876582 - All-day holidays are sometimes giving incorrect times
            //if (!startTime.HasTime)
            //{
            //    // Ensure the time properties are consistent with the start time
            //    fromTime = new iCalDateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            //    fromTime.HasTime = false;
            //}

            DateTime current = startTime;
            while (
                current <= endTime &&
                (
                    r.Count == int.MinValue ||
                    dateTimes.Count <= r.Count)
                )
            {
                // Retrieve occurrences that occur on our interval period                
                if (r.BySetPosition.Count == 0 && IsValidDate(current) && !dateTimes.Contains(current))
                    dateTimes.Add(current);

                // Retrieve "extra" occurrences that happen within our interval period
                if (r.Frequency > FrequencyType.Secondly)
                {
                    foreach (DateTime dt in GetExtraOccurrences(current, endTime, r))
                    {
                        // Don't add duplicates
                        if (!dateTimes.Contains(dt))
                            dateTimes.Add(dt);
                    }
                }

                IncrementDate(ref current, r, r.Interval);
            }

            return dateTimes;
        }        

        #endregion

        #region Calculating Extra Occurrences

        protected List<DateTime> GetExtraOccurrences(DateTime currentTime, DateTime absEndTime, IRecurrencePattern r)
        {
            // Determine the actual range we're looking at
            DateTime endTime = currentTime;
            IncrementDate(ref endTime, r, 1);
            endTime = endTime.AddTicks(-1);

            // Ensure the end of the range doesn't exceed our absolute end.
            if (endTime > absEndTime)
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
            DateTime baseDate = new DateTime(TC.StartDate.Year, 1, 1);
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
            IRecurrencePattern r = TC.Pattern;

            // If BYMONTH is specified, offset each day into those months,
            // otherwise, use Jan. 1st as a reference date.
            // NOTE: fixes USHolidays.ics eval
            List<int> months = new List<int>();
            if (r.ByMonth.Count == 0)
                months.Add(1);
            else months = TC.Months;

            foreach (int month in months)
            {
                DateTime baseDate = new DateTime(TC.StartDate.Year, month, 1);
                foreach (WeekDay day in TC.ByDays)
                {
                    DateTime curr = baseDate;

                    int inc = (day.Offset < 0) ? -1 : 1;
                    if (day.Offset != int.MinValue &&
                        day.Offset < 0)
                    {
                        // Start at end of year, or end of month if BYMONTH is specified
                        if (r.ByMonth.Count == 0)
                            curr = curr.AddYears(1).AddDays(-1);
                        else curr = curr.AddMonths(1).AddDays(-1);
                    }

                    while (curr.DayOfWeek != day.DayOfWeek)
                        curr = curr.AddDays(inc);

                    if (day.Offset != int.MinValue)
                    {
                        for (int i = 1; i < day.Offset; i++)
                            curr = curr.AddDays(7 * inc);

                        TC.Month = curr.Month;
                        TC.Day = curr.Day;
                        FillHours(TC);
                    }
                    else
                    {
                        while (
                            (r.Frequency == FrequencyType.Monthly &&
                            curr.Month == TC.StartDate.Month) ||
                            (r.Frequency == FrequencyType.Yearly &&
                            curr.Year == TC.StartDate.Year))
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

        protected List<DateTime> CalculateChildOccurrences(DateTime startDate, DateTime endDate, IRecurrencePattern r)
        {
            TimeCalculation TC = new TimeCalculation(startDate, endDate, r, this);
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
                List<DateTime> newDateTimes = new List<DateTime>();
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
                if (TC.DateTimes[i] < startDate ||
                    TC.DateTimes[i] > endDate)
                    TC.DateTimes.RemoveAt(i);
            }

            return TC.DateTimes;
        }

        #endregion

        /// <summary>
        /// Returns true if <paramref name="dt"/> is a date/time that aligns to (occurs within)
        /// the recurrence pattern of this Recur, false otherwise.
        /// </summary>
        protected bool IsValidDate(DateTime dt)
        {
            IRecurrencePattern r = Pattern;
            if (r.BySecond.Count != 0 && !r.BySecond.Contains(dt.Second)) return false;
            if (r.ByMinute.Count != 0 && !r.ByMinute.Contains(dt.Minute)) return false;
            if (r.ByHour.Count != 0 && !r.ByHour.Contains(dt.Hour)) return false;
            if (r.ByDay.Count != 0)
            {
                bool found = false;
                foreach (IWeekDay day in r.ByDay)
                {
                    if (IsValidDate(day, dt))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            if (r.ByWeekNo.Count != 0)
            {
                bool found = false;
                int lastWeekOfYear = Calendar.GetWeekOfYear(new DateTime(dt.Year, 12, 31), System.Globalization.CalendarWeekRule.FirstFourDayWeek, r.FirstDayOfWeek);
                int currWeekNo = Calendar.GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstFourDayWeek, r.FirstDayOfWeek);
                foreach (int WeekNo in r.ByWeekNo)
                {
                    if ((WeekNo > 0 && WeekNo == currWeekNo) ||
                        (WeekNo < 0 && lastWeekOfYear + WeekNo + 1 == currWeekNo))
                        found = true;
                }
                if (!found)
                    return false;
            }
            if (r.ByMonth.Count != 0 && !r.ByMonth.Contains(dt.Month)) return false;
            if (r.ByMonthDay.Count != 0)
            {
                // Handle negative days of month (count backwards from the end)
                // NOTE: fixes RRULE18 eval
                bool found = false;
                int DaysInMonth = Calendar.GetDaysInMonth(dt.Year, dt.Month);
                foreach (int Day in r.ByMonthDay)
                {
                    if ((Day > 0) && (Day == dt.Day))
                        found = true;
                    else if ((Day < 0) && (DaysInMonth + Day + 1 == dt.Day))
                        found = true;
                }

                if (!found)
                    return false;
            }
            if (r.ByYearDay.Count != 0)
            {
                // Handle negative days of year (count backwards from the end)
                // NOTE: fixes RRULE25 eval
                bool found = false;
                int DaysInYear = Calendar.GetDaysInYear(dt.Year);
                DateTime baseDate = new DateTime(dt.Year, 1, 1);

                foreach (int Day in r.ByYearDay)
                {
                    if (Day > 0 && dt.Date == baseDate.AddDays(Day - 1))
                        found = true;
                    else if (Day < 0 && dt.Date == baseDate.AddYears(1).AddDays(Day))
                        found = true;
                }
                if (!found)
                    return false;
            }
            return true;
        }

        protected bool IsValidDate(IWeekDay day, DateTime dt)
        {
            IRecurrencePattern r = Pattern;
            bool valid = false;

            if (day.DayOfWeek == dt.DayOfWeek)
                valid = true;

            if (valid && day.Offset != int.MinValue)
            {
                int mult = (day.Offset < 0) ? -1 : 1;
                int offset = (day.Offset < 0) ? 1 : 0;
                int abs = Math.Abs(day.Offset);

                switch (r.Frequency)
                {
                    case FrequencyType.Monthly:
                        {
                            DateTime mondt = new DateTime(dt.Year, dt.Month, 1, dt.Hour, dt.Minute, dt.Second, dt.Kind);
                            mondt = DateTime.SpecifyKind(mondt, dt.Kind);
                            if (offset > 0)
                                mondt = mondt.AddMonths(1).AddDays(-1);

                            while (mondt.DayOfWeek != day.DayOfWeek)
                                mondt = mondt.AddDays(mult);

                            for (int i = 1; i < abs; i++)
                                mondt = mondt.AddDays(7 * mult);

                            if (dt.Date != mondt.Date)
                                valid = false;
                        } break;

                    case FrequencyType.Yearly:
                        {
                            // If BYMONTH is specified, then offset our tests
                            // by those months; otherwise, begin with Jan. 1st.
                            // NOTE: fixes USHolidays.ics eval
                            IList<int> months = new List<int>();
                            if (r.ByMonth.Count == 0)
                                months.Add(1);
                            else months = r.ByMonth;

                            bool found = false;
                            foreach (int month in months)
                            {
                                DateTime yeardt = new DateTime(dt.Year, month, 1, dt.Hour, dt.Minute, dt.Second, dt.Kind);                                
                                if (offset > 0)
                                {
                                    // Start at end of year, or end of month if BYMONTH is specified
                                    if (r.ByMonth.Count == 0)
                                        yeardt = yeardt.AddYears(1).AddDays(-1);
                                    else yeardt = yeardt.AddMonths(1).AddDays(-1);
                                }

                                while (yeardt.DayOfWeek != day.DayOfWeek)
                                    yeardt = yeardt.AddDays(mult);

                                for (int i = 1; i < abs; i++)
                                    yeardt = yeardt.AddDays(7 * mult);

                                if (object.Equals(dt, yeardt))
                                    found = true;
                            }

                            if (!found)
                                valid = false;
                        } break;

                    // Ignore other frequencies
                    default: break;
                }
            }
            return valid;
        }

        #endregion        

        #region Overrides

        public override IList<IPeriod> Evaluate(
            IDateTime referenceDate,
            DateTime startDate,
            DateTime fromDate,
            DateTime toDate)
        {
            Trace.TraceInformation("Evaluating recurrence pattern starting at '" + startDate + "', from '" + fromDate + "' to '" + toDate + "'...");

            // Advance the start date along the interval
            
            // Add any static occurrences to our list of periods
            Periods.Clear();
            foreach (DateTime dt in StaticOccurrences)
                Periods.Add(new Period(ConvertToIDateTime(dt, referenceDate)));

            // Create a temporary recurrence for populating 
            // missing information using the 'StartDate'.
            RecurrencePattern r = new RecurrencePattern();
            r.CopyFrom(Pattern);
            r.AssociatedObject = Pattern.AssociatedObject;

            // Enforce evaluation engine rules
            EnforceEvaluationRestrictions();

            // Fill in missing, necessary ByXXX values
            // FIXME: is referenceDate okay to use here?
            EnsureByXXXValues(referenceDate, ref r);

            // Advance the 'startDate' along the recurrence interval toward the 'fromDate'
            // NOTE: Theoretically, this fixes bug #1741093 - WEEKLY frequency eval behaves strangely.
            // However, it has not been tested or specifically proven to do so singlehandedly.
            // NOTE: this code is part of the major evaluation refactoring.
            DateTime nextDate = startDate;
            IncrementDate(ref nextDate, r, r.Interval);
            while (nextDate < fromDate)
            {
                startDate = nextDate;
                IncrementDate(ref nextDate, r, r.Interval);
            }

            // Get the occurrences
            foreach (DateTime dt in GetOccurrences(startDate, toDate, r))
            {
                IPeriod p = new Period(ConvertToIDateTime(dt, referenceDate));
                if (!Periods.Contains(p))
                    Periods.Add(p);
            }

            // Ensure no items occur past the UNTIL value
            if (r.Until != null)
            {
                for (int i = Periods.Count - 1; i >= 0; i--)
                {
                    if (Periods[i].StartTime.GreaterThan(r.Until))
                        Periods.RemoveAt(i);
                }
            }

            // Limit the count of returned recurrences
            if (r.Count != int.MinValue &&
                Periods.Count > r.Count)
            {
                while (Periods.Count > r.Count)
                    Periods.RemoveAt(r.Count);
            }

            // Ensure that DateTimes have an assigned time if they occur less than daily
            if (r.Frequency < FrequencyType.Daily)
            {
                foreach (IPeriod p in Periods)
                    p.StartTime.HasTime = true;
            }

            return Periods;
        }

        #endregion

        #region Helper Classes

        protected class TimeCalculation
        {
            public DateTime StartDate;
            public DateTime EndDate;
            public IRecurrencePattern Pattern;            public OldRecurrencePatternEvaluator Evaluator;
            public int Year;
            public List<IWeekDay> ByDays;
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
            public List<DateTime> DateTimes;

            #region Public Properties

            public DateTime CurrentDateTime
            {
                get
                {
                    DateTime dt;

                    DateTimeKind kind = StartDate.Kind;
                    // Account for negative days of month (count backwards from the end of the month)
                    // NOTE: fixes RRULE18 evaluation
                    if (Day > 0)
                        // Changed from DateTimeKind.Local to StartDate.Kind
                        // NOTE: fixes bug #20
                        dt = new DateTime(Year, Month, Day, Hour, Minute, Second, kind);
                    else
                        dt = new DateTime(Year, Month, 1, Hour, Minute, Second, kind).AddMonths(1).AddDays(Day);

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

            public TimeCalculation(DateTime startDate, DateTime endDate, IRecurrencePattern pattern, OldRecurrencePatternEvaluator evaluator)
            {
                this.StartDate = startDate;
                this.EndDate = endDate;
                this.Pattern = pattern;                this.Evaluator = evaluator;

                CurrentDateTime = startDate;

                YearDays = new List<int>(Pattern.ByYearDay);
                ByDays = new List<IWeekDay>(Pattern.ByDay);
                Months = new List<int>(Pattern.ByMonth);
                Days = new List<int>(Pattern.ByMonthDay);
                Hours = new List<int>(Pattern.ByHour);
                Minutes = new List<int>(Pattern.ByMinute);
                Seconds = new List<int>(Pattern.BySecond);
                DateTimes = new List<DateTime>();

                // Only check what months and days are possible for
                // the week's period of time we're evaluating
                // NOTE: fixes RRULE10 evaluation
                if (Pattern.Frequency == FrequencyType.Weekly)
                {
                    // Weekly patterns can at most affect
                    // 7 days worth of scheduling.
                    // NOTE: fixes bug #2912657 - missing occurrences
                    DateTime dt = startDate;
                    for (int i = 0; i < 7; i++)
                    {
                        if (!Months.Contains(dt.Month))
                            Months.Add(dt.Month);
                        if (!Days.Contains(dt.Day))
                            Days.Add(dt.Day);

                        dt = dt.AddDays(1);
                    }
                }
                else if (Pattern.Frequency > FrequencyType.Daily)
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
                    DateTime current = CurrentDateTime;

                    // Make sure our day falls in the valid date range
                    if (Evaluator.IsValidDate(current) && 
                        // Ensure the DateTime hasn't already been calculated (NOTE: fixes RRULE34 eval)
                        !DateTimes.Contains(current))
                        DateTimes.Add(current);
                }
                catch (ArgumentOutOfRangeException) { }
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
                DateTime fromDate = DateUtil.GetSimpleDateTimeData(lastOccurrence);
                DateTime? toDate = null;

                RecurrencePattern r = new RecurrencePattern();
                r.CopyFrom(Pattern);
                r.AssociatedObject = Pattern.AssociatedObject;

                // Enforce evaluation engine rules
                EnforceEvaluationRestrictions();

                switch (r.Frequency)
                {
                    case FrequencyType.Yearly:
                        toDate = fromDate.AddDays(-fromDate.DayOfYear + 1).AddYears(r.Interval + 1);
                        break;
                    case FrequencyType.Monthly:
                        // Determine how far into the future we need to scan
                        // to get the next occurrence.
                        int yearsByInterval = (int)Math.Ceiling((double)r.Interval / 12.0);
                        if (r.ByMonthDay.Count > 0)
                            toDate = fromDate.AddDays(-fromDate.DayOfYear + 1).AddYears(yearsByInterval + 1);
                        else if (r.ByMonth.Count > 0)
                            toDate = fromDate.AddYears(yearsByInterval);
                        else
                            toDate = fromDate.AddDays(-fromDate.Day + 1).AddMonths(r.Interval + 1);
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
                    IList<IPeriod> periods = Evaluate(lastOccurrence, DateUtil.GetSimpleDateTimeData(lastOccurrence), fromDate, toDate.Value);
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
