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
    public class Occurrence :
        IOccurrence
    {
        #region Private Fields
        
        private IPeriod _Period;
        private IRecurringComponent _Component; 

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public IPeriod Period
        {
            get { return _Period; }
            set { _Period = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public IRecurringComponent Component
        {
            get { return _Component; }
            set { _Component = value; }
        } 

        #endregion

        #region Constructors

        public Occurrence()
        {
        }

        public Occurrence(IOccurrence ao)
        {
            this.Period = ao.Period.Copy<Period>();
            this.Component = ao.Component;
        }

        public Occurrence(IRecurringComponent component, IPeriod period)
        {
            Component = component;
            Period = period;
        }

        #endregion

        #region IComparable<Occurrence> Members

        public int CompareTo(IOccurrence other)
        {
            if (Period != null &&
                other.Period != null)
                return Period.CompareTo(other.Period);
              
            return 0;
        }

        #endregion
    }
}
