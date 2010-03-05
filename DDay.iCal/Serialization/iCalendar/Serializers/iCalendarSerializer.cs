using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class iCalendarSerializer :
        ComponentSerializer
    {
        #region Private Fields

        IICalendar m_ICalendar;

        #endregion

        #region Constructors

        public iCalendarSerializer() : base()
        {
        }

        public iCalendarSerializer(IICalendar iCal)
        {
            m_ICalendar = iCal;
        }

        public iCalendarSerializer(ISerializationContext ctx) : base(ctx)
        {
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
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                Serialize(iCal, fs, new UTF8Encoding());
            }
        }        

        #endregion

        #region Overrides

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                // Normalize the text before parsing it
                tr = TextUtil.Normalize(tr, SerializationContext);

                // Create a lexer for our text stream
                iCalLexer lexer = new iCalLexer(tr);
                iCalParser parser = new iCalParser(lexer);

                // Parse the iCalendar(s)!
                iCalendarCollection iCalendars = parser.icalendar(SerializationContext);

                // Close our text stream
                tr.Close();

                // Return the parsed iCalendar(s)
                return iCalendars;
            }
            return null;
        }

        #endregion
    }
}
