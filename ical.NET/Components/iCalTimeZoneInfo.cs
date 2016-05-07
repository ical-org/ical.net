using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Structs;
using Ical.Net.Utility;

namespace Ical.Net
{    
    /// <summary>
    /// A class that contains time zone information, and is usually accessed
    /// from an iCalendar object using the <see cref="Calendar.GetTimeZone"/> method.        
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CalTimeZoneInfo : CalendarComponent, ITimeZoneInfo
    {
        private TimeZoneInfoEvaluator _mEvaluator;

        public CalTimeZoneInfo() : base()
        {
            // FIXME: how do we ensure SEQUENCE doesn't get serialized?
            // base.Sequence = null;
            // iCalTimeZoneInfo does not allow sequence numbers
            // Perhaps we should have a custom serializer that fixes this?

            Initialize();
        }

        public CalTimeZoneInfo(string name) : this()
        {
            Name = name;
        }

        void Initialize()
        {
            _mEvaluator = new TimeZoneInfoEvaluator(this);
            SetService(_mEvaluator);
        }

        private string _tzId;
        virtual public string TzId
        {
            get
            {
                if (_tzId == null)
                {
                    var tz = Parent as ITimeZone;
                    _tzId = tz?.TzId;
                }
                return _tzId;
            }
        }

        /// <summary>
        /// Returns the name of the current Time Zone.
        /// <example>
        ///     The following are examples:
        ///     <list type="bullet">
        ///         <item>EST</item>
        ///         <item>EDT</item>
        ///         <item>MST</item>
        ///         <item>MDT</item>
        ///     </list>
        /// </example>
        /// </summary>
        virtual public string TimeZoneName
        {
            get
            {
                return !string.IsNullOrWhiteSpace(TzId)
                    ? TzId
                    : TimeZoneNames.FirstOrDefault();
            }
            set
            {
                TimeZoneNames.Clear();
                TimeZoneNames.Add(value);
            }
        }

        private IUtcOffset _offsetFrom;
        virtual public IUtcOffset OffsetFrom
        {
            get { return _offsetFrom ?? (_offsetFrom = Properties.Get<IUtcOffset>("TZOFFSETFROM")); }
            set { _offsetFrom = value; }
        }

        private IUtcOffset _offsetTo;
        virtual public IUtcOffset OffsetTo
        {
            get { return _offsetTo ?? (_offsetTo = Properties.Get<IUtcOffset>("TZOFFSETTO")); }
            set { _offsetTo = value; }
        }

        private IList<string> _tzNames = new List<string>();
        virtual public IList<string> TimeZoneNames
        {
            get { return _tzNames ?? (_tzNames = Properties.GetMany<string>("TZNAME")); }
            set { _tzNames = value; }
        }

        virtual public IDateTime DtStart
        {
            get { return Start; }
            set { Start = value; }
        }

        virtual public IDateTime Start
        {
            get { return Properties.Get<IDateTime>("DTSTART"); }
            set { Properties.Set("DTSTART", value); }
        }

        virtual public IList<IPeriodList> ExceptionDates
        {
            get { return Properties.GetMany<IPeriodList>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        virtual public IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        virtual public IList<IPeriodList> RecurrenceDates
        {
            get { return Properties.GetMany<IPeriodList>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        virtual public IList<IRecurrencePattern> RecurrenceRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("RRULE"); }
            set { Properties.Set("RRULE", value); }
        }

        virtual public IDateTime RecurrenceId
        {
            get { return Properties.Get<IDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        virtual public void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        virtual public HashSet<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt, true);
        }

        virtual public HashSet<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new CalDateTime(dt), true);
        }

        virtual public HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime, true);
        }

        virtual public HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, new CalDateTime(startTime), new CalDateTime(endTime), true);
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected bool Equals(CalTimeZoneInfo other)
        {
            return base.Equals(other) && Equals(_mEvaluator, other._mEvaluator);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((CalTimeZoneInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_mEvaluator != null ? _mEvaluator.GetHashCode() : 0);
                return hashCode;
            }
        }
    }    
}
