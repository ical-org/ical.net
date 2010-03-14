using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class RecurrencePatternEvaluator :
        Evaluator
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

        public RecurrencePatternEvaluator(IRecurrencePattern pattern)
        {
            Initialize(pattern);
        }

        void Initialize(IRecurrencePattern pattern)
        {
            Pattern = pattern;
        }

        #endregion        

        #region Protected Methods

        protected void EnsureByXXXValues(iCalDateTime StartDate, ref RecurrencePattern r)
        {
            // If the frequency is weekly, and
            // no day of week is specified, use
            // the original date's day of week.
            // NOTE: fixes RRULE7 and RRULE8 handling
            if (r.Frequency == FrequencyType.Weekly &&
                r.ByDay.Count == 0)
                r.ByDay.Add(new DaySpecifier(StartDate.Value.DayOfWeek));
            if (r.Frequency > FrequencyType.Secondly &&
                r.BySecond.Count == 0 &&
                StartDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.BySecond.Add(StartDate.Value.Second);
            if (r.Frequency > FrequencyType.Minutely &&
                r.ByMinute.Count == 0 &&
                StartDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByMinute.Add(StartDate.Value.Minute);
            if (r.Frequency > FrequencyType.Hourly &&
                r.ByHour.Count == 0 &&
                StartDate.HasTime /* NOTE: Fixes a bug where all-day events have BySecond/ByMinute/ByHour added incorrectly */)
                r.ByHour.Add(StartDate.Value.Hour);
            // If neither BYDAY, BYMONTHDAY, or BYYEARDAY is specified,
            // default to the current day of month
            // NOTE: fixes RRULE23 handling, added BYYEARDAY exclusion
            // to fix RRULE25 handling
            if (r.Frequency > FrequencyType.Weekly &&
                r.ByMonthDay.Count == 0 &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0)
                r.ByMonthDay.Add(StartDate.Value.Day);
            // If neither BYMONTH nor BYYEARDAY is specified, default to
            // the current month
            // NOTE: fixes RRULE25 handling
            if (r.Frequency > FrequencyType.Monthly &&
                r.ByYearDay.Count == 0 &&
                r.ByDay.Count == 0 &&
                r.ByMonth.Count == 0)
                r.ByMonth.Add(StartDate.Value.Month);
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

        protected List<Period> GetOccurrences(iCalDateTime startTime, iCalDateTime fromTime, iCalDateTime toTime)
        {
            List<Period> periods = new List<Period>();

            // If the Recur is restricted by COUNT, we need to evaluate just
            // after any static occurrences so it's correctly restricted to a
            // certain number. NOTE: fixes bug #13 and bug #16
            if (Pattern.Count > 0)
            {
                fromTime = startTime;
                foreach (iCalDateTime dt in StaticOccurrences)
                {
                    if (fromTime < dt)
                        fromTime = dt;
                }
            }

            // Handle "UNTIL" values that are date-only. If we didn't change values here, "UNTIL" would
            // exclude the day it specifies, instead of the inclusive behaviour it should exhibit.
            iCalDateTime until = Pattern.Until;
            if (until.IsAssigned && !until.HasTime)
            {
                Pattern.Until = until = new iCalDateTime(
                    new DateTime(until.Year, until.Month, until.Day, 23, 59, 59, until.Value.Kind),
                    until.TZID,
                    until.Calendar);
            }

            // Ignore recurrences that occur outside our time frame we're looking at
            if ((until.IsAssigned && fromTime > until) ||
                toTime < startTime)
                return periods;

            // Narrow down our time range further to avoid over-processing
            if (until.IsAssigned && until < toTime)
                toTime = until;
            if (startTime > fromTime)
                fromTime = startTime;

            // If the interval is greater than 1, then we need to ensure that the StartDate occurs in one of the
            // "active" days/weeks/months/years/etc. to ensure that we properly "step" through the interval.
            // NOTE: Fixes bug #1741093 - WEEKLY frequency eval behaves strangely
            {
                long difference = DateUtils.DateDiff(Pattern.Frequency, startTime, fromTime, Pattern.WeekStart);
                while (difference % Pattern.Interval > 0)
                {
                    fromTime = DateUtils.AddFrequency(Pattern.Frequency, fromTime, -1);
                    difference--;
                }
            }

            // If the start date has no time, then our "From" date should not either 
            // NOTE: Fixes bug #1876582 - All-day holidays are sometimes giving incorrect times
            if (!startTime.HasTime)
            {
                fromTime = new iCalDateTime(fromTime.Year, fromTime.Month, fromTime.Day, startTime.Hour, startTime.Minute, startTime.Second);
                fromTime.IsUniversalTime = startTime.IsUniversalTime;
                fromTime.HasTime = false;
            }

            while (
                fromTime <= toTime &&
                (
                    Pattern.Count == int.MinValue ||
                    periods.Count <= Pattern.Count)
                )
            {
                // Retrieve occurrences that occur on our interval period
                Period period = new Period(fromTime);
                if (Pattern.BySetPosition.Count == 0 && Pattern.IsValidDate(fromTime) && !periods.Contains(period))
                    periods.Add(period);

                // Retrieve "extra" occurrences that happen within our interval period
                if (Pattern.Frequency > FrequencyType.Secondly)
                {
                    foreach (iCalDateTime dt in GetExtraOccurrences(fromTime, toTime))
                    {
                        Period p = new Period(dt);

                        // Don't add duplicates
                        if (!periods.Contains(p))
                            periods.Add(p);
                    }
                }

                IncrementDate(ref fromTime, Pattern, Pattern.Interval);
            }

            return periods;
        }        

        #endregion

        #region Calculating Extra Occurrences

        protected List<iCalDateTime> GetExtraOccurrences(iCalDateTime StartDate, iCalDateTime AbsEndDate)
        {
            List<iCalDateTime> DateTimes = new List<iCalDateTime>();
            iCalDateTime EndDate = new iCalDateTime(StartDate);

            IncrementDate(ref EndDate, Pattern, 1);
            EndDate = EndDate.AddSeconds(-1);
            if (EndDate > AbsEndDate)
                EndDate = AbsEndDate;

            return CalculateChildOccurrences(StartDate, EndDate);
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

        protected List<iCalDateTime> CalculateChildOccurrences(iCalDateTime startDate, iCalDateTime endDate)
        {
            TimeCalculation TC = new TimeCalculation(startDate, endDate, Pattern);
            switch (Pattern.Frequency)
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
                    foreach (int hour in Pattern.ByHour)
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
                    throw new NotSupportedException("CalculateChildOccurrences() is not supported for a frequency of " + Pattern.Frequency.ToString());
            }

            // Sort the Date/Time values in ascending order, to ensure they
            // occur in the correct order.  This is critical for BYSETPOS
            // values, such as BYSETPOS=-1.
            TC.DateTimes.Sort();

            // Apply the BYSETPOS to the list of child occurrences
            // We do this before the dates are filtered by Start and End date
            // so that the BYSETPOS calculates correctly.
            // NOTE: fixes RRULE33 eval
            if (Pattern.BySetPosition.Count != 0)
            {
                List<iCalDateTime> newDateTimes = new List<iCalDateTime>();
                foreach (int pos in Pattern.BySetPosition)
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
                if ((iCalDateTime)TC.DateTimes[i] < startDate ||
                    (iCalDateTime)TC.DateTimes[i] > endDate)
                    TC.DateTimes.RemoveAt(i);
            }

            return TC.DateTimes;
        }

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the next occurrence of the pattern,
        /// given a valid previous occurrence, <paramref name="lastOccurrence"/>.
        /// As long as the recurrence pattern is valid, and
        /// <paramref name="lastOccurrence"/> is a valid previous 
        /// occurrence within the pattern, this will return the
        /// next occurrence.  NOTE: This will not give accurate results
        /// when COUNT or BYSETVAL are used.
        /// </summary>
        virtual public Period? GetNextOccurrence(iCalDateTime lastOccurrence)
        {
            if (lastOccurrence.IsAssigned)
            {
                iCalDateTime fromDate = lastOccurrence;
                iCalDateTime? toDate = null;

                RecurrencePattern r = new RecurrencePattern();
                r.CopyFrom(Pattern);

                // Enforce evaluation engine rules
                EnforceEvaluationRestrictions();

                switch (r.Frequency)
                {
                    case FrequencyType.Yearly:
                        toDate = fromDate.YearDate.AddYears(Pattern.Interval + 1);
                        break;
                    case FrequencyType.Monthly:
                        // Determine how far into the future we need to scan
                        // to get the next occurrence.
                        int yearsByInterval = (int)Math.Ceiling((double)Pattern.Interval / 12.0);
                        if (Pattern.ByMonthDay.Count > 0)
                            toDate = fromDate.YearDate.AddYears(yearsByInterval + 1);
                        else if (Pattern.ByMonth.Count > 0)
                            toDate = fromDate.AddYears(yearsByInterval);
                        else
                            toDate = fromDate.MonthDate.AddMonths(Pattern.Interval + 1);
                        break;
                    case FrequencyType.Weekly:
                        toDate = fromDate.AddDays((Pattern.Interval + 1) * 7);
                        break;
                    case FrequencyType.Daily:
                        if (Pattern.ByDay.Count > 0)
                            toDate = fromDate.AddDays(7);
                        else
                            toDate = fromDate.AddDays(Pattern.Interval + 1);
                        break;
                    case FrequencyType.Hourly:
                        if (Pattern.ByHour.Count > 0)
                        {
                            int daysByInterval = (int)Math.Ceiling((double)Pattern.Interval / 24.0);
                            toDate = fromDate.AddDays(daysByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddHours(Pattern.Interval + 1);
                        break;
                    case FrequencyType.Minutely:
                        if (Pattern.ByMinute.Count > 0)
                        {
                            int hoursByInterval = (int)Math.Ceiling((double)Pattern.Interval / 60.0);
                            toDate = fromDate.AddHours(hoursByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddMinutes(Pattern.Interval + 1);
                        break;
                    case FrequencyType.Secondly:
                        if (Pattern.BySecond.Count > 0)
                        {
                            int minutesByInterval = (int)Math.Ceiling((double)Pattern.Interval / 60.0);
                            toDate = fromDate.AddMinutes(minutesByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddMinutes(Pattern.Interval + 1);
                        break;
                }

                if (toDate != null)
                {
                    // Get the first occurence within the interval we just evaluated, if available.
                    IList<Period> periods = Evaluate(lastOccurrence, fromDate, toDate.Value);
                    if (periods != null &&
                        periods.Count > 0)
                    {
                        // NOTE: the lastOccurrence may or may not be contained in the
                        // result list of occurrence.  If it is, grab the next occurence
                        // if it is available.
                        if (periods[0].Equals(lastOccurrence))
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

        #region Overrides

        public override IList<Period> Evaluate(
            iCalDateTime startDate, 
            iCalDateTime fromDate, 
            iCalDateTime toDate)
        {
            List<Period> periods = new List<Period>();
            foreach (iCalDateTime dt in StaticOccurrences)
                periods.Add(new Period(dt));

            // Create a temporary recurrence for populating 
            // missing information using the 'StartDate'.
            RecurrencePattern r = new RecurrencePattern();
            r.CopyFrom(Pattern);

            // Enforce evaluation engine rules
            EnforceEvaluationRestrictions();

            // Fill in missing, necessary ByXXX values
            EnsureByXXXValues(startDate, ref r);

            // Get the occurrences
            foreach (Period p in GetOccurrences(startDate, fromDate, toDate))
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

            // Process the UNTIL, and make sure the DateTimes
            // occur between FromDate and ToDate
            for (int i = periods.Count - 1; i >= 0; i--)
            {
                iCalDateTime dt = (iCalDateTime)periods[i].StartTime;
                if (dt > toDate ||
                    dt < fromDate)
                    periods.RemoveAt(i);
            }

            // FIXME: do we still need to do this?
            // Assign missing values
            //foreach (iCalDateTime dt in periods)
            //    dt.MergeWith(startDate);

            // FIXME: should this happen during serialization instead?
            // Ensure that DateTimes have an assigned time if they occur less than daily
            //if (r.Frequency < FrequencyType.Daily)
            //{
            //    for (int i = 0; i < periods.Count; i++)
            //    {
            //        iCalDateTime dt = periods[i];
            //        dt.HasTime = true;
            //        periods[i] = dt;
            //    }
            //}

            return periods;
        }

        #endregion

        #region Helper Classes

        protected class TimeCalculation
        {
            public iCalDateTime StartDate;
            public iCalDateTime EndDate;
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
            public List<iCalDateTime> DateTimes;

            #region Public Properties

            public iCalDateTime CurrentDateTime
            {
                get
                {
                    iCalDateTime dt;

                    // Account for negative days of month (count backwards from the end of the month)
                    // NOTE: fixes RRULE18 evaluation
                    if (Day > 0)
                        // Changed from DateTimeKind.Local to StartDate.Kind
                        // NOTE: fixes bug #20
                        dt = new iCalDateTime(new DateTime(Year, Month, Day, Hour, Minute, Second, DateTimeKind.Utc));
                    else
                        dt = new iCalDateTime(new DateTime(Year, Month, 1, Hour, Minute, Second, DateTimeKind.Utc).AddMonths(1).AddDays(Day));

                    // Inherit time zone info, etc. from the start date
                    dt.MergeWith(StartDate);
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

            public TimeCalculation(iCalDateTime StartDate, iCalDateTime EndDate, IRecurrencePattern Recur)
            {
                this.StartDate = StartDate;
                this.EndDate = EndDate;
                this.Recur = Recur;

                CurrentDateTime = StartDate;

                YearDays = new List<int>(Recur.ByYearDay);
                ByDays = new List<IDaySpecifier>(Recur.ByDay);
                Months = new List<int>(Recur.ByMonth);
                Days = new List<int>(Recur.ByMonthDay);
                Hours = new List<int>(Recur.ByHour);
                Minutes = new List<int>(Recur.ByMinute);
                Seconds = new List<int>(Recur.BySecond);
                DateTimes = new List<iCalDateTime>();

                // Only check what months and days are possible for
                // the week's period of time we're evaluating
                // NOTE: fixes RRULE10 evaluation
                if (Recur.Frequency == FrequencyType.Weekly)
                {
                    // Weekly patterns can at most affect
                    // 7 days worth of scheduling.
                    // NOTE: fixes bug #2912657 - missing occurrences
                    iCalDateTime dt = StartDate;
                    for (int i = 0; i < 7; i++)
                    {
                        if (!Months.Contains(dt.Month))
                            Months.Add(dt.Month);
                        if (!Days.Contains(dt.Day))
                            Days.Add(dt.Day);

                        dt = dt.AddDays(1);
                    }
                }
                else if (Recur.Frequency > FrequencyType.Daily)
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
    }
}
