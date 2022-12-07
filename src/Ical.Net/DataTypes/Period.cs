using System;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary> <see cref="StartTime"/> with optional <see cref="Duration"/> and <see cref="EndTime"/>. </summary>    
    public class Period : EncodableDataType, IComparable<Period>
    {
        public Period() { }

        public Period(IDateTime occurs)
            : this(occurs, default(TimeSpan)) {}

        public Period(IDateTime start, IDateTime? end)
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
            if (duration == default)
            {
                return;
            }

            Duration = duration;
            EndTime = start.Add(duration);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            if (!(obj is Period p))
            {
                return;
            }
            StartTime = p.StartTime;
            EndTime = p.EndTime;
            Duration = p.Duration;
        }

        protected bool Equals(Period other) => Equals(StartTime, other.StartTime) && Equals(EndTime, other.EndTime) && Duration.Equals(other.Duration);

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Period) obj);
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

        void ExtrapolateTimes()
        {
            if (EndTime == null && StartTime != null && Duration != null)
            {
                EndTime = StartTime.Add(Duration.Value);
            }
            else if (Duration == null && StartTime != null && EndTime != null)
            {
                Duration = EndTime.Subtract(StartTime);
            }
            else if (StartTime == null && Duration != null && EndTime != null)
            {
                StartTime = EndTime.Subtract(Duration.Value);
            }
        }

        IDateTime _startTime;
        public IDateTime StartTime
        {
            get => _startTime.HasTime
                ? _startTime
                : new CalDateTime(new DateTime(_startTime.Value.Year, _startTime.Value.Month, _startTime.Value.Day, 0, 0, 0), _startTime.TzId);
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

        IDateTime? _endTime;
        public IDateTime? EndTime
        {
            get => _endTime;
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

        TimeSpan? _duration;
        public TimeSpan? Duration
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

        public bool Contains(IDateTime dt)
        {
            // Start time is inclusive
            if (dt == null || StartTime == null || !StartTime.LessThanOrEqual(dt))
            {
                return false;
            }

            // End time is exclusive
            return EndTime == null || EndTime.GreaterThan(dt);
        }

        public bool CollidesWith(Period period) => period != null
            && ((period.StartTime != null && Contains(period.StartTime)) || (period.EndTime != null && Contains(period.EndTime)));

        public int CompareTo(Period other)
        {
            if (StartTime.Equals(other.StartTime))
            {
                return 0;
            }
            if (StartTime.LessThan(other.StartTime))
            {
                return -1;
            }
            if (StartTime.GreaterThan(other.StartTime))
            {
                return 1;
            }
            throw new Exception("An error occurred while comparing two Periods.");
        }
    }
}