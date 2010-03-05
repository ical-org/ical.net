using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace DDay.iCal
{
    /// <summary>
    /// A class that provides additional information about a <see cref="ContentLine"/>.
    /// </summary>
    /// <remarks>
    /// <example>
    /// For example, a DTSTART line may look like this: <c>DTSTART;VALUE=DATE:20060116</c>.  
    /// The <c>VALUE=DATE</c> portion is a <see cref="Parameter"/> of the DTSTART value.
    /// </example>
    /// </remarks>
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

        #region ICalendarParameter Members

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
                if (value != null)
                    m_Values = new string[] { value };
                else m_Values = null;                    
            }
        }

        virtual public string[] Values
        {
            get { return m_Values; }
            set { m_Values = value; }
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
