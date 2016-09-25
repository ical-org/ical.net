using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ical.net.Evaluation;
using ical.net.Interfaces.DataTypes;
using ical.net.Interfaces.General;
using ical.net.Serialization.iCalendar.Serializers.DataTypes;

namespace ical.net.DataTypes
{
    /// <summary>
    /// An iCalendar representation of the <c>RRULE</c> property.
    /// </summary>
    public class RecurrencePattern : EncodableDataType, IRecurrencePattern
    {
        private int _interval = int.MinValue;
        private RecurrenceRestrictionType? _restrictionType;
        private RecurrenceEvaluationModeType? _evaluationMode;

        public FrequencyType Frequency { get; set; }

        public DateTime Until { get; set; } = DateTime.MinValue;

        public int Count { get; set; } = int.MinValue;

        public int Interval
        {
            get
            {
                return _interval == int.MinValue
                    ? 1
                    : _interval;
            }
            set { _interval = value; }
        }

        public IList<int> BySecond { get; set; } = new List<int>(16);

        public IList<int> ByMinute { get; set; } = new List<int>(16);

        public IList<int> ByHour { get; set; } = new List<int>(16);

        public IList<IWeekDay> ByDay { get; set; } = new List<IWeekDay>(16);

        public IList<int> ByMonthDay { get; set; } = new List<int>(16);

        public IList<int> ByYearDay { get; set; } = new List<int>(16);

        public IList<int> ByWeekNo { get; set; } = new List<int>(16);

        public IList<int> ByMonth { get; set; } = new List<int>(16);

        public IList<int> BySetPosition { get; set; } = new List<int>(16);

        public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

        public RecurrenceRestrictionType RestrictionType
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_restrictionType != null)
                {
                    return _restrictionType.Value;
                }
                return Calendar?.RecurrenceRestriction ?? RecurrenceRestrictionType.Default;
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
                return Calendar?.RecurrenceEvaluationMode ?? RecurrenceEvaluationModeType.Default;
            }
            set { _evaluationMode = value; }
        }

        public RecurrencePattern()
        {
            SetService(new RecurrencePatternEvaluator(this));
        }

        public RecurrencePattern(FrequencyType frequency) : this(frequency, 1) {}

        public RecurrencePattern(FrequencyType frequency, int interval) : this()
        {
            Frequency = frequency;
            Interval = interval;
        }

        public RecurrencePattern(string value) : this()
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            var serializer = new RecurrencePatternSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RecurrencePattern))
            {
                return false;
            }

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
            if (r.Until != DateTime.MinValue && !r.Until.Equals(Until))
            {
                return false;
            }
            if (Until != DateTime.MinValue)
            {
                return false;
            }
            return r.FirstDayOfWeek == FirstDayOfWeek;
        }

        public override string ToString()
        {
            var serializer = new RecurrencePatternSerializer();
            return serializer.SerializeToString(this);
        }

        //ToDo: This needs a memberwise hash code implementation
        public override int GetHashCode()
        {
            var hashCode = ByDay.GetHashCode() ^ ByHour.GetHashCode() ^ ByMinute.GetHashCode() ^ ByMonth.GetHashCode()
                ^ ByMonthDay.GetHashCode() ^ BySecond.GetHashCode() ^ BySetPosition.GetHashCode() ^ ByWeekNo.GetHashCode()
                ^ ByYearDay.GetHashCode() ^ Count.GetHashCode() ^ Frequency.GetHashCode();

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
            if (!(obj is IRecurrencePattern))
            {
                return;
            }

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

        private static bool CollectionEquals<T>(IEnumerable<T> c1, IEnumerable<T> c2) => c1.SequenceEqual(c2);
    }
}