using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Components;
using DDay.iCal.Serialization.iCalendar;
using DDay.iCal.Serialization.iCalendar.Components;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// A class that serializes <see cref="iCalendar"/>s into the
    /// standard iCalendar (.ics) format.
    /// <remarks>
    /// The default encoding for .ics files is UTF-8.
    /// </remarks>
    /// <example>
    /// The following code demonstrates how to serialize an iCalendar to file:
    /// <code>
    /// iCalendar iCal = new iCalendar();
    /// 
    /// Event evt = iCal.Create&lt;Event&gt;();
    /// evt.Start = DateTime.Now;
    /// evt.Summary = "Event summary";
    /// 
    /// iCalendarSerializer serializer = new iCalendarSerializer(iCal);
    /// serializer.Serialize(@"filename.ics");
    /// </code>
    /// </example>
    /// </summary>
    public class iCalendarSerializer : ComponentBaseSerializer 
    {
        #region Private Fields

        private DDay.iCal.iCalendar m_iCalendar;        

        #endregion

        #region Public Properties

        public DDay.iCal.iCalendar iCalendar
        {
            get { return m_iCalendar; }
            set
            {
                if (!object.Equals(m_iCalendar, value))
                {
                    m_iCalendar = value;
                    base.Component = value;
                }
            }
        }

        #endregion

        #region Constructors

        public iCalendarSerializer() { }
        public iCalendarSerializer(DDay.iCal.iCalendar iCal) : base(iCal)
        {
            iCalendar = iCal;
        }

        #endregion

        #region Public Methods

        public void Serialize(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            UTF8Encoding utf8 = new UTF8Encoding();
            Serialize(fs, utf8);
            fs.Close();
        }

        public override iCalObject Deserialize(TextReader tr, Type iCalendarType)
        {
            // Normalize line endings, so "\r" is treated the same as "\r\n"
            // NOTE: fixed bug #1773194 - Some applications emit mixed line endings
            TextReader textReader = NormalizeLineEndings(tr);

            // Create a lexer for our text stream
            iCalLexer lexer = new iCalLexer(textReader);
            iCalParser parser = new iCalParser(lexer);

            // Determine the calendar type we'll be using when constructing
            // iCalendar objects...
            parser.iCalendarType = iCalendarType;

            // Parse the iCalendar!
            DDay.iCal.iCalendar iCal = parser.icalobject();

            // Close our text stream
            textReader.Close();

            // Return the parsed iCalendar
            return iCal;
        }

        #endregion
    }
}
