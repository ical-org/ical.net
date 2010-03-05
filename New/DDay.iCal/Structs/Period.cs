using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// Represents an iCalendar period of time.
    /// </summary>
    [DebuggerDisplay("Period ( {StartTime} - {EndTime} )")]
#if DATACONTRACT
    [DataContract(Name = "Period", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public struct Period :
        IComparable
    {
        #region Private Fields

        private iCalDateTime m_StartTime;        
        private iCalDateTime m_EndTime;        
        private TimeSpan m_Duration;
        private bool m_MatchesDateOnly;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public iCalDateTime StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public iCalDateTime EndTime
        {
            get { return m_EndTime; }
            set { m_EndTime = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        public TimeSpan Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }
        
        /// <summary>
        /// When true, comparisons between this and other <see cref="Period"/>
        /// objects are matched against the date only, and
        /// not the date-time combination.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        public bool MatchesDateOnly
        {
            get { return m_MatchesDateOnly; }
            set { m_MatchesDateOnly = value; }
        }

        #endregion

        #region Constructors

        public Period(iCalDateTime occurs) : this(occurs, default(TimeSpan)) { }
        public Period(iCalDateTime start, iCalDateTime end)
            : this()
        {
            StartTime = start;
            if (end != null)
            {
                EndTime = end;
                Duration = end - start;
            }
        }
        public Period(iCalDateTime start, TimeSpan duration)
            : this()
        {
            StartTime = start;
            if (duration != default(TimeSpan))
            {
                Duration = duration;
                EndTime = start + duration;
            }
        }

        #endregion

        #region Overrides
        
        public override bool Equals(object obj)
        {
            if (obj is Period)
            {
                Period p = (Period)obj;
                if (MatchesDateOnly || p.MatchesDateOnly)
                {
                    return
                        StartTime.Value.Date == p.StartTime.Value.Date &&
                        (
                            !EndTime.IsAssigned ||
                            !p.EndTime.IsAssigned == null ||
                            EndTime.Value.Date.Equals(p.EndTime.Value.Date)
                        );
                }
                else
                {
                    return
                        StartTime.Equals(p.StartTime) &&
                        (
                            !EndTime.IsAssigned ||
                            !p.EndTime.IsAssigned ||
                            EndTime.Equals(p.EndTime)
                        );
                }
            }            
            return false;
        }

        public override int GetHashCode()
        {
            return StartTime.GetHashCode() ^ EndTime.GetHashCode();
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is Period)
            {
                if (Equals(obj))
                    return 0;
                else if (StartTime < ((Period)obj).StartTime)
                    return -1;
                else if (StartTime >= ((Period)obj).StartTime)
                    return 1;
            }
            throw new ArgumentException("obj must be a Period type");
        }

        #endregion
    }
}
