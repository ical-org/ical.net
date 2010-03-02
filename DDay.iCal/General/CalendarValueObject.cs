using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal
{
#if DATACONTRACT
    [DataContract(Name = "CalendarValueObject", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    public class CalendarValueObject :
        CalendarObject,
        ICalendarValueObject
    {
        #region Private Fields

        private IList<object> m_Values;

        #endregion

        #region Constructors

        public CalendarValueObject() { Initialize(); }
        public CalendarValueObject(string name) : base(name) { Initialize(); }
        public CalendarValueObject(int line, int col) : base(line, col) { Initialize(); }
        public CalendarValueObject(string name, object value)
            : base(name)
        {
            Initialize();
            Value = value;
        }
        public CalendarValueObject(string name, IList<object> values)
            : base(name)
        {
            Values = values;
        }

        void Initialize()
        {
            m_Values = new List<object>();
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable c)
        {
            base.CopyFrom(c);
         
            ICalendarParameter p = c as ICalendarParameter;
            if (p != null)
            {
                Values = p.Values;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ICalendarValueObject)
            {
                ICalendarValueObject d = (ICalendarValueObject)obj;
                return 
                    d.Name.Equals(Name) &&
                    Array.Equals(d.Values, Values);
            }

            return base.Equals(obj);
        }

        #endregion

        #region ICalendarData Members

        virtual public object Value
        {
            get
            {
                if (m_Values != null &&
                    m_Values.Count > 0)
                    return m_Values[0];
                return null;
            }
            set
            {
                m_Values = new List<object>(new object[] { value });
            }
        }

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public IList<object> Values
        {
            get { return m_Values; }
            set { m_Values = value; }
        }

        #endregion
    }
}
