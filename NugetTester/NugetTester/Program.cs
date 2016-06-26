using System;
using System.Collections.Generic;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace NugetTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var now = DateTime.Now;
            var later = now.AddHours(1);
            var rrule = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };

            var e = new Event
            {
                DtStart = new CalDateTime(now),
                DtEnd = new CalDateTime(later),
                Duration = TimeSpan.FromHours(1),
                RecurrenceRules = new List<IRecurrencePattern> {rrule},
            };

            var foo = new Calendar();
            foo.Events.Add(e);

            Console.ReadLine();
        }
    }
}
