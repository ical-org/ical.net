using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// Represents an iCalendar period of time.
    /// </summary>
    [DebuggerDisplay("Period ( {StartTime} - {EndTime} )")]
    public class Period : iCalDataType, IComparable
    {
        #region Private Fields

        private Date_Time m_StartTime = new Date_Time();        
        private Date_Time m_EndTime;        
        private Duration m_Duration;
        private bool m_MatchesDateOnly = false;

        #endregion

        #region Public Properties

        public Date_Time StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        public Date_Time EndTime
        {
            get { return m_EndTime; }
            set { m_EndTime = value; }
        }

        public Duration Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }
        
        /// <summary>
        /// When true, comparisons between this and other <see cref="Period"/>
        /// objects are matched against the date only, and
        /// not the date-time combination.
        /// </summary>
        public bool MatchesDateOnly
        {
            get { return m_MatchesDateOnly; }
            set { m_MatchesDateOnly = value; }
        }

        #endregion

        #region Constructors

        public Period() { }
        public Period(Date_Time occurs) : this(occurs, null) { }
        public Period(Date_Time start, Date_Time end)
            : this()
        {
            StartTime = start.Copy();
            if (end != null)
            {
                EndTime = end.Copy();
                Duration = new Duration(end.Value - start.Value);
            }            
        }
        public Period(Date_Time start, TimeSpan duration)
            : this()
        {
            StartTime = start.Copy();
            if (duration != TimeSpan.MinValue)
            {
                Duration = new Duration(duration);
                EndTime = start + duration;
            }            
        }
        public Period(string value)
            : this()
        {
            CopyFrom((Period)Parse(value));
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
                            EndTime == null ||
                            p.EndTime == null ||
                            EndTime.Value.Date == p.EndTime.Value.Date
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

        public override void CopyFrom(object obj)
        {
            if (obj is Period)
            {
                Period p = (Period)obj;
                StartTime = p.StartTime;
                EndTime = p.EndTime;
                Duration = p.Duration;
            }
            base.CopyFrom(obj);
        }
               
        public override bool TryParse(string value, ref object obj)
        {
            Period p = (Period)obj;

            string[] values = value.Split('/');
            if (values.Length != 2)
                return false;

            p.StartTime = new Date_Time();
            p.EndTime = new Date_Time();
            p.Duration = new Duration();

            object st = p.StartTime;
            object et = p.EndTime;
            object d = p.Duration;

            bool retVal = p.StartTime.TryParse(values[0], ref st) &&
                (
                    p.EndTime.TryParse(values[1], ref et) ||
                    p.Duration.TryParse(values[1], ref d)
                );

            // Fill in missing values
            if (!p.EndTime.HasDate)            
                p.EndTime = p.StartTime + p.Duration;            
            else if (p.Duration.Value.Ticks == 0)
                p.Duration = new Duration(p.EndTime.Value - p.StartTime.Value);

            return retVal;
        }

        public new Period Copy()
        {
            return (Period)base.Copy();
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
