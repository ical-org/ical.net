using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace DDay.iCal.Serialization.iCalendar
{
    public class RecurrencePatternSerializer :
        EncodableDataTypeSerializer
    {
        #region Static Public Methods

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

        #endregion

        #region Static Protected Methods

        static protected void AddInt32Values(IList<int> list, string value)
        {
            string[] values = value.Split(',');
            foreach (string v in values)
                list.Add(Convert.ToInt32(v));
        }

        #endregion

        #region Content Validation

        virtual public void CheckRange(string name, ICollection<int> values, int min, int max)
        {
            bool allowZero = (min == 0 || max == 0) ? true : false;
            foreach (int value in values)
                CheckRange(name, value, min, max, allowZero);
        }

        virtual public void CheckRange(string name, int value, int min, int max)
        {
            CheckRange(name, value, min, max, (min == 0 || max == 0) ? true : false);
        }

        virtual public void CheckRange(string name, int value, int min, int max, bool allowZero)
        {
            if (value != int.MinValue && (value < min || value > max || (!allowZero && value == 0)))
                throw new ArgumentException(name + " value " + value + " is out of range. Valid values are between " + min + " and " + max + (allowZero ? "" : ", excluding zero (0)") + ".");
        }

        virtual public void CheckMutuallyExclusive<T, U>(string name1, string name2, T obj1, U obj2)
        {
            if (object.Equals(obj1, default(T)) || object.Equals(obj2, default(U)))
                return;
            else
            {
                // If the object is MinValue instead of its default, consider
                // that to be unassigned.
                bool 
                    isMin1 = false,
                    isMin2 = false;

                Type 
                    t1 = obj1.GetType(),
                    t2 = obj2.GetType();

                FieldInfo fi1 = t1.GetField("MinValue");
                FieldInfo fi2 = t1.GetField("MinValue");
                
                isMin1 = fi1 != null && obj1.Equals(fi1.GetValue(null));
                isMin2 = fi2 != null && obj2.Equals(fi2.GetValue(null));
                if (isMin1 || isMin2)
                    return;                    
            }

            throw new ArgumentException("Both " + name1 + " and " + name2 + " cannot be supplied together; they are mutually exclusive.");
        }

        #endregion

        #region Private Methods

        private void SerializeByValue(List<string> aggregate, IList<int> byValue, string name)
        {
            if (byValue.Count > 0)
            {
                List<string> byValues = new List<string>();
                foreach (int i in byValue)
                    byValues.Add(i.ToString());

                aggregate.Add(name + "=" + string.Join(",", byValues.ToArray()));
            }
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(RecurrencePattern); }
        }

        public override string SerializeToString(object obj)
        {
            IRecurrencePattern recur = obj as IRecurrencePattern;
            ISerializerFactory factory = GetService<ISerializerFactory>();
            if (recur != null && factory != null)
            {
                // Push the recurrence pattern onto the serialization stack
                SerializationContext.Push(recur);

                List<string> values = new List<string>();

                values.Add("FREQ=" + Enum.GetName(typeof(FrequencyType), recur.Frequency).ToUpper());

                //-- FROM RFC2445 --
                //The INTERVAL rule part contains a positive integer representing how
                //often the recurrence rule repeats. The default value is "1", meaning
                //every second for a SECONDLY rule, or every minute for a MINUTELY
                //rule, every hour for an HOURLY rule, every day for a DAILY rule,
                //every week for a WEEKLY rule, every month for a MONTHLY rule and
                //every year for a YEARLY rule.
                int interval = recur.Interval;
                if (interval == int.MinValue)
                    interval = 1;

                if (interval != 1)
                    values.Add("INTERVAL=" + interval);

                if (recur.Until != null)
                {
                    IStringSerializer serializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
                    if (serializer != null)
                        values.Add("UNTIL=" + serializer.SerializeToString(recur.Until));
                }

                if (recur.FirstDayOfWeek != DayOfWeek.Monday)
                    values.Add("WKST=" + Enum.GetName(typeof(DayOfWeek), recur.FirstDayOfWeek).ToUpper().Substring(0, 2));

                if (recur.Count != int.MinValue)
                    values.Add("COUNT=" + recur.Count);

                if (recur.ByDay.Count > 0)
                {
                    List<string> bydayValues = new List<string>();

                    IStringSerializer serializer = factory.Build(typeof(IWeekDay), SerializationContext) as IStringSerializer;
                    if (serializer != null)
                    {
                        foreach (WeekDay byday in recur.ByDay)
                            bydayValues.Add(serializer.SerializeToString(byday));
                    }

                    values.Add("BYDAY=" + string.Join(",", bydayValues.ToArray()));
                }

                SerializeByValue(values, recur.ByHour, "BYHOUR");
                SerializeByValue(values, recur.ByMinute, "BYMINUTE");
                SerializeByValue(values, recur.ByMonth, "BYMONTH");
                SerializeByValue(values, recur.ByMonthDay, "BYMONTHDAY");
                SerializeByValue(values, recur.BySecond, "BYSECOND");
                SerializeByValue(values, recur.BySetPosition, "BYSETPOS");
                SerializeByValue(values, recur.ByWeekNo, "BYWEEKNO");
                SerializeByValue(values, recur.ByYearDay, "BYYEARDAY");

                // Pop the recurrence pattern off the serialization stack
                SerializationContext.Pop();
                
                return Encode(recur, string.Join(";", values.ToArray()));
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            // Instantiate the data type
            IRecurrencePattern r = CreateAndAssociate() as IRecurrencePattern;
            ISerializerFactory factory = GetService<ISerializerFactory>();

            if (r != null && factory != null)
            {
                // Decode the value, if necessary
                value = Decode(r, value);

                Match match = Regex.Match(value, @"FREQ=(SECONDLY|MINUTELY|HOURLY|DAILY|WEEKLY|MONTHLY|YEARLY);?(.*)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // Parse the frequency type
                    r.Frequency = (FrequencyType)Enum.Parse(typeof(FrequencyType), match.Groups[1].Value, true);

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
                                case "UNTIL":
                                    {
                                        IStringSerializer serializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
                                        if (serializer != null)
                                            r.Until = serializer.Deserialize(new StringReader(keyValue)) as IDateTime;
                                    } break;
                                case "COUNT": r.Count = Convert.ToInt32(keyValue); break;
                                case "INTERVAL": r.Interval = Convert.ToInt32(keyValue); break;
                                case "BYSECOND": AddInt32Values(r.BySecond, keyValue); break;
                                case "BYMINUTE": AddInt32Values(r.ByMinute, keyValue); break;
                                case "BYHOUR": AddInt32Values(r.ByHour, keyValue); break;
                                case "BYDAY":
                                    {
                                        string[] days = keyValue.Split(',');
                                        foreach (string day in days)
                                            r.ByDay.Add(new WeekDay(day));
                                    } break;
                                case "BYMONTHDAY": AddInt32Values(r.ByMonthDay, keyValue); break;
                                case "BYYEARDAY": AddInt32Values(r.ByYearDay, keyValue); break;
                                case "BYWEEKNO": AddInt32Values(r.ByWeekNo, keyValue); break;
                                case "BYMONTH": AddInt32Values(r.ByMonth, keyValue); break;
                                case "BYSETPOS": AddInt32Values(r.BySetPosition, keyValue); break;
                                case "WKST": r.FirstDayOfWeek = GetDayOfWeek(keyValue); break;
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
                        case "second": r.Frequency = FrequencyType.Secondly; break;
                        case "minute": r.Frequency = FrequencyType.Minutely; break;
                        case "hour": r.Frequency = FrequencyType.Hourly; break;
                        case "day": r.Frequency = FrequencyType.Daily; break;
                        case "week": r.Frequency = FrequencyType.Weekly; break;
                        case "month": r.Frequency = FrequencyType.Monthly; break;
                        case "year": r.Frequency = FrequencyType.Yearly; break;
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
                                            case FrequencyType.Yearly:
                                                r.ByYearDay.Add(num);
                                                break;
                                            case FrequencyType.Monthly:
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
                                WeekDay ds = new WeekDay((DayOfWeek)Enum.Parse(typeof(DayOfWeek), capture.Value, true));
                                ds.Offset = num;
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

                            r.Until = new iCalDateTime(dt);
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
                else
                {
                    // Couldn't parse the object, return null!
                    r = null;
                }

                if (r != null)
                {
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
                    CheckRange("BYSETPOS", r.BySetPosition, -366, 366);
                }
            }

            return r;            
        }

        #endregion
    }
}
