namespace Ical.Net.Serialization.iCalendar
{
    public class SerializationInfo
    {
        /// <summary>
        /// Returns the line number where this calendar
        /// object was found during parsing.
        /// </summary>
        private int Line { get; set; }

        /// <summary>
        /// Returns the column number where this calendar
        /// object was found during parsing.
        /// </summary>
        private int Column { get; set; }

        public SerializationInfo() {}

        public SerializationInfo(int line, int column) {}
    }
}