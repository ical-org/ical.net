using System;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization
{
    public class SerializationSettings :
        ISerializationSettings
    {
        #region Private Fields

        private Type m_iCalendarType = typeof(Net.iCalendar);        
        private bool m_EnsureAccurateLineNumbers;
        private ParsingModeType m_ParsingMode = ParsingModeType.Strict;
        private bool m_StoreExtraSerializationData;        

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

        virtual public bool StoreExtraSerializationData
        {
            get { return m_StoreExtraSerializationData; }
            set { m_StoreExtraSerializationData = value; }
        }

        #endregion
    }
}
