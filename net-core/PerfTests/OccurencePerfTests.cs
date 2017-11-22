using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace PerfTests
{
    public class OccurencePerfTests
    {
        [Benchmark]
        public void TenYears()
        {
            const string tzid = "Eastern Standard Time";
            var startDt = DateTime.Now;
            var endDt = startDt.AddHours(1);

            var start = new CalDateTime(startDt, tzid);
            var end = new CalDateTime(endDt, tzid);

            var rrule = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Until = startDt.AddYears(10),
            };

            var e = new CalendarEvent
            {
                Start = start,
                End = end,
                RecurrenceRules = new List<RecurrencePattern> { rrule },
            };

            var searchStart = startDt.AddYears(-1);
            var searchEnd = startDt.AddYears(11);
            var occurrences = e.GetOccurrences(searchStart, searchEnd);
        }

        [Benchmark]
        public void GetOccurrencesWithCalendarWithMultipleEvents()
        {
            var calendar = GetManyCalendarEvents();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1);
            var occurences = calendar.GetOccurrences(searchStart, searchEnd);
        }

        [Benchmark]
        public void MultipleEventOccurrences()
        {
            var calendar = GetManyCalendarEvents();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1);
            var eventOccurrences = calendar.Events
                .SelectMany(e => e.GetOccurrences(searchStart, searchEnd))
                .ToList();
        }

        [Benchmark]
        public void MultipleEventOccurrencesAsParallel()
        {
            var calendar = GetManyCalendarEvents();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1).AddDays(10);
            var eventOccurrences = calendar.Events
                .AsParallel()
                .SelectMany(e => e.GetOccurrences(searchStart, searchEnd))
                .ToList();
        }

        private Calendar GetManyCalendarEvents()
        {
            const string tzid = "America/New_York";
            const int limit = 10;
            var list = new List<CalendarEvent>(limit);

            var startTime = DateTime.Now.AddDays(-1);
            var interval = TimeSpan.FromDays(1);

            for (var i = 0; i < limit; i++)
            {
                var rrule = new RecurrencePattern(FrequencyType.Hourly, 1)
                {
                    Until = startTime.AddYears(1),
                };

                var e = new CalendarEvent
                {
                    Start = new CalDateTime(startTime.AddMinutes(5), tzid),
                    End = new CalDateTime(startTime.AddMinutes(10), tzid),
                    RecurrenceRules = new List<RecurrencePattern> {rrule},
                };
                list.Add(e);

                startTime += interval;
            }

            var c = new Calendar();
            foreach (var e in list)
            {
                c.Events.Add(e);
            }
            return c;
        }
    }
}
