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
    public class AlarmOccurrence : 
        Occurrence,
        IComparable<AlarmOccurrence>
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

        public iCalDateTime DateTime
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

        public AlarmOccurrence(AlarmOccurrence ao) : base(ao)
        {
            this.Alarm = ao.Alarm;
        }

        public AlarmOccurrence(Alarm a, iCalDateTime dt, RecurringComponent rc)
        {
            this.Alarm = a;
            this.Period = new Period(dt);
            this.Component = rc;
        }

        #endregion

        #region Public Methods

        public override Occurrence Copy()
        {
            AlarmOccurrence ao = new AlarmOccurrence(this);
            return ao;
        }

        #endregion

        #region IComparable<AlarmOccurrence> Members

        public int CompareTo(AlarmOccurrence other)
        {
            return base.CompareTo(other);
        }

        #endregion
    }
}
