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
        static void Main(string[] args)
        {
            var vEvent = new Event
            {
                DtStart = new CalDateTime(DateTime.Parse("2016-01-01")),
                DtEnd = new CalDateTime(DateTime.Parse("2016-01-05")),
            };

            var vEvent2 = new Event
            {
                DtStart = new CalDateTime(DateTime.Parse("2016-03-01")),
                DtEnd = new CalDateTime(DateTime.Parse("2016-03-05")),
            };

            var calendar = new Calendar();
            calendar.Events.Add(vEvent);
            calendar.Events.Add(vEvent2);

            var searchStart = DateTime.Parse("2015-12-29");
            var searchEnd = DateTime.Parse("2017-02-10");
            var occurrences = calendar.GetOccurrences(searchStart, searchEnd);

            Console.ReadLine();
        }
    }
}
