using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents the address of an iCalendar user.
    /// In iCalendar terms, this is usually represented by an
    /// e-mail address, in the following form:
    /// <c>MAILTO:email.address@host.com</c>
    /// </summary>
    [DebuggerDisplay("{Value}")]
#if DATACONTRACT
    [DataContract(Name = "Cal_Address", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class CalendarAddress : URI
    {
        #region Constructors

        public CalendarAddress() : base() { }
        public CalendarAddress(string value) : this(string.Empty, value) { }
        protected CalendarAddress(string name, string value) : this()
        {
            this.Name = name;
            ICalendarObject obj = this;
            if (!base.TryParse(value, ref obj) ||
                string.IsNullOrEmpty(this.Scheme))
                CopyFrom(Parse("MAILTO:" + value));
        }

        #endregion

        #region Operators

        static public implicit operator string(CalendarAddress addr)
        {
            return addr != null ? addr.Value : null;            
        }

        static public implicit operator CalendarAddress(string s)
        {
            return new CalendarAddress(s);
        }

        #endregion
    }
}
