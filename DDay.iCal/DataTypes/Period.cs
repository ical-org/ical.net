using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using DDay.iCal.Serialization.iCalendar;

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

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public IDateTime StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public IDateTime EndTime
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
        
        public override bool Equals(object obj)
        {
            if (obj is IPeriod)
            {
                IPeriod p = (IPeriod)obj;
                if (MatchesDateOnly || p.MatchesDateOnly)
                {
                    return
                        StartTime.Value.Date == p.StartTime.Value.Date &&
                        (
                            EndTime == null ||
                            p.EndTime == null ||
                            EndTime.Value.Date.Equals(p.EndTime.Value.Date)
                        );
                }
                else
                {
                    return
                        StartTime.Equals(p.StartTime) &&
                        (
                            EndTime == null ||
                            p.EndTime == null ||
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

        public override string ToString()
        {
            PeriodSerializer periodSerializer = new PeriodSerializer();
            return periodSerializer.SerializeToString(this);
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
