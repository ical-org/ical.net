using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace PerfTests
{
    public class OccurrencePerfTests
    {
        [Benchmark]
        public void MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar()
            => MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar_();
        [Test(ExpectedResult = 40)]
        public static int MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar_()
        {
            var calendar = GetFourCalendarEventsWithUntilRule();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1);
            return calendar.GetOccurrences(searchStart, searchEnd).Count;
        }

        [Benchmark]
        public void MultipleEventsWithUntilOccurrences() => MultipleEventsWithUntilOccurrences_();
        [Test(ExpectedResult = 40)]
        public static int MultipleEventsWithUntilOccurrences_()
        {
            var calendar = GetFourCalendarEventsWithUntilRule();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1);
            return calendar.Events
                .SelectMany(e => e.GetOccurrences(searchStart, searchEnd))
                .Count();
        }

        [Benchmark]
        public void MultipleEventsWithUntilOccurrencesEventsAsParallel() => MultipleEventsWithUntilOccurrencesEventsAsParallel_();
        [Test(ExpectedResult = 40)]
        public static int MultipleEventsWithUntilOccurrencesEventsAsParallel_()
        {
            var calendar = GetFourCalendarEventsWithUntilRule();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1).AddDays(10);
            return calendar.Events
                .AsParallel()
                .SelectMany(e => e.GetOccurrences(searchStart, searchEnd))
                .Count();
        }

        static Calendar GetFourCalendarEventsWithUntilRule()
        {
            const string tzid = "America/New_York";
            const int limit = 4;

            var startTime = DateTime.Now.AddDays(-1);
            var interval = TimeSpan.FromDays(1);

            var events = Enumerable
                .Range(0, limit)
                .Select(n =>
                {
                    var rrule = new RecurrencePattern(FrequencyType.Daily, 1)
                    {
                        Until = startTime.AddDays(10),
                    };

                    var e = new CalendarEvent
                    {
                        Start = new CalDateTime(startTime.AddMinutes(5), tzid),
                        End = new CalDateTime(startTime.AddMinutes(10), tzid),
                        RecurrenceRules = new List<RecurrencePattern> { rrule },
                    };
                    startTime += interval;
                    return e;
                });

            var c = new Calendar();
            c.Events.AddRange(events);
            return c;
        }

        [Benchmark]
        public void MultipleEventsWithCountOccurrencesSearchingByWholeCalendar()
            => MultipleEventsWithCountOccurrencesSearchingByWholeCalendar_();
        [Test(ExpectedResult = 400)]
        public static int MultipleEventsWithCountOccurrencesSearchingByWholeCalendar_()
        {
            var calendar = GetFourCalendarEventsWithCountRule();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1);
            return calendar.GetOccurrences(searchStart, searchEnd).Count;
        }

        [Benchmark]
        public static int MultipleEventsWithCountOccurrences() => MultipleEventsWithCountOccurrences_();
        [Test(ExpectedResult = 400)]
        public static int MultipleEventsWithCountOccurrences_()
        {
            var calendar = GetFourCalendarEventsWithCountRule();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1);
            return calendar.Events
                .SelectMany(e => e.GetOccurrences(searchStart, searchEnd))
                .Count();
        }

        [Benchmark]
        public void MultipleEventsWithCountOccurrencesEventsAsParallel()
            => MultipleEventsWithCountOccurrencesEventsAsParallel_();
        [Test(ExpectedResult = 400)]
        public static int MultipleEventsWithCountOccurrencesEventsAsParallel_()
        {
            var calendar = GetFourCalendarEventsWithCountRule();
            var searchStart = calendar.Events.First().DtStart.AddYears(-1);
            var searchEnd = calendar.Events.Last().DtStart.AddYears(1).AddDays(10);
            return calendar.Events
                .AsParallel()
                .SelectMany(e => e.GetOccurrences(searchStart, searchEnd))
                .Count();
        }

        static Calendar GetFourCalendarEventsWithCountRule()
        {
            const string tzid = "America/New_York";
            const int limit = 4;

            var startTime = DateTime.Now.AddDays(-1);
            var interval = TimeSpan.FromDays(1);

            var events = Enumerable
                .Range(0, limit)
                .Select(n =>
                {
                    var rrule = new RecurrencePattern(FrequencyType.Daily, 1)
                    {
                        Count = 100,
                    };

                    var e = new CalendarEvent
                    {
                        Start = new CalDateTime(startTime.AddMinutes(5), tzid),
                        End = new CalDateTime(startTime.AddMinutes(10), tzid),
                        RecurrenceRules = new List<RecurrencePattern> { rrule },
                    };
                    startTime += interval;
                    return e;
                });

            var c = new Calendar();
            c.Events.AddRange(events);
            return c;
        }
    }
}
