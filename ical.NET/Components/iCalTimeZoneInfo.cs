using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
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
        public CalTimeZoneInfo()
        {
            // FIXME: how do we ensure SEQUENCE doesn't get serialized?
            // base.Sequence = null;
            // iCalTimeZoneInfo does not allow sequence numbers
            // Perhaps we should have a custom serializer that fixes this?
        }

        public CalTimeZoneInfo(string name) : this()
        {
            Name = name;
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
        }

        protected bool Equals(CalTimeZoneInfo other)
        {
            return base.Equals(other);
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
            return Equals((CalTimeZoneInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397);
                return hashCode;
            }
        }
    }
}