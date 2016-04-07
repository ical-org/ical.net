using System;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// Represents an iCalendar period of time.
    /// </summary>    
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Period :
        EncodableDataType,
        IPeriod
    {
        #region Private Fields

        private IDateTime m_StartTime;
        private IDateTime m_EndTime;        
        private TimeSpan m_Duration;
        private bool m_MatchesDateOnly;

        #endregion        

        #region Constructors

        public Period()
        {
        }

        public Period(IDateTime occurs) : this(occurs, default(TimeSpan)) { }
        public Period(IDateTime start, IDateTime end) : this()
        {
            StartTime = start;
            if (end != null)
            {
                EndTime = end;
                Duration = end.Subtract(start);
            }
        }
        public Period(IDateTime start, TimeSpan duration)
            : this()
        {
            StartTime = start;
            if (duration != default(TimeSpan))
            {
                Duration = duration;
                EndTime = start.Add(duration);
            }
        }

        #endregion

        #region Overrides

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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Period)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (m_StartTime != null ? m_StartTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (m_EndTime != null ? m_EndTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ m_Duration.GetHashCode();
                hashCode = (hashCode * 397) ^ m_MatchesDateOnly.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var periodSerializer = new PeriodSerializer();
            return periodSerializer.SerializeToString(this);
        }

        #endregion

        #region Private Methods

        private void ExtrapolateTimes()
        {
            if (EndTime == null && StartTime != null && Duration != default(TimeSpan))
                EndTime = StartTime.Add(Duration);
            else if (Duration == default(TimeSpan) && StartTime != null && EndTime != null)
                Duration = EndTime.Subtract(StartTime);
            else if (StartTime == null && Duration != default(TimeSpan) && EndTime != null)
                StartTime = EndTime.Subtract(Duration);
        }

        #endregion

        #region IPeriod Members

        virtual public IDateTime StartTime
        {
            get { return m_StartTime; }
            set
            {                
                m_StartTime = value;
                ExtrapolateTimes();
            }
        }

        virtual public IDateTime EndTime
        {
            get { return m_EndTime; }
            set
            {
                m_EndTime = value;
                ExtrapolateTimes();
            }
        }

        virtual public TimeSpan Duration
        {
            get { return m_Duration; }
            set
            {
                if (!object.Equals(m_Duration, value))
                {
                    m_Duration = value;
                    ExtrapolateTimes();
                }
            }
        }

        /// <summary>
        /// When true, comparisons between this and other <see cref="Period"/>
        /// objects are matched against the date only, and
        /// not the date-time combination.
        /// </summary>
        virtual public bool MatchesDateOnly
        {
            get { return m_MatchesDateOnly; }
            set { m_MatchesDateOnly = value; }
        }

        virtual public bool Contains(IDateTime dt)
        {
            // Start time is inclusive
            if (dt != null &&
                StartTime != null &&
                StartTime.LessThanOrEqual(dt))
            {
                // End time is exclusive
                if (EndTime == null || EndTime.GreaterThan(dt))
                    return true;
            }
            return false;
        }

        virtual public bool CollidesWith(IPeriod period)
        {
            if (period != null &&
                (
                    (period.StartTime != null && Contains(period.StartTime)) ||
                    (period.EndTime != null && Contains(period.EndTime))
                ))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(IPeriod p)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            else if (Equals(p))
                return 0;
            else if (StartTime.LessThan(p.StartTime))
                return -1;
            else if (StartTime.GreaterThanOrEqual(p.StartTime))
                return 1;
            throw new Exception("An error occurred while comparing Period values.");
        }

        #endregion
    }
}
