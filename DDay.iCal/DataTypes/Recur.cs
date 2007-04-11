using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// An iCalendar representation of the <c>RRULE</c> property.
    /// </summary>
    public partial class Recur : iCalDataType
    {
        #region Public Enums and Classes
        public enum FrequencyType
        {
            SECONDLY,
            MINUTELY,
            HOURLY,
            DAILY,
            WEEKLY,
            MONTHLY,
            YEARLY
        };

        #endregion       

        #region Private Fields
        public System.Globalization.CultureInfo m_Culture;
        public System.Globalization.Calendar m_Calendar;

        private FrequencyType m_Frequency;
        private Date_Time m_Until;
        private int m_Count = int.MinValue;
        private int m_Interval = int.MinValue;
        private ArrayList m_BySecond = new ArrayList();
        private ArrayList m_ByMinute = new ArrayList();
        private ArrayList m_ByHour = new ArrayList();
        private ArrayList m_ByDay = new ArrayList();
        private ArrayList m_ByMonthDay = new ArrayList();
        private ArrayList m_ByYearDay = new ArrayList();
        private ArrayList m_ByWeekNo = new ArrayList();
        private ArrayList m_ByMonth = new ArrayList();
        private ArrayList m_BySetPos = new ArrayList();
        private DayOfWeek m_Wkst = DayOfWeek.Monday;
        private List<Date_Time> m_StaticOccurrences = new List<Date_Time>();
                
        #endregion

        #region Public Properties

        public FrequencyType Frequency
        {
            get { return m_Frequency; }
            set { m_Frequency = value; }
        }

        public Date_Time Until
        {
            get { return m_Until; }
            set { m_Until = value; }
        }

        public int Count
        {
            get { return m_Count; }
            set { m_Count = value; }
        }

        public int Interval
        {
            get { return m_Interval; }
            set { m_Interval = value; }
        }

        public ArrayList BySecond
        {
            get { return m_BySecond; }
            set { m_BySecond = value; }
        }

        public ArrayList ByMinute
        {
            get { return m_ByMinute; }
            set { m_ByMinute = value; }
        }

        public ArrayList ByHour
        {
            get { return m_ByHour; }
            set { m_ByHour = value; }
        }

        public ArrayList ByDay
        {
            get { return m_ByDay; }
            set { m_ByDay = value; }
        }

        public ArrayList ByMonthDay
        {
            get { return m_ByMonthDay; }
            set { m_ByMonthDay = value; }
        }

        public ArrayList ByYearDay
        {
            get { return m_ByYearDay; }
            set { m_ByYearDay = value; }
        }

        public ArrayList ByWeekNo
        {
            get { return m_ByWeekNo; }
            set { m_ByWeekNo = value; }
        }

        public ArrayList ByMonth
        {
            get { return m_ByMonth; }
            set { m_ByMonth = value; }
        }

        public ArrayList BySetPos
        {
            get { return m_BySetPos; }
            set { m_BySetPos = value; }
        }

        public DayOfWeek Wkst
        {
            get { return m_Wkst; }
            set { m_Wkst = value; }
        }

        public List<Date_Time> StaticOccurrences
        {
            get { return m_StaticOccurrences; }
            set { m_StaticOccurrences = value; }
        }

        #endregion

        #region Constructors
        public Recur()
        {            
            m_Calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        }
        public Recur(string value)
            : this()
        {
            CopyFrom((Recur)Parse(value));
        }
        #endregion

        #region Overrides

        public override void CopyFrom(object obj)
        {
            if (obj is Recur)
            {
                Recur r = (Recur)obj;

                Frequency = r.Frequency;
                Until = r.Until;
                Count = r.Count;
                Interval = r.Interval;
                BySecond = new ArrayList(r.BySecond);
                ByMinute = new ArrayList(r.ByMinute);
                ByHour = new ArrayList(r.ByHour);
                ByDay = new ArrayList(r.ByDay);
                ByMonthDay = new ArrayList(r.ByMonthDay);
                ByYearDay = new ArrayList(r.ByYearDay);
                ByWeekNo = new ArrayList(r.ByWeekNo);
                ByMonth = new ArrayList(r.ByMonth);
                BySetPos = new ArrayList(r.BySetPos);
                Wkst = r.Wkst;
            }
            base.CopyFrom(obj);
        }

        public override bool Equals(object obj)
        {
            if (obj is Recur)
            {
                Recur r = (Recur)obj;
                if (!ArrayEquals(r.ByDay, ByDay) ||
                    !ArrayEquals(r.ByHour, ByHour) ||
                    !ArrayEquals(r.ByMinute, ByMinute) ||
                    !ArrayEquals(r.ByMonth, ByMonth) ||
                    !ArrayEquals(r.ByMonthDay, ByMonthDay) ||
                    !ArrayEquals(r.BySecond, BySecond) ||
                    !ArrayEquals(r.BySetPos, BySetPos) ||
                    !ArrayEquals(r.ByWeekNo, ByWeekNo) ||
                    !ArrayEquals(r.ByYearDay, ByYearDay))
                    return false;
                if (r.Count != Count) return false;
                if (r.Frequency != Frequency) return false;
                if (r.Interval != Interval &&
                    // MinValue and 1 are treated as identical for Interval
                    ((r.Interval != int.MinValue && r.Interval != 1) ||
                     (Interval != int.MinValue && Interval != 1)))
                    return false;
                if (r.Until != null)
                {
                    if (!r.Until.Equals(Until))
                        return false;
                }
                else if (Until != null)
                    return false;
                if (r.Wkst != Wkst) return false;
                return true;
            }
            return base.Equals(obj);
        }

        private bool ArrayEquals(ArrayList a1, ArrayList a2)
        {
            for (int i = 0; i < a1.Count; i++)
                if (!a1[i].Equals(a2[i]))
                    return false;
            return true;
        }

        public override bool TryParse(string value, ref object obj)
        {
            Recur r = (Recur)obj;
                        
            Match match = Regex.Match(value, @"FREQ=(SECONDLY|MINUTELY|HOURLY|DAILY|WEEKLY|MONTHLY|YEARLY);?(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // Parse the frequency type
                r.Frequency = (FrequencyType)Enum.Parse(typeof(FrequencyType), match.Groups[1].Value);

                // NOTE: fixed a bug where the group 2 match
                // resulted in an empty string, which caused
                // an error.
                if (match.Groups[2].Success &&
                    match.Groups[2].Length > 0)
                {
                    string[] keywordPairs = match.Groups[2].Value.Split(';');
                    foreach (string keywordPair in keywordPairs)
                    {
                        string[] keyValues = keywordPair.Split('=');                        
                        string keyword = keyValues[0];
                        string keyValue = keyValues[1];

                        switch (keyword.ToUpper())
                        {
                            case "UNTIL": r.Until = new Date_Time(keyValue); break;
                            case "COUNT": r.Count = Convert.ToInt32(keyValue); break;
                            case "INTERVAL": r.Interval = Convert.ToInt32(keyValue); break;
                            case "BYSECOND": AddInt32Values(r.BySecond, keyValue); break;
                            case "BYMINUTE": AddInt32Values(r.ByMinute, keyValue); break;
                            case "BYHOUR": AddInt32Values(r.ByHour, keyValue); break;
                            case "BYDAY":
                                {
                                    string[] days = keyValue.Split(',');
                                    foreach (string day in days)
                                        r.ByDay.Add(new DaySpecifier(day));
                                } break;
                            case "BYMONTHDAY": AddInt32Values(r.ByMonthDay, keyValue); break;
                            case "BYYEARDAY": AddInt32Values(r.ByYearDay, keyValue); break;
                            case "BYWEEKNO": AddInt32Values(r.ByWeekNo, keyValue); break;
                            case "BYMONTH": AddInt32Values(r.ByMonth, keyValue); break;
                            case "BYSETPOS": AddInt32Values(r.BySetPos, keyValue); break;
                            case "WKST": r.Wkst = GetDayOfWeek(keyValue); break;                                
                        }
                    }
                }
            }
            else if ((match = Regex.Match(value, @"every\s+(?<Interval>other|\d+)?\w{0,2}\s*(?<Freq>second|minute|hour|day|week|month|year)s?,?\s*(?<More>.+)", RegexOptions.IgnoreCase)).Success)
            {
                if (match.Groups["Interval"].Success)
                {
                    int interval;
                    if (!int.TryParse(match.Groups["Interval"].Value, out interval))
                        r.Interval = 2; // "other"
                    else r.Interval = interval;
                }
                else r.Interval = 1;

                switch (match.Groups["Freq"].Value.ToLower())
                {
                    case "second": r.Frequency = FrequencyType.SECONDLY; break;
                    case "minute": r.Frequency = FrequencyType.MINUTELY; break;
                    case "hour": r.Frequency = FrequencyType.HOURLY; break;
                    case "day": r.Frequency = FrequencyType.DAILY; break;
                    case "week": r.Frequency = FrequencyType.WEEKLY; break;
                    case "month": r.Frequency = FrequencyType.MONTHLY; break;
                    case "year": r.Frequency = FrequencyType.YEARLY; break;
                }

                string[] values = match.Groups["More"].Value.Split(',');
                foreach (string item in values)
                {
                    if ((match = Regex.Match(item, @"(?<Num>\d+)\w\w\s+(?<Type>second|minute|hour|day|week|month)", RegexOptions.IgnoreCase)).Success ||
                        (match = Regex.Match(item, @"(?<Type>second|minute|hour|day|week|month)\s+(?<Num>\d+)", RegexOptions.IgnoreCase)).Success)
                    {
                        int num;
                        if (int.TryParse(match.Groups["Num"].Value, out num))
                        {
                            ArrayList al = null;
                            switch (match.Groups["Type"].Value.ToLower())
                            {
                                case "second":
                                    r.BySecond.Add(num);
                                    break;
                                case "minute":
                                    r.ByMinute.Add(num);
                                    break;
                                case "hour":
                                    r.ByHour.Add(num);
                                    break;
                                case "day":
                                    switch (r.Frequency)
                                    {
                                        case FrequencyType.YEARLY:
                                            r.ByYearDay.Add(num);
                                            break;
                                        case FrequencyType.MONTHLY:
                                            r.ByMonthDay.Add(num);
                                            break;
                                    }
                                    break;
                                case "week":
                                    r.ByWeekNo.Add(num);
                                    break;
                                case "month":
                                    r.ByMonth.Add(num);
                                    break;
                            }
                        }
                    }
                    else if ((match = Regex.Match(item, @"(?<Num>\d+\w{0,2})?(\w|\s)+?(?<First>first)?(?<Last>last)?\s*((?<Day>sunday|monday|tuesday|wednesday|thursday|friday|saturday)\s*(and|or)?\s*)+", RegexOptions.IgnoreCase)).Success)
                    {
                        int num = int.MinValue;
                        if (match.Groups["Num"].Success)
                        {
                            if (int.TryParse(match.Groups["Num"].Value, out num))
                            {
                                if (match.Groups["Last"].Success)
                                {
                                    // Make number negative
                                    num *= -1;
                                }
                            }
                        }
                        else if (match.Groups["Last"].Success)
                            num = -1;
                        else if (match.Groups["First"].Success)
                            num = 1;

                        foreach (Capture capture in match.Groups["Day"].Captures)
                        {                            
                            DaySpecifier ds = new DaySpecifier((DayOfWeek)Enum.Parse(typeof(DayOfWeek), capture.Value, true));
                            ds.Num = num;
                            r.ByDay.Add(ds);
                        }                        
                    }
                    else if ((match = Regex.Match(item, @"at\s+(?<Hour>\d{1,2})(:(?<Minute>\d{2})((:|\.)(?<Second>\d{2}))?)?\s*(?<Meridian>(a|p)m?)?", RegexOptions.IgnoreCase)).Success)
                    {
                        int hour, minute, second;
                        
                        if (int.TryParse(match.Groups["Hour"].Value, out hour))
                        {
                            // Adjust for PM
                            if (match.Groups["Meridian"].Success && 
                                match.Groups["Meridian"].Value.ToUpper().StartsWith("P"))
                                hour += 12;
                            
                            r.ByHour.Add(hour);

                            if (match.Groups["Minute"].Success &&
                                int.TryParse(match.Groups["Minute"].Value, out minute))
                            {
                                r.ByMinute.Add(minute);
                                if (match.Groups["Second"].Success &&
                                    int.TryParse(match.Groups["Second"].Value, out second))
                                    r.BySecond.Add(second);
                            }
                        }
                    }
                    else if ((match = Regex.Match(item, @"^\s*until\s+(?<DateTime>.+)$", RegexOptions.IgnoreCase)).Success)
                    {
                        DateTime dt = DateTime.Parse(match.Groups["DateTime"].Value);
                        DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                        r.Until = new Date_Time(dt);
                    }
                    else if ((match = Regex.Match(item, @"^\s*for\s+(?<Count>\d+)\s+occurrences\s*$", RegexOptions.IgnoreCase)).Success)
                    {
                        int count;
                        if (!int.TryParse(match.Groups["Count"].Value, out count))
                            return false;
                        else r.Count = count;
                    }
                }
            }
            else return false;

            CheckMutuallyExclusive("COUNT", "UNTIL", r.Count, r.Until);
            CheckRange("INTERVAL", r.Interval, 1, int.MaxValue);
            CheckRange("COUNT", r.Count, 1, int.MaxValue);
            CheckRange("BYSECOND", r.BySecond, 0, 59);
            CheckRange("BYMINUTE", r.ByMinute, 0, 59);
            CheckRange("BYHOUR", r.ByHour, 0, 23);
            CheckRange("BYMONTHDAY", r.ByMonthDay, -31, 31);
            CheckRange("BYYEARDAY", r.ByYearDay, -366, 366);
            CheckRange("BYWEEKNO", r.ByWeekNo, -53, 53);
            CheckRange("BYMONTH", r.ByMonth, 1, 12);
            CheckRange("BYSETPOS", r.BySetPos, -366, 366);
            return true;
        }

        /// <summary>
        /// Returns a typed copy of the object.
        /// </summary>
        /// <returns>A typed copy of the object.</returns>
        public Recur Copy()
        {
            return (Recur)base.Copy();
        }

        #endregion

        #region Static Methods
        static public DayOfWeek GetDayOfWeek(string value)
        {
            switch (value.ToUpper())
            {
                case "SU": return DayOfWeek.Sunday;
                case "MO": return DayOfWeek.Monday;
                case "TU": return DayOfWeek.Tuesday;
                case "WE": return DayOfWeek.Wednesday;
                case "TH": return DayOfWeek.Thursday;
                case "FR": return DayOfWeek.Friday;
                case "SA": return DayOfWeek.Saturday;
            }
            throw new ArgumentException(value + " is not a valid iCal day-of-week indicator.");
        }

        static protected void AddInt32Values(ArrayList array, string value)
        {
            string[] values = value.Split(',');
            foreach (string v in values)
                array.Add(Convert.ToInt32(v));
        }
        #endregion

        #region Protected Methods

        protected void EnsureInterval()
        {
            if (Interval == int.MinValue)
                Interval = 1;
        }

        protected void EnsureByXXXValues(Date_Time StartDate)
        {
            // If the frequency is weekly, and
            // no day of week is specified, use
            // the original date's day of week.
            // NOTE: fixes RRULE7 and RRULE8 handling
            if (Frequency == FrequencyType.WEEKLY &&
                ByDay.Count == 0)
                this.ByDay.Add(new DaySpecifier(StartDate.Value.DayOfWeek));            
            if (Frequency > FrequencyType.SECONDLY &&
                this.BySecond.Count == 0)
                this.BySecond.Add(StartDate.Value.Second);
            if (Frequency > FrequencyType.MINUTELY &&
                this.ByMinute.Count == 0)
                this.ByMinute.Add(StartDate.Value.Minute);
            if (Frequency > FrequencyType.HOURLY &&
                this.ByHour.Count == 0)
                this.ByHour.Add(StartDate.Value.Hour);
            // If neither BYDAY, BYMONTHDAY, or BYYEARDAY is specified, default to the current day of month
            // NOTE: fixes RRULE23 handling, added BYYEARDAY exclusion to fix RRULE25 handling
            if (Frequency > FrequencyType.WEEKLY &&
                this.ByMonthDay.Count == 0 && 
                this.ByYearDay.Count == 0 && 
                this.ByDay.Count == 0) 
                this.ByMonthDay.Add(StartDate.Value.Day);
            // If neither BYMONTH or BYYEARDAY is specified, default to the current month
            // NOTE: fixes RRULE25 handling
            if (Frequency > FrequencyType.MONTHLY &&
                this.ByYearDay.Count == 0 && 
                this.ByDay.Count == 0 &&
                this.ByMonth.Count == 0)
                this.ByMonth.Add(StartDate.Value.Month);
        }

        #region Calculating Occurrences

        protected List<Date_Time> GetOccurrences(Date_Time StartDate, Date_Time EndDate, int Count)
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            while (StartDate <= EndDate &&
                (Count == int.MinValue ||
                DateTimes.Count <= Count))
            {
                // Retrieve occurrences that occur on our interval period
                if (BySetPos.Count == 0 && CheckValidDate(StartDate) && !DateTimes.Contains(StartDate.Value))
                    DateTimes.Add(StartDate.Copy());

                // Retrieve "extra" occurrences that happen within our interval period
                if (Frequency > FrequencyType.SECONDLY)
                {
                    foreach (Date_Time dt in GetExtraOccurrences(StartDate, EndDate))
                    {
                        // Don't add duplicates
                        if (!DateTimes.Contains(dt))
                            DateTimes.Add(dt.Copy());
                    }
                }

                IncrementDate(StartDate);
            }
            
            return DateTimes;
        }

        protected void IncrementDate(Date_Time dt)
        {
            IncrementDate(dt, this.Interval);
        }

        protected void IncrementDate(Date_Time dt, int Interval)
        {
            DateTime old = dt.Value;
            switch (Frequency)
            {
                case FrequencyType.SECONDLY: dt.Value = old.AddSeconds(Interval); break;
                case FrequencyType.MINUTELY: dt.Value = new DateTime(old.Year, old.Month, old.Day, old.Hour, old.Minute, old.Second, old.Kind).AddMinutes(Interval); break;
                case FrequencyType.HOURLY: dt.Value = new DateTime(old.Year, old.Month, old.Day, old.Hour, old.Minute, old.Second, old.Kind).AddHours(Interval); break;
                case FrequencyType.DAILY: dt.Value = new DateTime(old.Year, old.Month, old.Day, old.Hour, old.Minute, old.Second, old.Kind).AddDays(Interval); break;
                case FrequencyType.WEEKLY:
                    // How the week increments depends on the WKST indicated (defaults to Monday)
                    // So, basically, we determine the week of year using the necessary rules,
                    // and we increment the day until the week number matches our "goal" week number.
                    // So, if the current week number is 36, and our Interval is 2, then our goal
                    // week number is 38.
                    // NOTE: fixes RRULE12 eval.
                    int current = m_Calendar.GetWeekOfYear(old, System.Globalization.CalendarWeekRule.FirstFourDayWeek, Wkst),
                        last = m_Calendar.GetWeekOfYear(new DateTime(old.Year, 12, 31, 0, 0, 0, DateTimeKind.Local), System.Globalization.CalendarWeekRule.FirstFourDayWeek, Wkst),
                        goal = current + Interval;

                    // If the goal week is greater than the last week of the year, wrap it!
                    if (goal > last)
                        goal = goal - last;

                    while (current != goal)
                    {
                        old = old.AddDays(1);
                        current = m_Calendar.GetWeekOfYear(old, System.Globalization.CalendarWeekRule.FirstFourDayWeek, Wkst);
                    }                        
                    dt.Value = old;
                    break;
                case FrequencyType.MONTHLY: dt.Value = new DateTime(old.Year, old.Month, 1, old.Hour, old.Minute, old.Second, old.Kind).AddMonths(Interval); break;
                case FrequencyType.YEARLY: dt.Value = new DateTime(old.Year, 1, 1, old.Hour, old.Minute, old.Second, old.Kind).AddYears(Interval); break;
                default: throw new Exception("IncrementDate() failed.");
            }
        }

        #endregion

        #region Calculating Extra Occurrences

        protected ArrayList GetExtraOccurrences(Date_Time StartDate, Date_Time AbsEndDate)
        {
            ArrayList DateTimes = new ArrayList();
            Date_Time EndDate = new Date_Time(StartDate);
            AbsEndDate = AbsEndDate.AddSeconds(-1);
            IncrementDate(EndDate, 1);
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
            ArrayList months = new ArrayList();
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
                            (TC.Recur.Frequency == FrequencyType.MONTHLY &&
                            curr.Month == TC.StartDate.Value.Month) ||
                            (TC.Recur.Frequency == FrequencyType.YEARLY &&
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
            foreach (int hour in TC.Hours)
            {
                TC.Hour = hour;
                FillMinutes(TC);
            }
        }

        protected void FillMinutes(TimeCalculation TC)
        {
            foreach (int minute in TC.Minutes)
            {
                TC.Minute = minute;
                FillSeconds(TC);
            }
        }

        protected void FillSeconds(TimeCalculation TC)
        {
            foreach (int second in TC.Seconds)
            {
                TC.Second = second;
                TC.Calculate();
            }
        }

        protected ArrayList CalculateChildOccurrences(Date_Time StartDate, Date_Time EndDate)
        {
            TimeCalculation TC = new TimeCalculation(StartDate, EndDate, this);                        
            switch (Frequency)
            {
                case FrequencyType.YEARLY:
                    FillYearDays(TC);
                    FillByDay(TC);
                    FillMonths(TC);
                    break;
                case FrequencyType.WEEKLY: // Weeks can span across months, so we must fill months (Note: fixes RRULE10 eval)                    
                    FillMonths(TC);
                    break;
                case FrequencyType.MONTHLY:
                    FillDays(TC);
                    FillByDay(TC);
                    break;
                case FrequencyType.DAILY:
                    FillHours(TC);
                    break;
                case FrequencyType.HOURLY:
                    FillMinutes(TC);
                    break;
                case FrequencyType.MINUTELY:
                    FillSeconds(TC);
                    break;
                default:
                    throw new NotSupportedException("CalculateChildOccurrences() is not supported for a frequency of " + Frequency.ToString());                    
            }

            // Apply the BYSETPOS to the list of child occurrences
            // We do this before the dates are filtered by Start and End date
            // so that the BYSETPOS calculates correctly.
            // NOTE: fixes RRULE33 eval
            if (BySetPos.Count != 0)
            {
                ArrayList newDateTimes = new ArrayList();
                foreach (int pos in BySetPos)
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
                if ((Date_Time)TC.DateTimes[i] < StartDate ||
                    (Date_Time)TC.DateTimes[i] > EndDate)
                    TC.DateTimes.RemoveAt(i);
            }

            return TC.DateTimes;
        }

        #endregion        

        #endregion

        #region Public Methods

        public List<Date_Time> Evaluate(Date_Time StartDate, Date_Time FromDate, Date_Time ToDate)
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(StaticOccurrences);

            // If the Recur is restricted by COUNT, we need to evaluate just
            // after any static occurrences so it's correctly restricted to a
            // certain number. NOTE: fixes bug #13 and bug #16
            if (Count > 0)
            {
                FromDate = StartDate;
                foreach (Date_Time dt in StaticOccurrences)
                {
                    if (FromDate < dt)
                        FromDate = dt.AddSeconds(1);                    
                }
            }

            // Handle "UNTIL" values that are date-only. If we didn't change values here, "UNTIL" would
            // exclude the day it specifies, instead of the inclusive behaviour it should exhibit.
            if (Until != null && !Until.HasTime)
                Until.Value = new DateTime(Until.Year, Until.Month, Until.Day, 23, 59, 59, Until.Value.Kind);

            // Ignore recurrences that occur outside our time frame we're looking at
            if ((Until != null && FromDate > Until) ||
                ToDate < StartDate)
                return DateTimes;
            
            // Narrow down our time range further to avoid over-processing
            if (Until != null && Until < ToDate)
                ToDate = Until;
            if (StartDate > FromDate)
                FromDate = StartDate;

            // Create a temporary recurrence for populating 
            // missing information using the 'StartDate'.
            Recur r = new Recur();
            r.CopyFrom(this);

            // If an INTERVAL was not specified, default to 1
            r.EnsureInterval();
            
            // Fill in missing, necessary ByXXX values
            r.EnsureByXXXValues(StartDate);
                        
            // Get the occurrences
            foreach (Date_Time occurrence in r.GetOccurrences(FromDate.Copy(), ToDate, r.Count))
            {
                // NOTE:
                // Used to be DateTime.AddRange(r.GetOccurrences(FromDate.Copy(), ToDate, r.Count))
                // By doing it this way, fixes bug #19.
                if (!DateTimes.Contains(occurrence))
                    DateTimes.Add(occurrence);
            }            

            // Limit the count of returned recurrences
            if (Count != int.MinValue &&
                DateTimes.Count > Count)
                DateTimes.RemoveRange(Count, DateTimes.Count - Count);

            // Process the UNTIL, and make sure the DateTimes
            // occur between FromDate and ToDate
            for (int i = DateTimes.Count - 1; i >= 0; i--)
            {
                Date_Time dt = (Date_Time)DateTimes[i];
                if (dt > ToDate ||
                    dt < FromDate)
                    DateTimes.RemoveAt(i);
            }

            // Assign missing values
            foreach (Date_Time dt in DateTimes)
                dt.MergeWith(StartDate);

            // Ensure that DateTimes have an assigned time if they occur less than dailyB
            foreach (Date_Time dt in DateTimes)
            {
                if (Frequency < FrequencyType.DAILY)
                    dt.HasTime = true;
            }
            
            return DateTimes;
        }

        public bool CheckValidDate(Date_Time dt)
        {
            if (BySecond.Count != 0 && !BySecond.Contains(dt.Value.Second)) return false;
            if (ByMinute.Count != 0 && !ByMinute.Contains(dt.Value.Minute)) return false;
            if (ByHour.Count != 0 && !ByHour.Contains(dt.Value.Hour)) return false;
            if (ByDay.Count != 0)
            {
                bool found = false;
                foreach (DaySpecifier bd in ByDay)
                {
                    if (bd.CheckValidDate(this, dt))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            if (ByWeekNo.Count != 0)
            {
                bool found = false;
                int lastWeekOfYear = m_Calendar.GetWeekOfYear(new DateTime(dt.Value.Year, 12, 31), System.Globalization.CalendarWeekRule.FirstFourDayWeek, Wkst);
                int currWeekNo = m_Calendar.GetWeekOfYear(dt.Value, System.Globalization.CalendarWeekRule.FirstFourDayWeek, Wkst);
                foreach (int WeekNo in ByWeekNo)
                {
                    if ((WeekNo > 0 && WeekNo == currWeekNo) ||
                        (WeekNo < 0 && lastWeekOfYear + WeekNo + 1 == currWeekNo))
                        found = true;
                }
                if (!found)
                    return false;
            }
            if (ByMonth.Count != 0 && !ByMonth.Contains(dt.Value.Month)) return false;
            if (ByMonthDay.Count != 0)
            {
                // Handle negative days of month (count backwards from the end)
                // NOTE: fixes RRULE18 eval
                bool found = false;
                int DaysInMonth = m_Calendar.GetDaysInMonth(dt.Value.Year, dt.Value.Month);
                foreach (int Day in ByMonthDay)
                {
                    if ((Day > 0) && (Day == dt.Value.Day))
                        found = true;
                    else if ((Day < 0) && (DaysInMonth + Day + 1 == dt.Value.Day))
                        found = true;
                }

                if (!found)
                    return false;
            }
            if (ByYearDay.Count != 0)
            {
                // Handle negative days of year (count backwards from the end)
                // NOTE: fixes RRULE25 eval
                bool found = false;
                int DaysInYear = m_Calendar.GetDaysInYear(dt.Value.Year);
                DateTime baseDate = new DateTime(dt.Value.Year, 1, 1);

                foreach (int Day in ByYearDay)
                {
                    if (Day > 0 && dt.Value.Date == baseDate.AddDays(Day - 1))
                        found = true;
                    else if (Day < 0 && dt.Value.Date == baseDate.AddYears(1).AddDays(Day))
                        found = true;
                }
                if (!found)
                    return false;
            }
            return true;
        }

        #endregion

        #region Helper Classes

        protected class TimeCalculation
        {
            public Date_Time StartDate;
            public Date_Time EndDate;
            public Recur Recur;
            public int Year;
            public ArrayList ByDays;
            public ArrayList YearDays;
            public ArrayList Months;
            public ArrayList Days;
            public ArrayList Hours;
            public ArrayList Minutes;
            public ArrayList Seconds;
            public int Month;
            public int Day;
            public int Hour;
            public int Minute;
            public int Second;
            public ArrayList DateTimes;

            public TimeCalculation(Date_Time StartDate, Date_Time EndDate, Recur Recur)
            {
                this.StartDate = StartDate;
                this.EndDate = EndDate;
                this.Recur = Recur;

                Year = StartDate.Value.Year;
                Month = StartDate.Value.Month;
                Day = StartDate.Value.Day;
                Hour = StartDate.Value.Hour;
                Minute = StartDate.Value.Minute;
                Second = StartDate.Value.Second;

                YearDays = new ArrayList(Recur.ByYearDay);
                ByDays = new ArrayList(Recur.ByDay);
                Months = new ArrayList(Recur.ByMonth);
                Days = new ArrayList(Recur.ByMonthDay);
                Hours = new ArrayList(Recur.ByHour);
                Minutes = new ArrayList(Recur.ByMinute);
                Seconds = new ArrayList(Recur.BySecond);
                DateTimes = new ArrayList();

                // Only check what months and days are possible for
                // the week's period of time we're evaluating
                // NOTE: fixes RRULE10 evaluation                
                if (Recur.Frequency == FrequencyType.WEEKLY)
                {                    
                    if (Months.Count == 0)
                    {
                        Months.Add(StartDate.Value.Month);
                        if (StartDate.Value.Month != EndDate.Value.Month)
                            Months.Add(EndDate.Value.Month);
                    }
                    if (Days.Count == 0)
                    {
                        DateTime dt = StartDate.Value;
                        while (dt < EndDate.Value)
                        {
                            Days.Add(dt.Day);
                            dt = dt.AddDays(1);
                        }
                        Days.Add(EndDate.Value.Day);
                    }
                }
                else
                {
                    if (Months.Count == 0) Months.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });                                
                    if (Days.Count == 0) Days.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 });
                }
            }

            public Date_Time CurrentDateTime
            {
                get
                {
                    Date_Time dt = null;
                    // Account for negative days of month (count backwards from the end of the month)
                    // NOTE: fixes RRULE18 evaluation
                    if (Day > 0)
                        // Changed from DateTimeKind.Local to StartDate.Kind
                        // NOTE: fixes bug #20
                        dt = new Date_Time(new DateTime(Year, Month, Day, Hour, Minute, Second, StartDate.Kind));
                    else
                        dt = new Date_Time(new DateTime(Year, Month, 1, Hour, Minute, Second, StartDate.Kind).AddMonths(1).AddDays(Day));

                    // Inherit time zone info, etc. from the start date
                    dt.MergeWith(StartDate);
                    return dt;
                }
            }

            public void Calculate()
            {
                try
                {
                    // Make sure our day falls in the valid date range
                    if (Recur.CheckValidDate(CurrentDateTime) &&
                        // Ensure the DateTime hasn't already been calculated (NOTE: fixes RRULE34 eval)
                        !DateTimes.Contains(CurrentDateTime))
                        DateTimes.Add(CurrentDateTime);
                }
                catch (ArgumentOutOfRangeException ex) { }
            }
        }

        #endregion
    }
}
