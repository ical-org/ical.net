using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// An iCalendar list of recurring dates (or date exclusions)
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "PeriodList", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#else
    [Serializable]
#endif
    public class PeriodList : 
        EncodableDataType,
        IPeriodList
    {
        #region Private Fields

        private IList<IPeriod> m_Periods = new List<IPeriod>();
        private string m_TZID;
        private PeriodListEvaluator m_Evaluator;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public string TZID
        {
            get { return m_TZID; }
            set { m_TZID = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        protected IList<IPeriod> Periods
        {
            get { return m_Periods; }
            set { m_Periods = value; }
        }

        #endregion

        #region Constructors

        public PeriodList()
        {
            m_Evaluator = new PeriodListEvaluator(this);
        }
        public PeriodList(string value) : this()
        {
            PeriodListSerializer serializer = new PeriodListSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is IPeriodList)
            {
                IPeriodList r = (IPeriodList)obj;

                IEnumerator<IPeriod> p1Enum = GetEnumerator();
                IEnumerator<IPeriod> p2Enum = r.GetEnumerator();

                while (p1Enum.MoveNext())
                {
                    if (!p2Enum.MoveNext())
                        return false;

                    if (!object.Equals(p1Enum.Current, p2Enum.Current))
                        return false;
                }

                if (p2Enum.MoveNext())
                    return false;

                return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (IPeriod p in this)
                hashCode ^= p.GetHashCode();
            return hashCode;
        }
 
        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IPeriodList)
            {
                IPeriodList rdt = (IPeriodList)obj;
                foreach (IPeriod p in rdt)
                    Add(p.Copy<IPeriod>());
            }
        }

        public override string ToString()
        {
            PeriodListSerializer serializer = new PeriodListSerializer();
            return serializer.SerializeToString(this);
        }

        public override object GetService(Type serviceType)
        {
            if (typeof(IEvaluator).IsAssignableFrom(serviceType))
                return m_Evaluator;
            return null;
        }

        #endregion

        #region Public Methods

        public List<Period> Evaluate(iCalDateTime StartDate, iCalDateTime FromDate, iCalDateTime EndDate)
        {
            List<Period> periods = new List<Period>();

            if (StartDate > FromDate)
                FromDate = StartDate;

            if (EndDate < FromDate ||
                FromDate > EndDate)
                return periods;

            foreach (Period p in Periods)
                if (!periods.Contains(p))
                    periods.Add(p);

            return periods;
        }

        #endregion

        #region IPeriodList Members

        virtual public void Add(IDateTime dt)
        {
            Periods.Add(new Period(dt));
        }

        virtual public void Remove(IDateTime dt)
        {
            Periods.Remove(new Period(dt));
        }

        public IPeriod this[int index]
        {
            get
            {
                return m_Periods[index];
            }
            set
            {
                m_Periods[index] = value;
            }
        }

        #endregion

        #region ICollection<IPeriod> Members

        virtual public void Add(IPeriod item)
        {
            m_Periods.Add(item);
        }

        virtual public void Clear()
        {
            m_Periods.Clear();
        }

        public bool Contains(IPeriod item)
        {
            return m_Periods.Contains(item);
        }

        public void CopyTo(IPeriod[] array, int arrayIndex)
        {
            m_Periods.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_Periods.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IPeriod item)
        {
            return m_Periods.Remove(item);
        }

        #endregion

        #region IEnumerable<IPeriod> Members

        public IEnumerator<IPeriod> GetEnumerator()
        {
            return m_Periods.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Periods.GetEnumerator();
        }

        #endregion
    }
}
