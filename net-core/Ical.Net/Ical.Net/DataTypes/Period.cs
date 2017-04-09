using System;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary> Represents an iCalendar period of time. </summary>    
    public class Period : EncodableDataType, IComparable<Period>, IEquatable<Period>
    {
        public Period() { }

        public Period(IDateTime occurs)
            : this(occurs, default(TimeSpan)) {}

        public Period(IDateTime start, IDateTime end)
        {
            if (end != null && end.LessThanOrEqual(start))
            {
                throw new ArgumentException($"Start time ( {start} ) must come before the end time ( {end} )");
            }

            StartTime = start;
            if (end == null)
            {
                return;
            }
            EndTime = end;
            Duration = end.Subtract(start);
        }

        public Period(IDateTime start, TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentException($"Duration ( ${duration} ) cannot be less than zero");
            }

            StartTime = start;
            if (duration == default(TimeSpan))
            {
                return;
            }

            Duration = duration;
            EndTime = start.Add(duration);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var p = obj as Period;
            if (p == null)
            {
                return;
            }
            StartTime = p.StartTime;
            EndTime = p.EndTime;
            Duration = p.Duration;
        }

        public bool Equals(Period other)
        {
            return Equals(StartTime, other.StartTime) && Equals(EndTime, other.EndTime) && Duration.Equals(other.Duration);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Period) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StartTime?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (EndTime?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Duration.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var periodSerializer = new PeriodSerializer();
            return periodSerializer.SerializeToString(this);
        }

        private void ExtrapolateTimes()
        {
            if (EndTime == null && StartTime != null && Duration != default(TimeSpan))
            {
                EndTime = StartTime.Add(Duration);
            }
            else if (Duration == default(TimeSpan) && StartTime != null && EndTime != null)
            {
                Duration = EndTime.Subtract(StartTime);
            }
            else if (StartTime == null && Duration != default(TimeSpan) && EndTime != null)
            {
                StartTime = EndTime.Subtract(Duration);
            }
        }

        private IDateTime _startTime;
        public virtual IDateTime StartTime
        {
            get { return _startTime; }
            set
            {
                if (Equals(_startTime, value))
                {
                    return;
                }
                _startTime = value;
                ExtrapolateTimes();
            }
        }

        private IDateTime _endTime;
        public virtual IDateTime EndTime
        {
            get { return _endTime; }
            set
            {
                if (Equals(_endTime, value))
                {
                    return;
                }
                _endTime = value;
                ExtrapolateTimes();
            }
        }

        private TimeSpan _duration;
        public virtual TimeSpan Duration
        {
            get
            {
                if (StartTime != null
                    && EndTime == null
                    && StartTime.Value.TimeOfDay == TimeSpan.Zero)
                {
                    return TimeSpan.FromDays(1);
                }
                return _duration;
            }
            set
            {
                if (Equals(_duration, value))
                {
                    return;
                }
                _duration = value;
                ExtrapolateTimes();
            }
        }

        public virtual bool Contains(IDateTime dt)
        {
            // Start time is inclusive
            if (dt == null || StartTime == null || !StartTime.LessThanOrEqual(dt))
            {
                return false;
            }

            // End time is exclusive
            return EndTime == null || EndTime.GreaterThan(dt);
        }

        public virtual bool CollidesWith(Period period)
        {
            return period != null
                && ((period.StartTime != null && Contains(period.StartTime)) || (period.EndTime != null && Contains(period.EndTime)));
        }

        public int CompareTo(Period other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }
            if (StartTime.GreaterThanOrEqual(other.StartTime))
            {
                return 1;
            }
            if (Equals(other))
            {
                return 0;
            }
            return -1;
        }
    }
}