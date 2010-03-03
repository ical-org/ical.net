using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
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
#if DATACONTRACT
    [DataContract(Name = "AlarmOccurrence", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class AlarmOccurrence : 
        Occurrence,
        IAlarmOccurrence        
    {
        #region Private Fields

        private IAlarm m_Alarm;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public IAlarm Alarm
        {
            get { return m_Alarm; }
            set { m_Alarm = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public iCalDateTime DateTime
        {
            get
            {
                if (Period != null)
                    return Period.StartTime;
                return default(iCalDateTime);
            }
            set
            {
                if (Period != null)
                    Period.StartTime = value;
                else Period = new Period(value);
            }
        }

        #endregion

        #region Constructors

        public AlarmOccurrence(IAlarmOccurrence ao) : base(ao)
        {
            this.Alarm = ao.Alarm;
        }

        public AlarmOccurrence(IAlarm a, iCalDateTime dt, IRecurringComponent rc)
        {
            this.Alarm = a;
            this.Period = new Period(dt);
            this.Component = rc;
        }

        #endregion

        #region IComparable<IAlarmOccurrence> Members

        public int CompareTo(IAlarmOccurrence other)
        {
            return base.CompareTo(other);
        }

        #endregion
    }
}
