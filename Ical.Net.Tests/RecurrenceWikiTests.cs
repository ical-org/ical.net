//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

public static class RecurrenceWikiTestsUtilsExtensions
{
    public static Calendar ToCalendar(this CalendarEvent eve, Action<Calendar>? func = null)
    {
        var calendar = new Calendar();
        calendar.Events.Add(eve);
        if (func is not null)
        {
            func(calendar);
        }

        return calendar;
    }

    public static Calendar ToCalendar(this IEnumerable<CalendarEvent> eves, Action<Calendar>? func = null)
    {
        var calendar = new Calendar();
        calendar.Events.AddRange(eves);
        if (func is not null)
        {
            func(calendar);
        }

        return calendar;
    }

    public static CalendarEvent With(this CalendarEvent eve, Action<CalendarEvent> func)
    {
        func(eve);
        return eve;
    }

    public static CalendarEvent WithRecurrenceRule(this CalendarEvent eve, params RecurrencePattern[] rules)
    {
        eve.RecurrenceRules = rules;
        return eve;
    }

    public static RecurrencePattern With(this RecurrencePattern pattern, Action<RecurrencePattern> func)
    {
        func(pattern);
        return pattern;
    }
}

[TestFixture, Category("Recurrence")]
public class RecurrenceWikiTests
{
    [Test]
    public void WikiExample()
    {
        // Setup CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern()
        {
            Frequency = FrequencyType.Daily,
            Interval = 2,
            Count = 2
        };

        var calendarEvent = new CalendarEvent()
        {
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRules = [recurrence]
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var calendarAsIcs = calendarSerializer.SerializeToString(calendar);
        Assert.That(calendarAsIcs, Is.Not.Null);

        // Calculate all occurrences
        var occurrences = calendar.GetOccurrences();
        Assert.That(occurrences.Count(), Is.EqualTo(2));

        // Test first occurrence
        var first = occurrences.First().Period;
        Assert.That(first.StartTime?.Value.ToString("s"), Is.Not.Null.And.EqualTo("2025-07-10T09:00:00"));
        Assert.That(first.Duration, Is.Not.Null.And.EqualTo(new Duration(hours: 1)));
        Assert.That(first.EffectiveEndTime?.Value.ToString("s"), Is.Not.Null.And.EqualTo("2025-07-10T10:00:00"));

        // Test second occurrence
        var second = occurrences.Last().Period;
        Assert.That(second.StartTime?.Value.ToString("s"), Is.Not.Null.And.EqualTo("2025-07-12T09:00:00"));
        Assert.That(second.Duration, Is.Not.Null.And.EqualTo(new Duration(hours: 1)));
        Assert.That(second.EffectiveEndTime?.Value.ToString("s"), Is.Not.Null.And.EqualTo("2025-07-12T10:00:00"));
    }

    [Test]
    [TestCaseSource(nameof(GetDailyTestData))]
    public void CalendarSerializeAndOccurrences(
        Calendar calendar,
        string expectedCalendar,
        string expectedOccurrences
        )
    {
        // Test Calendar serialization.
        var calendarSerializer = new CalendarSerializer();
        var calendarAsIcs = calendarSerializer.SerializeToString(calendar);
        Assert.That(calendarAsIcs, Is.Not.Null);
        var calendarAsIcsTestable = ToTestableCalendarString(calendarAsIcs);
        Assert.That(calendarAsIcsTestable, Is.EqualTo(expectedCalendar));

        // Test Occurrence calculation.
        var occurrences = calendar.GetOccurrences();
        var occurrenceCount = Regex.Matches(expectedOccurrences, "DTSTART").Count;
        Assert.That(occurrences.Count(), Is.EqualTo(occurrenceCount));
        var occurrenceDates = ToTestablePeriodString(occurrences);
        Assert.That(occurrenceDates, Is.EqualTo(expectedOccurrences));
    }

    private static string ToTestableCalendarString(string calendarAsIcs)
        => calendarAsIcs
            .Split('\n')
            .Select(e => e.Replace("\r", ""))
            .Where(e => !e.StartsWith("PRODID"))
            .Where(e => !e.StartsWith("DTSTAMP"))
            .Where(e => !e.StartsWith("UID"))
            .Where(e => !e.StartsWith("SEQUENCE"))
            .Where(e => !e.StartsWith("VERSION"))
            .Aggregate(new StringBuilder(), (acc, e) => acc.AppendLine(e), e => e.ToString().TrimEnd());

    private static string ToTestablePeriodString(IEnumerable<Occurrence> occurrences)
        => occurrences
            .Select(e => GetPeriodString(e.Period))
            .OfType<string>()
            .Aggregate(new StringBuilder(), (acc, e) => acc.AppendLine(e), e => e.ToString().TrimEnd());

    private static string GetPeriodString(Period p)
    {
        var start = new CalendarProperty("DTSTART", p.StartTime);
        var end = new CalendarProperty("DTEND", p.EffectiveEndTime ?? p.StartTime.Add(p.Duration!.Value));
        var serializer = new PropertySerializer();

        return serializer.SerializeToString(end) + serializer.SerializeToString(start);
    }

    private const string TimeZone = "Europe/Zurich";
    private static readonly CalDateTime _start = new(2025, 07, 10, 09, 00, 00, TimeZone);
    private static CalendarEvent TestEvent1 => new()
    {
        Start = _start,
        End = _start.AddHours(1),
    };

    public static IEnumerable<TestCaseData> GetDailyTestData()
    {
        yield return new TestCaseData(
            TestEvent1.WithRecurrenceRule(
                new RecurrencePattern()
                {
                    Frequency = FrequencyType.Daily,
                    Interval = 2,
                    Count = 2,
                }
            )
            .ToCalendar(),
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=DAILY;INTERVAL=2;COUNT=2
            END:VEVENT
            END:VCALENDAR
            """,
            """
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000

            DTEND;TZID=Europe/Zurich:20250712T100000
            DTSTART;TZID=Europe/Zurich:20250712T090000
            """
            )
            .SetName("Daily: Every second day");

        yield return new TestCaseData(
            TestEvent1.WithRecurrenceRule(
                new RecurrencePattern()
                {
                    Frequency = FrequencyType.Daily,
                    ByDay = [new(DayOfWeek.Thursday), new(DayOfWeek.Monday)],
                    // 2025-07-17 09:00:00 @Europe/Zurich
                    Until = new CalDateTime(2025, 07, 17, 07, 00, 00, CalDateTime.UtcTzId),
                }
            )
            .ToCalendar(),
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=DAILY;UNTIL=20250717T070000Z;BYDAY=TH,MO
            END:VEVENT
            END:VCALENDAR
            """,
            """
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000

            DTEND;TZID=Europe/Zurich:20250714T100000
            DTSTART;TZID=Europe/Zurich:20250714T090000
            
            DTEND;TZID=Europe/Zurich:20250717T100000
            DTSTART;TZID=Europe/Zurich:20250717T090000
            """
            )
            .SetName("Daily: every Monday and Thursday");
    }
}
