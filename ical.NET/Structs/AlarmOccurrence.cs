using System;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Structs
{
    /// <summary>
    /// A class that represents a specific occurrence of an <see cref="Alarm"/>.        
    /// </summary>
    /// <remarks>
    /// The <see cref="AlarmOccurrence"/> contains the <see cref="Period"/> when
    /// the alarm occurs, the <see cref="Alarm"/> that fired, and the 
    /// component on which the alarm fired.
    /// </remarks>
    [Serializable]
    public struct AlarmOccurrence : IComparable<AlarmOccurrence>
    {
        #region Private Fields

        private IPeriod _mPeriod;
        private IRecurringComponent _mComponent;
        private IAlarm _mAlarm;

        #endregion

        #region Public Properties

        public IPeriod Period
        {
            get { return _mPeriod; }
            set { _mPeriod = value; }
        }

        public IRecurringComponent Component
        {
            get { return _mComponent; }
            set { _mComponent = value; }
        }

        public IAlarm Alarm
        {
            get { return _mAlarm; }
            set { _mAlarm = value; }
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
            _mPeriod = ao.Period;
            _mComponent = ao.Component;
            _mAlarm = ao.Alarm;
        }

        public AlarmOccurrence(IAlarm a, IDateTime dt, IRecurringComponent rc)
        {
            _mAlarm = a;
            _mPeriod = new Period(dt);
            _mComponent = rc;
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