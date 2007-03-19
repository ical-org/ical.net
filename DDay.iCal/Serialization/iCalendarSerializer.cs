using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
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

        private DDay.iCal.iCalendar m_iCal;

        #endregion

        #region Constructors

        public iCalendarSerializer(DDay.iCal.iCalendar iCal) : base(iCal)
        {
            this.m_iCal = iCal;
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

        #endregion
    }
}
