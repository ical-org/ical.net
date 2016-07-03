using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents a specific occurrence of an <see cref="Alarm"/>.        
    /// </summary>
    /// <remarks>
    /// The <see cref="AlarmOccurrence"/> contains the <see cref="Period"/> when
    /// the alarm occurs, the <see cref="Alarm"/> that fired, and the 
    /// component on which the alarm fired.
    /// </remarks>
    public class AlarmOccurrence : IComparable<AlarmOccurrence>
    {
        private IPeriod _mPeriod;
        private IRecurringComponent _mComponent;
        private IAlarm _mAlarm;

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

        public int CompareTo(AlarmOccurrence other)
        {
            return Period.CompareTo(other.Period);
        }
    }
}