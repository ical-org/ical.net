using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.DataTypes;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    [DebuggerDisplay("{Component.Summary} ({Period.StartTime} - {Period.EndTime})")]
#if SILVERLIGHT
    [DataContract(Name = "Occurrence", Namespace="http://www.ddaysoftware.com/dday.ical/components/2009/07/")]
#else
    [Serializable]
#endif
    public class Occurrence :
        IComparable<Occurrence>
    {
        #region Private Fields
        
        private Period _Period;
        private RecurringComponent _Component; 

        #endregion

        #region Public Properties

        virtual public Period Period
        {
            get { return _Period; }
            set { _Period = value; }
        }

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
