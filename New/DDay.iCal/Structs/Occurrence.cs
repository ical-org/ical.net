using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    [DebuggerDisplay("{Component.Summary} ({Period.StartTime} - {Period.EndTime})")]
#if DATACONTRACT
    [DataContract(Name = "Occurrence", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public struct Occurrence :
        IComparable<Occurrence>
    {
        #region Private Fields
        
        private Period m_Period;
        private IRecurringComponent m_Component; 

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public Period Period
        {
            get { return m_Period; }
            set { m_Period = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public IRecurringComponent Component
        {
            get { return m_Component; }
            set { m_Component = value; }
        } 

        #endregion

        #region Constructors

        public Occurrence(Occurrence ao)
        {
            m_Period = ao.Period;
            m_Component = ao.Component;
        }

        public Occurrence(IRecurringComponent component, Period period)
        {
            m_Component = component;
            m_Period = period;
        }

        #endregion

        #region IComparable<Occurrence> Members

        public int CompareTo(Occurrence other)
        {
            return Period.CompareTo(other.Period);
        }

        #endregion
    }
}
