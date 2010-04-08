using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace DDay.iCal
{
    [DebuggerDisplay("{Name}={string.Join(\",\", Values)}")]
#if DATACONTRACT
    [DataContract(Name = "CalendarParameter", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class CalendarParameter : 
        CalendarObject,
        ICalendarParameter
    {
        #region Private Fields

        string[] m_Values;

        #endregion

        #region Constructors

        public CalendarParameter() {}
        public CalendarParameter(string name) : base(name) {}
        public CalendarParameter(string name, string value) : base(name)
        {
            Value = value;
        }
        public CalendarParameter(string name, string[] values) : base(name)
        {
            m_Values = values;
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable c)
        {
            base.CopyFrom(c);

            ICalendarParameter p = c as ICalendarParameter;
            if (p != null)
            {
                if (p.Values != null)
                {
                    Values = new string[p.Values.Length];
                    Array.Copy(p.Values, Values, p.Values.Length);
                }
            }
        }

        #endregion

        #region ICalendarParameter Members

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected void OnValueChanged(object oldValue, object newValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs(oldValue, newValue));
        }

        virtual public string Value
        {
            get
            {
                if (m_Values != null &&
                    m_Values.Length > 0)
                    return m_Values[0];
                return null;
            }
            set
            {
                object oldValues = m_Values;

                string oldValue = null;
                if (m_Values != null && m_Values.Length > 0)
                    oldValue = m_Values[0];

                if (!object.Equals(oldValue, value))
                {
                    if (value != null)
                        m_Values = new string[] { value };
                    else m_Values = null;

                    OnValueChanged(oldValues, m_Values);
                }
            }
        }

        virtual public string[] Values
        {
            get { return m_Values; }
            set
            {
                if (!object.Equals(m_Values, value))
                {
                    object old = m_Values;
                    m_Values = value;
                    OnValueChanged(old, m_Values);
                }
            }
        }

        #endregion

        #region IKeyedObject Members

        virtual public string Key
        {
            get { return Name; }
        }

        #endregion
    }
}
