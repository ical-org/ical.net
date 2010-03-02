using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization
{
    public class iCalendarSerializer :
        ComponentSerializer
    {
        #region Private Fields

        IICalendar m_ICalendar;

        #endregion

        #region Constructors

        public iCalendarSerializer()
        {
        }

        public iCalendarSerializer(IICalendar iCal)
        {
            m_ICalendar = iCal;
        } 

        #endregion

        #region Public Methods

        [Obsolete("Use the Serialize(IICalendar iCal, string filename) method instead.")]
        virtual public void Serialize(string filename)
        {
            if (m_ICalendar != null)
                Serialize(m_ICalendar, filename);
        }

        [Obsolete("Use the SerializeToString(ICalendarObject obj) method instead.")]
        virtual public string SerializeToString()
        {
            return SerializeToString(m_ICalendar);
        }

        virtual public void Serialize(IICalendar iCal, string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            UTF8Encoding utf8 = new UTF8Encoding();
            Serialize(iCal, fs, utf8);
            fs.Close();
        }        

        #endregion
    }
}
