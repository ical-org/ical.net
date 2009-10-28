using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.DataTypes;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    [DebuggerDisplay("{Component.Summary} ({Period.StartTime} - {Period.EndTime})")]
#if DATACONTRACT
    [DataContract(Name = "Occurrence", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Occurrence :
        IComparable<Occurrence>
    {
        #region Private Fields
        
        private Period _Period;
        private RecurringComponent _Component; 

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public Period Period
        {
            get { return _Period; }
            set { _Period = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public RecurringComponent Component
        {
            get { return _Component; }
            set { _Component = value; }
        } 

        #endregion

        #region Constructors

        public Occurrence()
        {
        }

        public Occurrence(Occurrence ao)
        {
            this.Period = ao.Period.Copy();
            this.Component = ao.Component;
        }

        public Occurrence(RecurringComponent component, Period period)
        {
            Component = component;
            Period = period;
        }

        #endregion

        #region Public Methods

        virtual public Occurrence Copy()
        {
            Occurrence o = new Occurrence(this);
            return o;
        }

        #endregion

        #region IComparable<Occurrence> Members

        public int CompareTo(Occurrence other)
        {
            if (Period != null &&
                other.Period != null)
                return Period.CompareTo(other.Period);
              
            return 0;
        }

        #endregion
    }
}
