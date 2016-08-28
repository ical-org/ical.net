using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;

namespace NugetTester
{
    class Program
    {
        const string ical = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTSTART;VALUE=DATE:20120823
DTEND;VALUE=DATE:20120824
RRULE:FREQ=DAILY;UNTIL=20120824
EXDATE;VALUE=DATE:20120824
EXDATE;VALUE=DATE:20120823
DTSTAMP:20131031T111655Z
CREATED:20120621T142631Z
STATUS:CONFIRMED
SUMMARY:Test Summary
TRANSP:TRANSPARENT
END:VEVENT
END:VCALENDAR";

        static void Main(string[] args)
        {
            var now = DateTime.Now;
            var later = now.AddHours(1);

            var e = new Event
            {
                DtStart = new CalDateTime(now),
                DtEnd = new CalDateTime(later),
            };
            e.AddProperty("X-MICROSOFT-CDO-BUSYSTATUS", "OOF"); // I think "OOF" is right per the MS documentation

            var calendar = new Calendar();
            calendar.Events.Add(e);

            //var serializer = new CalendarSerializer(new SerializationContext());
            //var icalString = serializer.SerializeToString(calendar);
            //Console.WriteLine(icalString);

            var collection = Calendar.LoadFromStream(new StringReader(ical));
            var firstEvent = collection.First().Events.First();
            var occurrences = firstEvent.GetOccurrences(new CalDateTime(2010, 1, 1, "US-Eastern"), new CalDateTime(2016, 12, 31, "US-Eastern"));

            Console.ReadLine();
        }
    }
}
