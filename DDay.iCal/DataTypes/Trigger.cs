using System;
using System.IO;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// A class that is used to specify exactly when an <see cref="Alarm"/> component will trigger.
    /// Usually this date/time is relative to the component to which the Alarm is associated.
    /// </summary>    
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Trigger : 
        EncodableDataType,
        ITrigger
    {
        #region Private Fields

        private IDateTime m_DateTime;
        private TimeSpan? m_Duration;
        private TriggerRelation m_Related = TriggerRelation.Start;

        #endregion

        #region Public Properties

        virtual public IDateTime DateTime
        {
            get { return m_DateTime; }
            set
            {
                m_DateTime = value;
                if (m_DateTime != null)
                {
                    // NOTE: this, along with the "Duration" setter, fixes the bug tested in
                    // TODO11(), as well as this thread: https://sourceforge.net/forum/forum.php?thread_id=1926742&forum_id=656447

                    // DateTime and Duration are mutually exclusive
                    Duration = null;

                    // Do not allow timeless date/time values
                    m_DateTime.HasTime = true;
                }
            }
        }

        virtual public TimeSpan? Duration
        {
            get { return m_Duration; }
            set
            {
                m_Duration = value;
                if (m_Duration != null)
                {
                    // NOTE: see above.

                    // DateTime and Duration are mutually exclusive
                    DateTime = null;
                }
            }
        }

        virtual public TriggerRelation Related
        {
            get { return m_Related; }
            set { m_Related = value; }
        }
        
        virtual public bool IsRelative
        {
            get { return m_Duration != null; }
        }

        #endregion

        #region Constructors

        public Trigger() { }
        public Trigger(TimeSpan ts)
        {
            Duration = ts;
        }
        public Trigger(string value)
            : this()
        {
            TriggerSerializer serializer = new TriggerSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is ITrigger)
            {
                ITrigger t = (ITrigger)obj;
                DateTime = t.DateTime;
                Duration = t.Duration;
                Related = t.Related;
            }
        }

        protected bool Equals(Trigger other)
        {
            return Equals(m_DateTime, other.m_DateTime) && m_Duration.Equals(other.m_Duration) && m_Related == other.m_Related;
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
            return Equals((Trigger) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (m_DateTime != null ? m_DateTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ m_Duration.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) m_Related;
                return hashCode;
            }
        }

        #endregion
    }
}
