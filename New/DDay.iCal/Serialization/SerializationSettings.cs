using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public class SerializationSettings :
        ISerializationSettings
    {
        #region Private Fields

        private Type m_iCalendarType = typeof(iCalendar);        
        private bool m_EnsureAccurateLineNumbers = false;
        private ParsingModeType m_ParsingMode = ParsingModeType.Strict;

        #endregion

        #region ISerializationSettings Members

        virtual public Type iCalendarType
        {
            get { return m_iCalendarType; }
            set { m_iCalendarType = value; }
        }

        virtual public bool EnsureAccurateLineNumbers
        {
            get { return m_EnsureAccurateLineNumbers; }
            set { m_EnsureAccurateLineNumbers = value; }
        }

        virtual public ParsingModeType ParsingMode
        {
            get { return m_ParsingMode; }
            set { m_ParsingMode = value; }
        }

        #endregion
    }
}
