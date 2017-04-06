using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers;

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

        private static string SerializeToString(IEvent calendarEvent) => SerializeToString(new Calendar { Events = { calendarEvent } });

        private static string SerializeToString(Calendar iCalendar) => new CalendarSerializer().SerializeToString(iCalendar);
    }
}
