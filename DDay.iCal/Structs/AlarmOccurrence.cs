using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents a specific occurrence of an <see cref="Alarm"/>.        
    /// </summary>
    /// <remarks>
    /// The <see cref="AlarmOccurrence"/> contains the <see cref="Period"/> when
    /// the alarm occurs, the <see cref="Alarm"/> that fired, and the 
    /// component on which the alarm fired.
    /// </remarks>
#if !SILVERLIGHT
    [Serializable]
#endif
    public struct AlarmOccurrence : 
        IComparable<AlarmOccurrence>
    {
        #region Private Fields

        private IPeriod m_Period;
        private IRecurringComponent m_Component; 
        private IAlarm m_Alarm;

        #endregion

        #region Public Properties

        public IPeriod Period
        {
            get { return m_Period; }
            set { m_Period = value; }
        }

        public IRecurringComponent Component
        {
            get { return m_Component; }
            set { m_Component = value; }
        } 

        public IAlarm Alarm
        {
            get { return m_Alarm; }
            set { m_Alarm = value; }
        }

        public IDateTime DateTime
        {
            get { return Period.StartTime; }
            set { Period = new Period(value); }
        }

        #endregion

        #region Constructors

        public AlarmOccurrence(AlarmOccurrence ao)
        {
            m_Period = ao.Period;
            m_Component = ao.Component;
            m_Alarm = ao.Alarm;
        }

        public AlarmOccurrence(IAlarm a, IDateTime dt, IRecurringComponent rc)
        {
            m_Alarm = a;
            m_Period = new Period(dt);
            m_Component = rc;
        }

        #endregion

        #region IComparable<AlarmOccurrence> Members

        public int CompareTo(AlarmOccurrence other)
        {
            return Period.CompareTo(other.Period);
        }

        #endregion
    }
}
