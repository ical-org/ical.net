//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Evaluation;

namespace Ical.Net.Benchmarks;

public class OccurencePerfTests
{
    private Calendar _calendarFourEvents = null!;
    private Calendar _calendarWithRecurrences = null!;

    [GlobalSetup]
    public void Setup()
    {
        _calendarFourEvents = GetFourCalendarEventsWithUntilRule();
        _calendarWithRecurrences = GenerateCalendarWithRecurrences();
    }

    [Benchmark]
    public void GetOccurrences()
    {
        _ = _calendarWithRecurrences.GetOccurrences().ToList();
    }

    [Benchmark]
    public void MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar()
    {
        var searchStart = _calendarFourEvents.Events.First().DtStart!.AddYears(-1);
        var searchEnd = _calendarFourEvents.Events.Last().DtStart!.AddYears(1);
        _ = _calendarFourEvents.GetOccurrences(searchStart).TakeWhile(p => p.Period.StartTime.LessThan(searchEnd));
    }

    [Benchmark]
    public void MultipleEventsWithUntilOccurrences()
    {
        var searchStart = _calendarFourEvents.Events.First().DtStart!.AddYears(-1);
        var searchEnd = _calendarFourEvents.Events.Last().DtStart!.AddYears(1);
        _ = _calendarFourEvents.Events
            .SelectMany(e => e.GetOccurrences(searchStart).TakeWhile(p => p.Period.StartTime.LessThan(searchEnd)))
            .ToList();
    }

    [Benchmark]
    public void MultipleEventsWithUntilOccurrencesEventsAsParallel()
    {
        var searchStart = _calendarFourEvents.Events.First().DtStart!.AddYears(-1);
        var searchEnd = _calendarFourEvents.Events.Last().DtStart!.AddYears(1).AddDays(10);
        _ = _calendarFourEvents.Events
            .AsParallel()
            .SelectMany(e => e.GetOccurrences(searchStart).TakeWhile(p => p.Period.StartTime.LessThan(searchEnd)))
            .ToList();
    }

    private static Calendar GenerateCalendarWithRecurrences()
    {
        var calendar = new Calendar();
        var dailyEvent = new CalendarEvent
        {
            Start = new CalDateTime(2025, 3, 1),
            End = null,
            RecurrenceRules = new List<RecurrencePattern>
            {
                new RecurrencePattern(FrequencyType.Daily, 1) { Count = 1000 }
            }
        };
        calendar.Events.Add(dailyEvent);
        return calendar;
    }

    private static Calendar GetFourCalendarEventsWithUntilRule()
    {
        const string tzid = "America/New_York";
        const int limit = 4;

        var startTime = CalDateTime.Now.AddDays(-1);
        var interval = TimeSpan.FromDays(1);

        var events = Enumerable
            .Range(0, limit)
            .Select(n =>
            {
                var rrule = new RecurrencePattern(FrequencyType.Daily, 1)
                {
                    Until = new CalDateTime(startTime.AddDays(10)),
                };

                var e = new CalendarEvent
                {
                    Start = startTime.AddMinutes(5).ToTimeZone(tzid),
                    End = startTime.AddMinutes(10).ToTimeZone(tzid),
                    RecurrenceRules = new List<RecurrencePattern> { rrule },
                };
                startTime = startTime.Add(Duration.FromTimeSpanExact(interval));
                return e;
            });

        var c = new Calendar();
        c.Events.AddRange(events);
        return c;
    }

    [Benchmark]
    public void MultipleEventsWithCountOccurrencesSearchingByWholeCalendar()
    {
        var calendar = GetFourCalendarEventsWithCountRule();
        var searchStart = calendar.Events.First().DtStart!.AddYears(-1);
        var searchEnd = calendar.Events.Last().DtStart!.AddYears(1);
        _ = calendar.GetOccurrences(searchStart).TakeWhile(p => p.Period.StartTime.LessThan(searchEnd));
    }

    [Benchmark]
    public void MultipleEventsWithCountOccurrences()
    {
        var calendar = GetFourCalendarEventsWithCountRule();
        var searchStart = calendar.Events.First().DtStart!.AddYears(-1);
        var searchEnd = calendar.Events.Last().DtStart!.AddYears(1);
        _ = calendar.Events
            .SelectMany(e => e.GetOccurrences(searchStart).TakeWhile(p => p.Period.StartTime.LessThan(searchEnd)))
            .ToList();
    }

    [Benchmark]
    public void MultipleEventsWithCountOccurrencesEventsAsParallel()
    {
        var calendar = GetFourCalendarEventsWithCountRule();
        var searchStart = calendar.Events.First().DtStart!.AddYears(-1);
        var searchEnd = calendar.Events.Last().DtStart!.AddYears(1).AddDays(10);
        _ = calendar.Events
            .AsParallel()
            .SelectMany(e => e.GetOccurrences(searchStart).TakeWhile(p => p.Period.StartTime.LessThan(searchEnd)))
            .ToList();
    }

    private static Calendar GetFourCalendarEventsWithCountRule()
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
                startTime = startTime.Add(interval);
                return e;
            });

        var c = new Calendar();
        c.Events.AddRange(events);
        return c;
    }
}
