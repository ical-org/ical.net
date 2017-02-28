using System;
using System.IO;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;

namespace NugetTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
        }

        private static Event DeserializeCalendarEvent(string ical)
        {
            var calendar = DeserializeCalendar(ical);
            var calendarEvent = calendar.First().Events.First() as Event;
            return calendarEvent;
        }

        private static CalendarCollection DeserializeCalendar(string ical)
        {
            using (var reader = new StringReader(ical))
            {
                return Calendar.LoadFromStream(reader) as CalendarCollection;
            }
        }
    }
}
