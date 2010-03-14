using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;
using System.IO;

namespace DDay.iCal
{
    /// <summary>
    /// An iCalendar representation of the <c>RRULE</c> property.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "RecurrencePattern", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public partial class RecurrencePattern :
        EncodableDataType,
        IRecurrencePattern
    {
        #region Private Fields

#if !SILVERLIGHT
        [NonSerialized]
#endif
        private FrequencyType _Frequency;
        private iCalDateTime _Until;
        private int _Count = int.MinValue;
        private int _Interval = int.MinValue;
        private IList<int> _BySecond = new List<int>();
        private IList<int> _ByMinute = new List<int>();
        private IList<int> _ByHour = new List<int>();
        private IList<IDaySpecifier> _ByDay = new List<IDaySpecifier>();
        private IList<int> _ByMonthDay = new List<int>();
        private IList<int> _ByYearDay = new List<int>();
        private IList<int> _ByWeekNo = new List<int>();
        private IList<int> _ByMonth = new List<int>();
        private IList<int> _BySetPosition = new List<int>();
        private DayOfWeek _WeekStart = DayOfWeek.Monday;
        private IList<iCalDateTime> _StaticOccurrences = new List<iCalDateTime>();
        private RecurrenceRestrictionType? _RestrictionType = null;
        private RecurrenceEvaluationModeType? _EvaluationMode = null;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public FrequencyType Frequency
        {
            get { return _Frequency; }
            set { _Frequency = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public iCalDateTime Until
        {
            get { return _Until; }
            set { _Until = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        public int Interval
        {
            get
            {
                if (_Interval == int.MinValue)
                    return 1;
                return _Interval;
            }
            set { _Interval = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 5)]
#endif
        public IList<int> BySecond
        {
            get { return _BySecond; }
            set { _BySecond = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 6)]
#endif
        public IList<int> ByMinute
        {
            get { return _ByMinute; }
            set { _ByMinute = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 7)]
#endif
        public IList<int> ByHour
        {
            get { return _ByHour; }
            set { _ByHour = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 8)]
#endif
        public IList<IDaySpecifier> ByDay
        {
            get { return _ByDay; }
            set { _ByDay = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 9)]
#endif
        public IList<int> ByMonthDay
        {
            get { return _ByMonthDay; }
            set { _ByMonthDay = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 10)]
#endif
        public IList<int> ByYearDay
        {
            get { return _ByYearDay; }
            set { _ByYearDay = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 11)]
#endif
        public IList<int> ByWeekNo
        {
            get { return _ByWeekNo; }
            set { _ByWeekNo = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 12)]
#endif
        public IList<int> ByMonth
        {
            get { return _ByMonth; }
            set { _ByMonth = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 13)]
#endif
        public IList<int> BySetPosition
        {
            get { return _BySetPosition; }
            set { _BySetPosition = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 14)]
#endif
        public DayOfWeek WeekStart
        {
            get { return _WeekStart; }
            set { _WeekStart = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 15)]
#endif
        public IList<iCalDateTime> StaticOccurrences
        {
            get { return _StaticOccurrences; }
            set { _StaticOccurrences = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 16)]
#endif
        public RecurrenceRestrictionType RestrictionType
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_RestrictionType != null &&
                    _RestrictionType.HasValue)
                    return _RestrictionType.Value;

                // FIXME: use defaults from the calendar instead
                //else if (Calendar != null)
                //    return Calendar.RecurrenceRestriction;
                else
                    return RecurrenceRestrictionType.Default;
            }
            set { _RestrictionType = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 17)]
#endif
        public RecurrenceEvaluationModeType EvaluationMode
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_EvaluationMode != null &&
                    _EvaluationMode.HasValue)
                    return _EvaluationMode.Value;
                // FIXME: use calendar settings instead.
                // (reimplement this)
                //else if (Calendar != null)
                //    return Calendar.RecurrenceEvaluationMode;
                else
                    return RecurrenceEvaluationModeType.Default;
            }
            set { _EvaluationMode = value; }
        }

        #endregion

        #region Constructors

        public RecurrencePattern()
        {   
        }

        public RecurrencePattern(FrequencyType frequency) : this(frequency, 1)
        {
        }

        public RecurrencePattern(FrequencyType frequency, int interval)
        {
            Frequency = frequency;
            Interval = interval;
        }

        public RecurrencePattern(string value)
        {
            if (value != null)
            {
                DDay.iCal.Serialization.iCalendar.RecurrencePatternSerializer serializer = new DDay.iCal.Serialization.iCalendar.RecurrencePatternSerializer();
                CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
            }
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is RecurrencePattern)
            {
                RecurrencePattern r = (RecurrencePattern)obj;
                if (!CollectionEquals(r.ByDay, ByDay) ||
                    !CollectionEquals(r.ByHour, ByHour) ||
                    !CollectionEquals(r.ByMinute, ByMinute) ||
                    !CollectionEquals(r.ByMonth, ByMonth) ||
                    !CollectionEquals(r.ByMonthDay, ByMonthDay) ||
                    !CollectionEquals(r.BySecond, BySecond) ||
                    !CollectionEquals(r.BySetPosition, BySetPosition) ||
                    !CollectionEquals(r.ByWeekNo, ByWeekNo) ||
                    !CollectionEquals(r.ByYearDay, ByYearDay))
                    return false;
                if (r.Count != Count) return false;
                if (r.Frequency != Frequency) return false;
                if (r.Interval != Interval) return false;
                if (r.Until.IsAssigned)
                {
                    if (!r.Until.Equals(Until))
                        return false;
                }
                else if (Until.IsAssigned)
                    return false;
                if (r.WeekStart != WeekStart) return false;
                return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode =
                ByDay.GetHashCode() ^ ByHour.GetHashCode() ^ ByMinute.GetHashCode() ^
                ByMonth.GetHashCode() ^ ByMonthDay.GetHashCode() ^ BySecond.GetHashCode() ^
                BySetPosition.GetHashCode() ^ ByWeekNo.GetHashCode() ^ ByYearDay.GetHashCode() ^
                Count.GetHashCode() ^ Frequency.GetHashCode();

            if (Interval.Equals(1))
                hashCode ^= 0x1;
            else hashCode ^= Interval.GetHashCode();

            hashCode ^= Until.GetHashCode();
            hashCode ^= WeekStart.GetHashCode();
            return hashCode;
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IRecurrencePattern)
            {
                IRecurrencePattern r = (IRecurrencePattern)obj;

                Frequency = r.Frequency;
                Until = r.Until;
                Count = r.Count;
                Interval = r.Interval;
                BySecond = new List<int>(r.BySecond);
                ByMinute = new List<int>(r.ByMinute);
                ByHour = new List<int>(r.ByHour);
                ByDay = new List<IDaySpecifier>(r.ByDay);
                ByMonthDay = new List<int>(r.ByMonthDay);
                ByYearDay = new List<int>(r.ByYearDay);
                ByWeekNo = new List<int>(r.ByWeekNo);
                ByMonth = new List<int>(r.ByMonth);
                BySetPosition = new List<int>(r.BySetPosition);
                WeekStart = r.WeekStart;
                RestrictionType = r.RestrictionType;
                EvaluationMode = r.EvaluationMode;
            }            
        }

        private bool CollectionEquals<T>(ICollection<T> c1, ICollection<T> c2)
        {
            // NOTE: fixes a bug where collections weren't properly compared
            if (c1 == null ||
                c2 == null)
            {
                if (c1 == c2)
                    return true;
                else return false;
            }
            if (!c1.Count.Equals(c2.Count))
                return false;

            IEnumerator e1 = c1.GetEnumerator();
            IEnumerator e2 = c2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                if (!object.Equals(e1.Current, e2.Current))
                    return false;
            }
            return true;
        }

        #endregion

        #region Protected Methods

       
        #endregion

        #region Public Methods
        
        

        /// <summary>
        /// [Deprecated]: Use IsValidDate() instead.
        /// </summary>
        [Obsolete("Use IsValidDate() instead.")]
        public bool CheckValidDate(iCalDateTime dt) { return IsValidDate(dt); }

        /// <summary>
        /// Returns true if <paramref name="dt"/> is a date/time that aligns to (occurs within)
        /// the recurrence pattern of this Recur, false otherwise.
        /// </summary>
        public bool IsValidDate(iCalDateTime dt)
        {
            IEvaluator evaluator = GetService(typeof(IEvaluator)) as IEvaluator;

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
                int lastWeekOfYear = evaluator.Calendar.GetWeekOfYear(new DateTime(dt.Value.Year, 12, 31), System.Globalization.CalendarWeekRule.FirstFourDayWeek, WeekStart);
                int currWeekNo = evaluator.Calendar.GetWeekOfYear(dt.Value, System.Globalization.CalendarWeekRule.FirstFourDayWeek, WeekStart);
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
                int DaysInMonth = evaluator.Calendar.GetDaysInMonth(dt.Value.Year, dt.Value.Month);
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
                int DaysInYear = evaluator.Calendar.GetDaysInYear(dt.Value.Year);
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

        
    }
}
