using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
    public class iCalendarSerializer : ComponentSerializer 
    {
        #region Readonly Fields

        static private readonly string _Version = "2.0";
        static private readonly string _ProdID = "-//DDay.iCal//NONSGML ddaysoftware.com//EN";

        #endregion

        #region Private Fields

        private IICalendar m_iCalendar;        

        #endregion

        #region Public Properties

        public IICalendar iCalendar
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
        public iCalendarSerializer(IICalendar iCal) : base(iCal)
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

        public override void Serialize(Stream stream, Encoding encoding)
        {
            // Set default values for these required properties
            // NOTE: fixes bug #1672047            
            if (string.IsNullOrEmpty(iCalendar.Version))
                iCalendar.Version = _Version;
            if (string.IsNullOrEmpty(iCalendar.ProductID))
                iCalendar.ProductID = _ProdID;

            base.Serialize(stream, encoding);
        }

        public override object Deserialize(TextReader tr, Type iCalendarType)
        {
            // Normalize line endings, so "\r" is treated the same as "\r\n"
            // NOTE: fixed bug #1773194 - Some applications emit mixed line endings
            TextReader textReader = NormalizeLineEndings(tr, SerializationContext.EnsureAccurateLineNumbers);

            // Create a lexer for our text stream
            iCalLexer lexer = new iCalLexer(textReader);
            iCalParser parser = new iCalParser(lexer);

            // Determine the calendar type we'll be using when constructing
            // iCalendar objects...
            parser.iCalendarType = iCalendarType;

            // Parse the iCalendar!
            iCalendarCollection calendarCollection = parser.icalobject(SerializationContext);

            // Close our text stream
            textReader.Close();

            // Return the parsed iCalendar
            return calendarCollection;
        }

        #endregion
    }
}
