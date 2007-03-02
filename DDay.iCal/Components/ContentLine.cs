using System;
using System.Collections;
using System.Text;

namespace DDay.iCal.Components
{
    /// <summary>
    /// This class represents a single line of text in an <see cref="iCalendar"/>
    /// (.ics) file.
    /// </summary>
    /// <remarks>
    /// In the RFC 2445 specification, content lines are wrapped
    /// with a newline, followed by a whitespace character, after they surpass
    /// 75 characters in length.  Therefore, a <see cref="ContentLine"/> represents
    /// this entire line, after "unwrapping" the newline + whitespace character
    /// sequence.
    /// </remarks>
    public class ContentLine : iCalObject
    {
        #region Private Fields

        private string m_value;

        #endregion

        #region Public Properties

        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        #endregion

        #region Constructors

        public ContentLine(iCalObject parent) : base(parent) { }

        #endregion
    }
}
