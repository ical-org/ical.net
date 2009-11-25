using System;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// This class represents a single line of text in an <see cref="iCalendar"/>
    /// (.ics) file.
    /// </summary>
    /// <remarks>
    /// In the RFC 2445 specification, content lines are wrapped
    /// with a newline, followed by a whitespace character, after they surpass
    /// 75 characters in length.  A <see cref="ContentLine"/> represents
    /// this entire line, including potential line wrap sequences ("\r\n ").
    /// </remarks>
#if DATACONTRACT
    [DataContract(Name = "ContentLine", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]    
#endif
    [Serializable]
    public class ContentLine : iCalObject
    {
        #region Private Fields

        private string m_value;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        #endregion

        #region Constructors

        public ContentLine(iCalObject parent) : base(parent) { }
        public ContentLine(iCalObject parent, int line, int col) : base(parent, line, col) { }

        #endregion
    }
}
