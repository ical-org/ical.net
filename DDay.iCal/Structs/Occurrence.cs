using System;

namespace DDay.iCal
{    
#if !SILVERLIGHT
    [Serializable]
#endif
    public struct Occurrence :
        IComparable<Occurrence>
    {
        #region Private Fields
        
        private IPeriod m_Period;
        private IRecurrable m_Source; 

        #endregion

        #region Public Properties

        public IPeriod Period
        {
            get { return m_Period; }
            set { m_Period = value; }
        }

        public IRecurrable Source
        {
            get { return m_Source; }
            set { m_Source = value; }
        } 

        #endregion

        #region Constructors

        public Occurrence(Occurrence ao)
        {
            m_Period = ao.Period;
            m_Source = ao.Source;
        }

        public Occurrence(IRecurrable recurrable, IPeriod period)
        {
            m_Source = recurrable;
            m_Period = period;
        }

        #endregion

        #region Overrides

        public bool Equals(Occurrence other)
        {
            return Equals(m_Period, other.m_Period) && Equals(m_Source, other.m_Source);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Occurrence && Equals((Occurrence) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((m_Period != null ? m_Period.GetHashCode() : 0) * 397) ^ (m_Source != null ? m_Source.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            string s = "Occurrence";
            if (Source != null)
                s = Source.GetType().Name + " ";

            if (Period != null)
                s += "(" + Period.StartTime.ToString() + ")";

            return s;
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
