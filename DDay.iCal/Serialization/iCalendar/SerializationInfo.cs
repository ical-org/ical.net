using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization.iCalendar
{
    public class SerializationInfo
    {
        /// <summary>
        /// Returns the line number where this calendar
        /// object was found during parsing.
        /// </summary>
        int Line { get; set; }

        /// <summary>
        /// Returns the column number where this calendar
        /// object was found during parsing.
        /// </summary>
        int Column { get; set; }

        public SerializationInfo()
        {
        }

        public SerializationInfo(int line, int column)
        {
        }
    }
}
