using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.IO;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// An iCalendar representation of the <c>RRULE</c> property.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public partial class RecurrencePattern :
        EncodableDataType,
        IRecurrencePattern
    {
        #region Private Fields

#if !SILVERLIGHT
        [NonSerialized]
#endif
        private FrequencyType _Frequency;
        private DateTime _Until = DateTime.MinValue;
        private int _Count = int.MinValue;
        private int _Interval = int.MinValue;
        private IList<int> _BySecond = new List<int>();
        private IList<int> _ByMinute = new List<int>();
        private IList<int> _ByHour = new List<int>();
        private IList<IWeekDay> _ByDay = new List<IWeekDay>();
        private IList<int> _ByMonthDay = new List<int>();
        private IList<int> _ByYearDay = new List<int>();
        private IList<int> _ByWeekNo = new List<int>();
        private IList<int> _ByMonth = new List<int>();
        private IList<int> _BySetPosition = new List<int>();
        private DayOfWeek _FirstDayOfWeek = DayOfWeek.Monday;
        private RecurrenceRestrictionType? _RestrictionType = null;
        private RecurrenceEvaluationModeType? _EvaluationMode = null;

        #endregion

        #region Public Properties

        public FrequencyType Frequency
        {
            get { return _Frequency; }
            set { _Frequency = value; }
        }

        public DateTime Until
        {
            get { return _Until; }
            set { _Until = value; }
        }

        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }

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

        public IList<int> BySecond
        {
            get { return _BySecond; }
            set { _BySecond = value; }
        }

        public IList<int> ByMinute
        {
            get { return _ByMinute; }
            set { _ByMinute = value; }
        }

        public IList<int> ByHour
        {
            get { return _ByHour; }
            set { _ByHour = value; }
        }

        public IList<IWeekDay> ByDay
        {
            get { return _ByDay; }
            set { _ByDay = value; }
        }

        public IList<int> ByMonthDay
        {
            get { return _ByMonthDay; }
            set { _ByMonthDay = value; }
        }

        public IList<int> ByYearDay
        {
            get { return _ByYearDay; }
            set { _ByYearDay = value; }
        }

        public IList<int> ByWeekNo
        {
            get { return _ByWeekNo; }
            set { _ByWeekNo = value; }
        }

        public IList<int> ByMonth
        {
            get { return _ByMonth; }
            set { _ByMonth = value; }
        }

        public IList<int> BySetPosition
        {
            get { return _BySetPosition; }
            set { _BySetPosition = value; }
        }

        public DayOfWeek FirstDayOfWeek
        {
            get { return _FirstDayOfWeek; }
            set { _FirstDayOfWeek = value; }
        }

        public RecurrenceRestrictionType RestrictionType
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_RestrictionType != null &&
                    _RestrictionType.HasValue)
                    return _RestrictionType.Value;
                else if (Calendar != null)
                    return Calendar.RecurrenceRestriction;
                else
                    return RecurrenceRestrictionType.Default;
            }
            set { _RestrictionType = value; }
        }

        public RecurrenceEvaluationModeType EvaluationMode
        {
            get
            {
                // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
                if (_EvaluationMode != null &&
                    _EvaluationMode.HasValue)
                    return _EvaluationMode.Value;
                else if (Calendar != null)
                    return Calendar.RecurrenceEvaluationMode;
                else
                    return RecurrenceEvaluationModeType.Default;
            }
            set { _EvaluationMode = value; }
        }

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
                IDateTime fromDate = lastOccurrence;
                IDateTime toDate = null;

                IRecurrencePattern r = new RecurrencePattern();
                r.CopyFrom(this);

                switch (r.Frequency)
                {
                    case FrequencyType.Yearly:
                        toDate = fromDate.FirstDayOfYear.AddYears(Interval + 1);
                        break;
                    case FrequencyType.Monthly:
                        // Determine how far into the future we need to scan
                        // to get the next occurrence.
                        int yearsByInterval = (int)Math.Ceiling((double)Interval / 12.0);
                        if (ByMonthDay.Count > 0)
                            toDate = fromDate.FirstDayOfYear.AddYears(yearsByInterval + 1);
                        else if (ByMonth.Count > 0)
                            toDate = fromDate.AddYears(yearsByInterval);
                        else
                            toDate = fromDate.FirstDayOfMonth.AddMonths(Interval + 1);
                        break;
                    case FrequencyType.Weekly:
                        toDate = fromDate.AddDays((Interval + 1) * 7);
                        break;
                    case FrequencyType.Daily:
                        if (ByDay.Count > 0)
                            toDate = fromDate.AddDays(7);
                        else
                            toDate = fromDate.AddDays(Interval + 1);
                        break;
                    case FrequencyType.Hourly:
                        if (ByHour.Count > 0)
                        {
                            int daysByInterval = (int)Math.Ceiling((double)Interval / 24.0);
                            toDate = fromDate.AddDays(daysByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddHours(Interval + 1);
                        break;
                    case FrequencyType.Minutely:
                        if (ByMinute.Count > 0)
                        {
                            int hoursByInterval = (int)Math.Ceiling((double)Interval / 60.0);
                            toDate = fromDate.AddHours(hoursByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddMinutes(Interval + 1);
                        break;
                    case FrequencyType.Secondly:
                        if (BySecond.Count > 0)
                        {
                            int minutesByInterval = (int)Math.Ceiling((double)Interval / 60.0);
                            toDate = fromDate.AddMinutes(minutesByInterval + 1);
                        }
                        else
                            toDate = fromDate.AddMinutes(Interval + 1);
                        break;
                }

                if (toDate != null)
                {
                    IEvaluator evaluator = new RecurrencePatternEvaluator(this);

                    IPeriod lastOccurrencePeriod = new Period(lastOccurrence);

                    // Get the first occurence within the interval we just evaluated, if available.
                    IList<IPeriod> occurrences = evaluator.Evaluate(lastOccurrence, fromDate.Value, toDate.Value, true);
                    if (occurrences != null &&
                        occurrences.Count > 0)
                    {
                        // NOTE: the lastOccurrence may or may not be contained in the
                        // result list of occurrences.  If it is, grab the next occurence
                        // if it is available.
                        if (occurrences[0].Equals(lastOccurrencePeriod))
                        {
                            if (occurrences.Count > 1)
                                return occurrences[1];
                        }
                        else return occurrences[0];
                    }
                }
            }

            return null;
        }

        #endregion

        #region Constructors

        public RecurrencePattern()
        {
            Initialize();
        }

        public RecurrencePattern(FrequencyType frequency) : this(frequency, 1)
        {
        }

        public RecurrencePattern(FrequencyType frequency, int interval) :
            this()
        {
            Frequency = frequency;
            Interval = interval;
        }

        public RecurrencePattern(string value) : 
            this()
        {
            if (value != null)
            {
                DDay.iCal.Serialization.iCalendar.RecurrencePatternSerializer serializer = new DDay.iCal.Serialization.iCalendar.RecurrencePatternSerializer();
                CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
            }
        }

        void Initialize()
        {
            SetService(new RecurrencePatternEvaluator(this));
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

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
                if (r.Until != DateTime.MinValue)
                {
                    if (!r.Until.Equals(Until))
                        return false;
                }
                else if (Until != DateTime.MinValue)
                    return false;
                if (r.FirstDayOfWeek != FirstDayOfWeek) return false;
                return true;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            RecurrencePatternSerializer serializer = new RecurrencePatternSerializer();
            return serializer.SerializeToString(this);
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
            hashCode ^= FirstDayOfWeek.GetHashCode();
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
    }
}
