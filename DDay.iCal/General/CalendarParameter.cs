using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

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
#if DATACONTRACT
    [DataContract(Name = "Parameter", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class CalendarParameter : 
        CalendarObject,
        ICalendarParameter
    {
        #region Private Fields

        private IList<string> m_Values = new List<string>();
        
        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public IList<string> Values
        {
            get { return m_Values; }
            set { m_Values = value; }
        }

        #endregion

        #region Constructors

        public CalendarParameter() {}
        public CalendarParameter(string name) : base(name) {}
        public CalendarParameter(string name, string value) : this(name)
        {
            Values.Add(value);
        }

        #endregion

        #region Public Methods

        public void CopyFrom(object obj)
        {
            if (obj is CalendarParameter)
            {
                Values.Clear();

                CalendarParameter p = (CalendarParameter)obj;
                foreach (string value in p.Values)
                    Values.Add(value);
            }
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            CalendarParameter p = obj as CalendarParameter;
            if (p != null)
                return object.Equals(p.Name, Name);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Name != null)
                return Name.GetHashCode();
            else return base.GetHashCode();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            ICalendarParameter p = obj as ICalendarParameter;
            if (p != null)
            {
                Values.Clear();
                foreach (string s in p.Values)
                    Values.Add(s);
            }
        }
        
        #endregion

        #region IKeyedObject Members

        public string Key
        {
            get { return Name; }
        }

        #endregion
    }
}
