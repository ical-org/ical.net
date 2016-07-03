using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.Evaluation;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An iCalendar representation of the <c>RRULE</c> property.
    /// </summary>
    public class RecurrencePattern : EncodableDataType, IRecurrencePattern
    {
        [NonSerialized] private FrequencyType _frequency;
        private DateTime _until = DateTime.MinValue;
        private int _count = int.MinValue;
        private int _interval = int.MinValue;
        private IList<int> _bySecond = new List<int>(128);
        private IList<int> _byMinute = new List<int>(128);
        private IList<int> _byHour = new List<int>(128);
        private IList<IWeekDay> _byDay = new List<IWeekDay>(128);
        private IList<int> _byMonthDay = new List<int>(128);
        private IList<int> _byYearDay = new List<int>(128);
        private IList<int> _byWeekNo = new List<int>(128);
        private IList<int> _byMonth = new List<int>(128);
        private IList<int> _bySetPosition = new List<int>(128);
        private DayOfWeek _firstDayOfWeek = DayOfWeek.Monday;
        private RecurrenceRestrictionType? _restrictionType;
        private RecurrenceEvaluationModeType? _evaluationMode;

        public FrequencyType Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        public DateTime Until
        {
            get { return _until; }
            set { _until = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public int Interval
        {
            get
            {
                if (_interval == int.MinValue)
                {
                    return 1;
                }
                return _interval;
            }
            set { _interval = value; }
        }

        public IList<int> BySecond
        {
            get { return _bySecond; }
            set { _bySecond = value; }
        }

        public IList<int> ByMinute
        {
            get { return _byMinute; }
            set { _byMinute = value; }
        }

        public IList<int> ByHour
        {
            get { return _byHour; }
            set { _byHour = value; }
        }

        public IList<IWeekDay> ByDay
        {
            get { return _byDay; }
            set { _byDay = value; }
        }

        public IList<int> ByMonthDay
        {
            get { return _byMonthDay; }
            set { _byMonthDay = value; }
        }

        public IList<int> ByYearDay
        {
            get { return _byYearDay; }
            set { _byYearDay = value; }
        }

        public IList<int> ByWeekNo
        {
            get { return _byWeekNo; }
            set { _byWeekNo = value; }
        }

        public IList<int> ByMonth
        {
            get { return _byMonth; }
            set { _byMonth = value; }
        }

        public IList<int> BySetPosition
        {
            get { return _bySetPosition; }
            set { _bySetPosition = value; }
        }

        public DayOfWeek FirstDayOfWeek
        {
            get { return _firstDayOfWeek; }
            set { _firstDayOfWeek = value; }
        }

        public RecurrenceRestrictionType RestrictionType
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_restrictionType != null)
                {
                    return _restrictionType.Value;
                }
                if (Calendar != null)
                {
                    return Calendar.RecurrenceRestriction;
                }
                return RecurrenceRestrictionType.Default;
            }
            set { _restrictionType = value; }
        }

        public RecurrenceEvaluationModeType EvaluationMode
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_evaluationMode != null)
                {
                    return _evaluationMode.Value;
                }
                if (Calendar != null)
                {
                    return Calendar.RecurrenceEvaluationMode;
                }
                return RecurrenceEvaluationModeType.Default;
            }
            set { _evaluationMode = value; }
        }

        ///// <summary>
        ///// Returns the next occurrence of the pattern,
        ///// given a valid previous occurrence, <paramref name="lastOccurrence"/>.
        ///// As long as the recurrence pattern is valid, and
        ///// <paramref name="lastOccurrence"/> is a valid previous 
        ///// occurrence within the pattern, this will return the
        ///// next occurrence.  NOTE: This will not give accurate results
        ///// when COUNT or BYSETVAL are used.
        ///// </summary>
        //virtual public IPeriod GetNextOccurrence(IDateTime lastOccurrence)
        //{
        //    RecurrencePatternEvaluator evaluator = GetService<RecurrencePatternEvaluator>();
        //    if (evaluator != null)
        //        return evaluator.GetNext(lastOccurrence);

        //    return null;
        //}

        public RecurrencePattern()
        {
            Initialize();
        }

        public RecurrencePattern(FrequencyType frequency) : this(frequency, 1) {}

        public RecurrencePattern(FrequencyType frequency, int interval) : this()
        {
            Frequency = frequency;
            Interval = interval;
        }

        public RecurrencePattern(string value) : this()
        {
            if (value != null)
            {
                var serializer = new RecurrencePatternSerializer();
                CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
            }
        }

        private void Initialize()
        {
            SetService(new RecurrencePatternEvaluator(this));
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override bool Equals(object obj)
        {
            if (obj is RecurrencePattern)
            {
                var r = (RecurrencePattern) obj;
                if (!CollectionEquals(r.ByDay, ByDay) || !CollectionEquals(r.ByHour, ByHour) || !CollectionEquals(r.ByMinute, ByMinute) ||
                    !CollectionEquals(r.ByMonth, ByMonth) || !CollectionEquals(r.ByMonthDay, ByMonthDay) || !CollectionEquals(r.BySecond, BySecond) ||
                    !CollectionEquals(r.BySetPosition, BySetPosition) || !CollectionEquals(r.ByWeekNo, ByWeekNo) || !CollectionEquals(r.ByYearDay, ByYearDay))
                {
                    return false;
                }
                if (r.Count != Count)
                {
                    return false;
                }
                if (r.Frequency != Frequency)
                {
                    return false;
                }
                if (r.Interval != Interval)
                {
                    return false;
                }
                if (r.Until != DateTime.MinValue)
                {
                    if (!r.Until.Equals(Until))
                    {
                        return false;
                    }
                }
                else if (Until != DateTime.MinValue)
                {
                    return false;
                }
                if (r.FirstDayOfWeek != FirstDayOfWeek)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            var serializer = new RecurrencePatternSerializer();
            return serializer.SerializeToString(this);
        }

        public override int GetHashCode()
        {
            var hashCode = ByDay.GetHashCode() ^ ByHour.GetHashCode() ^ ByMinute.GetHashCode() ^ ByMonth.GetHashCode() ^ ByMonthDay.GetHashCode() ^
                           BySecond.GetHashCode() ^ BySetPosition.GetHashCode() ^ ByWeekNo.GetHashCode() ^ ByYearDay.GetHashCode() ^ Count.GetHashCode() ^
                           Frequency.GetHashCode();

            if (Interval.Equals(1))
            {
                hashCode ^= 0x1;
            }
            else
            {
                hashCode ^= Interval.GetHashCode();
            }

            hashCode ^= Until.GetHashCode();
            hashCode ^= FirstDayOfWeek.GetHashCode();
            return hashCode;
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IRecurrencePattern)
            {
                var r = (IRecurrencePattern) obj;

                Frequency = r.Frequency;
                Until = r.Until;
                Count = r.Count;
                Interval = r.Interval;
                BySecond = new List<int>(r.BySecond);
                ByMinute = new List<int>(r.ByMinute);
                ByHour = new List<int>(r.ByHour);
                ByDay = new List<IWeekDay>(r.ByDay);
                ByMonthDay = new List<int>(r.ByMonthDay);
                ByYearDay = new List<int>(r.ByYearDay);
                ByWeekNo = new List<int>(r.ByWeekNo);
                ByMonth = new List<int>(r.ByMonth);
                BySetPosition = new List<int>(r.BySetPosition);
                FirstDayOfWeek = r.FirstDayOfWeek;
                RestrictionType = r.RestrictionType;
                EvaluationMode = r.EvaluationMode;
            }
        }

        private bool CollectionEquals<T>(IEnumerable<T> c1, IEnumerable<T> c2) => c1.SequenceEqual(c2);
    }
}