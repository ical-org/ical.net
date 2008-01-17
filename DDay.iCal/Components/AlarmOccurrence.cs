using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents a specific occurrence of an <see cref="Alarm"/>.        
    /// </summary>
    /// <remarks>
    /// The <see cref="AlarmOccurrence"/> contains the <see cref="Period"/> when
    /// the alarm occurs, the <see cref="Alarm"/> that fired, and the 
    /// component on which the alarm fired.
    /// </remarks>
    public class AlarmOccurrence : Occurrence
    {
        #region Private Fields

        private Alarm m_Alarm;

        #endregion

        #region Public Properties

        public Alarm Alarm
        {
            get { return m_Alarm; }
            set { m_Alarm = value; }
        }

        public Date_Time DateTime
        {
            get
            {
                if (Period != null)
                    return Period.StartTime;
                return default(Date_Time);
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

        public AlarmOccurrence(AlarmOccurrence ao)
        {
            this.Alarm = ao.Alarm;
            this.Period = ao.Period.Copy();
            this.Component = ao.Component;
        }

        public AlarmOccurrence(Alarm a, Date_Time dt, RecurringComponent rc)
        {
            this.Alarm = a;
            this.Period = new Period(dt);
            this.Component = rc;
        }

        #endregion

        #region Public Methods

        public AlarmOccurrence Copy()
        {
            AlarmOccurrence ao = new AlarmOccurrence(this);
            return ao;
        }

        #endregion
    }
}
