using System;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// Represents an iCalendar period of time.
    /// </summary>    
    public class Period : EncodableDataType, IPeriod
    {
        private IDateTime _mStartTime;
        private IDateTime _mEndTime;
        private TimeSpan _mDuration;
        private bool _mMatchesDateOnly;

        public Period() {}

        public Period(IDateTime occurs) : this(occurs, default(TimeSpan)) {}

        public Period(IDateTime start, IDateTime end) : this()
        {
            StartTime = start;
            if (end != null)
            {
                EndTime = end;
                Duration = end.Subtract(start);
            }
        }

        public Period(IDateTime start, TimeSpan duration) : this()
        {
            StartTime = start;
            if (duration != default(TimeSpan))
            {
                Duration = duration;
                EndTime = start.Add(duration);
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var p = obj as IPeriod;
            if (p != null)
            {
                StartTime = p.StartTime;
                EndTime = p.EndTime;
                Duration = p.Duration;
                MatchesDateOnly = p.MatchesDateOnly;
            }
        }

        public bool Equals(Period period)
        {
            if (MatchesDateOnly || period.MatchesDateOnly)
            {
                return StartTime.Value.Date == period.StartTime.Value.Date &&
                       (EndTime == null || period.EndTime == null || EndTime.Value.Date.Equals(period.EndTime.Value.Date));
            }
            return StartTime.Equals(period.StartTime) && (EndTime == null || period.EndTime == null || EndTime.Equals(period.EndTime));
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
            return Equals((Period) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _mStartTime?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (_mEndTime?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _mDuration.GetHashCode();
                hashCode = (hashCode * 397) ^ _mMatchesDateOnly.GetHashCode();
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

        public virtual IDateTime StartTime
        {
            get { return _mStartTime; }
            set
            {
                _mStartTime = value;
                ExtrapolateTimes();
            }
        }

        public virtual IDateTime EndTime
        {
            get { return _mEndTime; }
            set
            {
                _mEndTime = value;
                ExtrapolateTimes();
            }
        }

        public virtual TimeSpan Duration
        {
            get { return _mDuration; }
            set
            {
                if (!Equals(_mDuration, value))
                {
                    _mDuration = value;
                    ExtrapolateTimes();
                }
            }
        }

        /// <summary>
        /// When true, comparisons between this and other <see cref="Period"/>
        /// objects are matched against the date only, and
        /// not the date-time combination.
        /// </summary>
        public virtual bool MatchesDateOnly
        {
            get { return _mMatchesDateOnly; }
            set { _mMatchesDateOnly = value; }
        }

        public virtual bool Contains(IDateTime dt)
        {
            // Start time is inclusive
            if (dt != null && StartTime != null && StartTime.LessThanOrEqual(dt))
            {
                // End time is exclusive
                if (EndTime == null || EndTime.GreaterThan(dt))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool CollidesWith(IPeriod period)
        {
            if (period != null && ((period.StartTime != null && Contains(period.StartTime)) || (period.EndTime != null && Contains(period.EndTime))))
            {
                return true;
            }
            return false;
        }

        public int CompareTo(IPeriod p)
        {
            if (p == null)
            {
                throw new ArgumentNullException(nameof(p));
            }
            if (Equals(p))
            {
                return 0;
            }
            if (StartTime.LessThan(p.StartTime))
            {
                return -1;
            }
            if (StartTime.GreaterThanOrEqual(p.StartTime))
            {
                return 1;
            }
            throw new Exception("An error occurred while comparing Period values.");
        }
    }
}