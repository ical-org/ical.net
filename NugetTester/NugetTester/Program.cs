using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

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

                var unserialized = Calendar.Load(serialized).Events.First();
                Console.WriteLine(unserialized.Start);

                const string brokenIcal = @"BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.0//EN
VERSION:2.0
BEGIN:VEVENT
DESCRIPTION:Foo
DTEND:20171115T045900Z
DTSTAMP:20171113T231608Z
DTSTART:20171114T140000Z
SEQUENCE:0
SUMMARY:Blockchain For Wall Street
UID:7aa68f0e-adc5-4af2-80b9-429ef1f5f193
END:VEVENT
END:VCALENDAR";

                var deserializedBroken = Calendar.Load(brokenIcal);
                var firstEvent = deserializedBroken.Events.First();
                Console.WriteLine(firstEvent.Start);
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
                RecurrenceRules = new List<RecurrencePattern> { GetSampleRecurrenceRules() }
            };
            return e;
        }

        private static RecurrencePattern GetSampleRecurrenceRules()
            => new RecurrencePattern(FrequencyType.Daily, 1);

        private static string SerializeToString(CalendarEvent calendarEvent)
            => SerializeToString(new Calendar { Events = { calendarEvent } });

        private static string SerializeToString(Calendar iCalendar)
            => new CalendarSerializer().SerializeToString(iCalendar);
    }
}
