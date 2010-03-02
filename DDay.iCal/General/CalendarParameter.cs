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
    [DataContract(Name = "CalendarParameter", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class CalendarParameter : 
        CalendarValueObject,
        ICalendarParameter
    {
        #region Constructors

        public CalendarParameter() {}
        public CalendarParameter(string name) : base(name) {}
        public CalendarParameter(string name, string value) : base(name, value) { }

        #endregion

        #region IKeyedObject Members

        virtual public string Key
        {
            get { return Name; }
        }

        #endregion
    }
}
