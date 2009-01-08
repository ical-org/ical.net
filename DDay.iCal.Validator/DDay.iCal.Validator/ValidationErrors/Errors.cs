using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    static public class Errors
    {
        /// <summary>
        /// The calendar has not been loaded.
        /// </summary>
        public const int CalendarNotLoaded = 0x0001;

        /// <summary>
        /// The iCalendar is missing the VERSION parameter.
        /// </summary>
        public const int MissingVersionError = 0x0002;

        /// <summary>
        /// A mismatching calendar version was used for validation.
        /// The validation used does not support the calendar version.
        /// </summary>
        public const int VersionMismatchError = 0x0003;

        /// <summary>
        /// The calendar could not be parsed by the iCalendar parser.
        /// </summary>
        public const int CalendarParseError = 0x0004;
    }
}
