using Ical.Net.Serialization.iCalendar.Serializers;

namespace Ical.Net.UnitTests
{
    internal class SerializationHelpers
    {
        public static string SerializeToString(CalendarEvent calendarEvent)
            => SerializeToString(new Calendar { Events = { calendarEvent } });

        public static string SerializeToString(Calendar iCalendar)
            => new CalendarSerializer().SerializeToString(iCalendar);
    }
}
