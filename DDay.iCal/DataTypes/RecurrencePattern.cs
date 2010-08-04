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
        public DateTime Until
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
        public IList<IWeekDay> ByDay
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
        public DayOfWeek FirstDayOfWeek
        {
            get { return _FirstDayOfWeek; }
            set { _FirstDayOfWeek = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 15)]
#endif
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

#if DATACONTRACT
        [DataMember(Order = 16)]
#endif
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
