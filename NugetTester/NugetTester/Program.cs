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
            try
            {
                var c = new Calendar();
                c.Events.Add(GetSampleEvent());
                var serialized = SerializeToString(GetSampleEvent());
                Console.WriteLine(serialized);

                var unserialized = DeserializeCalendarEvent(serialized);
                Console.WriteLine(unserialized.Start);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        private static readonly DateTime _now = DateTime.Now;
        private static readonly DateTime _later = _now.AddHours(1);
        private static CalendarEvent GetSampleEvent()
        {
            var e = new CalendarEvent
            {
                Start = new CalDateTime(_now),
                End = new CalDateTime(_later),
                RecurrenceRules = new List<RecurrencePattern> {GetSampleRecurrenceRules()}
            };
            return e;
        }

        private static RecurrencePattern GetSampleRecurrenceRules()
            => new RecurrencePattern(FrequencyType.Daily, 1);

        private static CalendarEvent DeserializeCalendarEvent(string ical)
        {
            var calendar = DeserializeCalendar(ical);
            var calendarEvent = calendar.First().Events.First() as CalendarEvent;
            return calendarEvent;
        }

        private static CalendarCollection DeserializeCalendar(string ical)
        {
            using (var reader = new StringReader(ical))
            {
                return Calendar.LoadFromStream(reader) as CalendarCollection;
            }
        }

        private static string SerializeToString(CalendarEvent calendarEvent) => SerializeToString(new Calendar { Events = { calendarEvent } });

        private static string SerializeToString(Calendar iCalendar) => new CalendarSerializer().SerializeToString(iCalendar);
    }
}
