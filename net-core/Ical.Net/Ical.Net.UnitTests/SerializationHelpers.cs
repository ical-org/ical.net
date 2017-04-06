using System.IO;
using System.Linq;
using Ical.Net.Serialization.iCalendar.Serializers;

namespace Ical.Net.UnitTests
{
    internal class SerializationHelpers
    {
        public static CalendarEvent DeserializeCalendarEvent(string ical)
        {
            var calendar = DeserializeCalendar(ical);
            var calendarEvent = calendar.First().Events.First();
            return calendarEvent;
        }

        public static CalendarCollection DeserializeCalendar(string ical)
        {
            using (var reader = new StringReader(ical))
            {
                return Calendar.LoadFromStream(reader);
            }
        }

        public static string SerializeToString(CalendarEvent calendarEvent) => SerializeToString(new Calendar { Events = { calendarEvent } });

        public static string SerializeToString(Calendar iCalendar) => new CalendarSerializer().SerializeToString(iCalendar);
    }
}
