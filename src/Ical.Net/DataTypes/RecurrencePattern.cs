using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.Evaluation;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes
{

    public static class XRecurrence
    {
        public static void IncrementDate(this RecurrencePattern pattern, ref DateTime dt) => pattern.IncrementDate_(ref dt, pattern.Interval);
        public static void DecrementDate(this RecurrencePattern pattern, ref DateTime dt) => pattern.IncrementDate_(ref dt, -pattern.Interval);
        static void IncrementDate_(this RecurrencePattern pattern, ref DateTime dt, int interval)
        {
            // FIXME: use a more specific exception.
            if (interval == 0)
            {
                throw new Exception("Cannot evaluate with an interval of zero.  Please use an interval other than zero.");
            }

            var old = dt;
            dt = pattern.Frequency switch
            {
                FrequencyType.Secondly => old.AddSeconds(interval),
                FrequencyType.Minutely => old.AddMinutes(interval),
                FrequencyType.Hourly => old.AddHours(interval),
                FrequencyType.Daily => old.AddDays(interval),
                FrequencyType.Weekly => old.AddWeeks(interval, pattern.FirstDayOfWeek),
                FrequencyType.Monthly => old.AddDays(-old.Day + 1).AddMonths(interval),
                FrequencyType.Yearly => old.AddDays(-old.DayOfYear + 1).AddYears(interval),
                _ => throw new Exception(
                    "FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.")
            };
        }

    }
    /// <summary> An iCalendar representation of the <c>RRULE</c> property. https://tools.ietf.org/html/rfc5545#section-3.3.10 </summary>
    /// <remarks>
    /// Multiple Patterns are combined into a <see cref="CalendarComponents.CalendarEvent"/>
    /// This is closely related to the Unix Cron Pattern, but with additional <see cref="Until"/> Time.
    /// </remarks>
    public class RecurrencePattern : EncodableDataType
    {
        private int _interval = int.MinValue;
        private RecurrenceRestrictionType? _restrictionType;
        private RecurrenceEvaluationModeType? _evaluationMode;

        /// <summary> Period of this Pattern; </summary>
        /// <remarks>
        /// defines when this Event Happens, together with these Lists of Filters:
        /// * <see cref="BySecond"/>
        /// * <see cref="ByMinute"/>
        /// * <see cref="ByHour"/>
        /// * <see cref="ByDay"/>
        /// * <see cref="ByMonthDay"/>
        /// * <see cref="ByYearDay"/>
        /// * <see cref="ByWeekNo"/>
        /// * <see cref="BySetPosition"/>
        /// </remarks>
        public FrequencyType Frequency { get; set; }

        private DateTime _until = DateTime.MinValue;
        public DateTime Until
        {
            get => _until;
            set
            {
                if (_until == value && _until.Kind == value.Kind)
                {
                    return;
                }

                _until = value;
            }
        }

        /// <summary> Specifies how often this recurrence should repeat. </summary>
        /// <remarks>
        /// <see cref="int.MinValue"/> specifies unlimited Count.
        /// </remarks>
        public int Count { get; set; } = int.MinValue;

        /// <summary> Specifies the Period Length of the recurrence. </summary>
        /// <remarks>
        /// - 1 = every
        /// - 2 = every second
        /// - 3 = every third
        /// </remarks>
        public int Interval
        {
            get => _interval == int.MinValue
                ? 1
                : _interval;
            set => _interval = value;
        }

        /// <summary> List of Seconds from 0 to 59 when this Event recurs </summary>
        public List<int> BySecond { get; set; } = new List<int>();

        /// <summary> The ordinal minutes of the hour associated with this recurrence pattern. Valid values are 0-59. </summary>
        public List<int> ByMinute { get; set; } = new List<int>();

        /// <summary> The ordinal hours of the day associated with this recurrence pattern. Valid values are 0-23. </summary>
        public List<int> ByHour { get; set; } = new List<int>();

        /// <summary> The ordinal days of the week associated with this recurrence pattern. Valid values are 0-23. </summary>
        public List<WeekDay> ByDay { get; set; } = new List<WeekDay>();

        /// <summary> The ordinal days of the month associated with this recurrence pattern. Valid values are 1-31. </summary>
        public List<int> ByMonthDay { get; set; } = new List<int>();

        /// <summary>
        /// The ordinal days of the year associated with this recurrence pattern. Something recurring on the first day of the year would be a list containing
        /// 1, and would also be New Year's Day.
        /// </summary>
        public List<int> ByYearDay { get; set; } = new List<int>();

        /// <summary>
        /// The ordinal week of the year. Valid values are -53 to +53.
        /// Negative values count backwards from the end of the specified year.
        /// A week is defined by ISO.8601.2004
        /// </summary>
        public List<int> ByWeekNo { get; set; } = new List<int>();

        /// <summary>
        /// List of months in the year associated with this rule. Valid values are 1 through 12.
        /// </summary>
        public List<int> ByMonth { get; set; } = new List<int>();

        public List<int> BySetPosition { get; set; } = new List<int>();

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
            set => _restrictionType = value;
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
            set => _evaluationMode = value;
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

        public override string ToString()
        {
            var serializer = new RecurrencePatternSerializer();
            return serializer.SerializeToString(this);
        }

        protected bool Equals(RecurrencePattern other) => (Interval == other.Interval)
            && RestrictionType == other.RestrictionType
            && EvaluationMode == other.EvaluationMode
            && Frequency == other.Frequency
            && Until.Equals(other.Until)
            && Count == other.Count
            && (FirstDayOfWeek == other.FirstDayOfWeek)
            && CollectionEquals(BySecond, other.BySecond)
            && CollectionEquals(ByMinute, other.ByMinute)
            && CollectionEquals(ByHour, other.ByHour)
            && CollectionEquals(ByDay, other.ByDay)
            && CollectionEquals(ByMonthDay, other.ByMonthDay)
            && CollectionEquals(ByYearDay, other.ByYearDay)
            && CollectionEquals(ByWeekNo, other.ByWeekNo)
            && CollectionEquals(ByMonth, other.ByMonth)
            && CollectionEquals(BySetPosition, other.BySetPosition);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RecurrencePattern)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Interval.GetHashCode();
                hashCode = (hashCode * 397) ^ RestrictionType.GetHashCode();
                hashCode = (hashCode * 397) ^ EvaluationMode.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Frequency;
                hashCode = (hashCode * 397) ^ Until.GetHashCode();
                hashCode = (hashCode * 397) ^ Count;
                hashCode = (hashCode * 397) ^ (int)FirstDayOfWeek;
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(BySecond);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByMinute);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByHour);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByDay);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByMonthDay);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByYearDay);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByWeekNo);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByMonth);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(BySetPosition);
                return hashCode;
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (!(obj is RecurrencePattern pattern))
            {
                return;
            }

            Frequency = pattern.Frequency;
            Until = pattern.Until;
            Count = pattern.Count;
            Interval = pattern.Interval;
            BySecond = new List<int>(pattern.BySecond);
            ByMinute = new List<int>(pattern.ByMinute);
            ByHour = new List<int>(pattern.ByHour);
            ByDay = new List<WeekDay>(pattern.ByDay);
            ByMonthDay = new List<int>(pattern.ByMonthDay);
            ByYearDay = new List<int>(pattern.ByYearDay);
            ByWeekNo = new List<int>(pattern.ByWeekNo);
            ByMonth = new List<int>(pattern.ByMonth);
            BySetPosition = new List<int>(pattern.BySetPosition);
            FirstDayOfWeek = pattern.FirstDayOfWeek;
            RestrictionType = pattern.RestrictionType;
            EvaluationMode = pattern.EvaluationMode;
        }

        private static bool CollectionEquals<T>(IEnumerable<T> c1, IEnumerable<T> c2) => c1.SequenceEqual(c2);
    }
}