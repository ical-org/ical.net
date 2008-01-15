using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Components
{
    public class Occurrence :
        IComparable<Occurrence>
    {
        #region Private Fields
        
        private Period _Period;
        private RecurringComponent _Component; 

        #endregion

        #region Public Properties

        public Period Period
        {
            get { return _Period; }
            set { _Period = value; }
        }

        public RecurringComponent Component
        {
            get { return _Component; }
            set { _Component = value; }
        } 

        #endregion

        #region Constructors

        public Occurrence()
        {
        }

        public Occurrence(RecurringComponent component, Period period)
        {
            Component = component;
            Period = period;
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
