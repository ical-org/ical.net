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

    [Serializable]

    public class CalTimeZoneInfo : CalendarComponent, ITimeZoneInfo
    {
        private TimeZoneInfoEvaluator _mEvaluator;

        public CalTimeZoneInfo()
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
        public virtual string TzId
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
        public virtual string TimeZoneName
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
        public virtual IUtcOffset OffsetFrom
        {
            get { return _offsetFrom ?? (_offsetFrom = Properties.Get<IUtcOffset>("TZOFFSETFROM")); }
            set { _offsetFrom = value; }
        }

        private IUtcOffset _offsetTo;
        public virtual IUtcOffset OffsetTo
        {
            get { return _offsetTo ?? (_offsetTo = Properties.Get<IUtcOffset>("TZOFFSETTO")); }
            set { _offsetTo = value; }
        }

        private IList<string> _tzNames = new List<string>();
        public virtual IList<string> TimeZoneNames
        {
            get { return _tzNames ?? (_tzNames = Properties.GetMany<string>("TZNAME")); }
            set { _tzNames = value; }
        }

        public virtual IDateTime DtStart
        {
            get { return Start; }
            set { Start = value; }
        }

        public virtual IDateTime Start
        {
            get { return Properties.Get<IDateTime>("DTSTART"); }
            set { Properties.Set("DTSTART", value); }
        }

        public virtual IList<IPeriodList> ExceptionDates
        {
            get { return Properties.GetMany<IPeriodList>("EXDATE"); }
            set { Properties.Set("EXDATE", value); }
        }

        public virtual IList<IRecurrencePattern> ExceptionRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("EXRULE"); }
            set { Properties.Set("EXRULE", value); }
        }

        public virtual IList<IPeriodList> RecurrenceDates
        {
            get { return Properties.GetMany<IPeriodList>("RDATE"); }
            set { Properties.Set("RDATE", value); }
        }

        public virtual IList<IRecurrencePattern> RecurrenceRules
        {
            get { return Properties.GetMany<IRecurrencePattern>("RRULE"); }
            set { Properties.Set("RRULE", value); }
        }

        public virtual IDateTime RecurrenceId
        {
            get { return Properties.Get<IDateTime>("RECURRENCE-ID"); }
            set { Properties.Set("RECURRENCE-ID", value); }
        }

        public virtual void ClearEvaluation()
        {
            RecurrenceUtil.ClearEvaluation(this);
        }

        public virtual HashSet<Occurrence> GetOccurrences(IDateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, dt, true);
        }

        public virtual HashSet<Occurrence> GetOccurrences(DateTime dt)
        {
            return RecurrenceUtil.GetOccurrences(this, new CalDateTime(dt), true);
        }

        public virtual HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            return RecurrenceUtil.GetOccurrences(this, startTime, endTime, true);
        }

        public virtual HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
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
