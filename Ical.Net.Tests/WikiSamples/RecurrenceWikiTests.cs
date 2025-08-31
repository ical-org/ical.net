//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests.WikiSamples;

[TestFixture, Category("Recurrence")]
public class RecurrenceWikiTests
{
    [Test]
    public void Introduction()
    {
        var recurrence = new RecurrencePattern()
        {
            Frequency = FrequencyType.Daily,
            Interval = 2,
            Count = 30
            // Add other parameters like ByDay, ByMonth, etc.
        };

        var calendarEvent = new CalendarEvent()
        {
            DtStart = new CalDateTime(2025, 07, 10),
            DtEnd = new CalDateTime(2025, 07, 11),
            // Add the rule to the event.
            RecurrenceRules = [recurrence]
        };

        // Get all occurrences of the series.
        IEnumerable<Occurrence> allOccurrences = calendarEvent.GetOccurrences();
        Assert.That(allOccurrences.Count(), Is.EqualTo(30));

        // Get the occurrences in July.
        IEnumerable<Occurrence> julyOccurrences = calendarEvent
            .GetOccurrences(new CalDateTime(2025, 07, 01))
            .TakeWhileBefore(new CalDateTime(2025, 08, 01));
        Assert.That(julyOccurrences.Count(), Is.EqualTo(11));
    }

    [Test]
    public void DailyIntervalCount()
    {
        // Pattern: Daily every second day, tow times.

        // Create the CalendarEvent
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
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();
        Assert.That(occurrences.Count(), Is.EqualTo(2));

        // Calendar output
        var expectedIcsCalendar = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=DAILY;INTERVAL=2;COUNT=2
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurring dates
        var expectedOccurrenceDates = """
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            
            DTEND;TZID=Europe/Zurich:20250712T100000
            DTSTART;TZID=Europe/Zurich:20250712T090000
            """;

        // Asserts
        Assert.That(calendarAsIcs, Is.Not.Null);
        var calendarTestString = ToTestableCalendarString(calendarAsIcs);
        Assert.That(calendarTestString, Is.EqualTo(expectedIcsCalendar));

        var occurrenceTestString = ToTestablePeriodString(occurrences);
        Assert.That(occurrenceTestString, Is.EqualTo(expectedOccurrenceDates));
    }

    [Test]
    public void YearlyByMonthDayUntil()
    {
        // Pattern: Yearly at 10 and 12th of July, until +2 years.
        // Note: TimeZone of the until-date.
        // Note: Inclusive manner of the until-date.

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern()
        {
            Frequency = FrequencyType.Yearly,
            ByMonthDay = [10, 12],
            // 2027-07-10 09:00:00 Europe/Zurich (07:00:00 UTC)
            Until = start.AddYears(2).ToTimeZone("UTC")
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

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Calendar output
        var expectedIcsCalendar = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=YEARLY;UNTIL=20270710T070000Z;BYMONTHDAY=10,12
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurring dates
        var expectedOccurrenceDates = """
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            
            DTEND;TZID=Europe/Zurich:20250712T100000
            DTSTART;TZID=Europe/Zurich:20250712T090000

            DTEND;TZID=Europe/Zurich:20260710T100000
            DTSTART;TZID=Europe/Zurich:20260710T090000
            
            DTEND;TZID=Europe/Zurich:20260712T100000
            DTSTART;TZID=Europe/Zurich:20260712T090000

            DTEND;TZID=Europe/Zurich:20270710T100000
            DTSTART;TZID=Europe/Zurich:20270710T090000
            """;

        // Asserts
        Assert.That(calendarAsIcs, Is.Not.Null);
        var calendarTestString = ToTestableCalendarString(calendarAsIcs);
        Assert.That(calendarTestString, Is.EqualTo(expectedIcsCalendar));

        var occurrenceTestString = ToTestablePeriodString(occurrences);
        Assert.That(occurrenceTestString, Is.EqualTo(expectedOccurrenceDates));
    }

    [Test]
    public void MonthlyByDayCountRDate()
    {
        // Pattern: Monthly every last Sunday, for 3 times - plus July 10th.
        // Note: RDATE takes a List of PeriodList.

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 06, 29, 16, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern()
        {
            Frequency = FrequencyType.Monthly,
            ByDay = [new(DayOfWeek.Sunday, FrequencyOccurrence.Last)],
            Count = 3,
        };

        // Create additional occurrence.
        PeriodList periodList = new PeriodList();
        periodList.Add(new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich"));

        var calendarEvent = new CalendarEvent()
        {
            DtStart = start,
            DtEnd = start.AddHours(4),
            RecurrenceRules = [recurrence],
            // Add the additional occurrence to the series.
            RecurrenceDatesPeriodLists = [periodList]
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var calendarAsIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Calendar output
        var expectedIcsCalendar = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250629T200000
            DTSTART;TZID=Europe/Zurich:20250629T160000
            RDATE;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=MONTHLY;COUNT=3;BYDAY=-1SU
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurring dates
        var expectedOccurrenceDates = """
            DTEND;TZID=Europe/Zurich:20250629T200000
            DTSTART;TZID=Europe/Zurich:20250629T160000
            
            DTEND;TZID=Europe/Zurich:20250710T130000
            DTSTART;TZID=Europe/Zurich:20250710T090000

            DTEND;TZID=Europe/Zurich:20250727T200000
            DTSTART;TZID=Europe/Zurich:20250727T160000
            
            DTEND;TZID=Europe/Zurich:20250831T200000
            DTSTART;TZID=Europe/Zurich:20250831T160000
            """;

        // Asserts
        Assert.That(calendarAsIcs, Is.Not.Null);
        var calendarTestString = ToTestableCalendarString(calendarAsIcs);
        Assert.That(calendarTestString, Is.EqualTo(expectedIcsCalendar));

        var occurrenceTestString = ToTestablePeriodString(occurrences);
        Assert.That(occurrenceTestString, Is.EqualTo(expectedOccurrenceDates));
    }

    [Test]
    public void HourlyUntilExDate()
    {
        // Pattern: Hourly every hour, until midnight (inclusive) - except 22:00.
        // Note: EXDATE takes a List of PeriodList.

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 20, 00, 00, "UTC");
        var recurrence = new RecurrencePattern()
        {
            Frequency = FrequencyType.Hourly,
            Until = start.AddHours(4)
        };

        // Create exception for an occurrence.
        PeriodList periodList = new PeriodList();
        periodList.Add(new CalDateTime(2025, 07, 10, 22, 00, 00, "UTC"));

        var calendarEvent = new CalendarEvent()
        {
            DtStart = start,
            DtEnd = start.AddMinutes(15),
            RecurrenceRules = [recurrence],
            // Add the exception date(s) to the series.
            ExceptionDatesPeriodLists = [periodList]
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var calendarAsIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Calendar output
        var expectedIcsCalendar = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND:20250710T201500Z
            DTSTART:20250710T200000Z
            EXDATE:20250710T220000Z
            RRULE:FREQ=HOURLY;UNTIL=20250711T000000Z
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurring dates
        var expectedOccurrenceDates = """
            DTEND:20250710T201500Z
            DTSTART:20250710T200000Z

            DTEND:20250710T211500Z
            DTSTART:20250710T210000Z

            DTEND:20250710T231500Z
            DTSTART:20250710T230000Z
            
            DTEND:20250711T001500Z
            DTSTART:20250711T000000Z
            """;

        // Asserts
        Assert.That(calendarAsIcs, Is.Not.Null);
        var calendarTestString = ToTestableCalendarString(calendarAsIcs);
        Assert.That(calendarTestString, Is.EqualTo(expectedIcsCalendar));

        var occurrenceTestString = ToTestablePeriodString(occurrences);
        Assert.That(occurrenceTestString, Is.EqualTo(expectedOccurrenceDates));
    }

    [Test]
    public void DailyIntervalCountMoved()
    {
        // Pattern: Daily every second day, four times - third is moved.
        // Note: Link moved events with series-master by same UID.

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern()
        {
            Frequency = FrequencyType.Daily,
            Interval = 2,
            Count = 4
        };

        var calendarEvent = new CalendarEvent()
        {
            // UID links master with child.
            Uid = "my-custom-id",
            Summary = "Walking",
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRules = [recurrence],
        };

        var startMoved = new CalDateTime(2025, 07, 13, 13, 00, 00, "Europe/Zurich");
        var movedEvent = new CalendarEvent()
        {
            // UID links master with child.
            Uid = "my-custom-id",
            // Overwrite properties of the original occurrence.
            Summary = "Short after lunch walk",
            // Set new start and end time.
            DtStart = startMoved,
            DtEnd = startMoved.AddMinutes(13),
            // Set the original date of the occurrence (2025-07-14 09:00:00).
            RecurrenceId = start.AddDays(4)
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);
        calendar.Events.Add(movedEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var calendarAsIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Calendar output
        var expectedIcsCalendar = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=DAILY;INTERVAL=2;COUNT=4
            SUMMARY:Walking
            UID:my-custom-id
            END:VEVENT
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250713T131300
            DTSTART;TZID=Europe/Zurich:20250713T130000
            RECURRENCE-ID;TZID=Europe/Zurich:20250714T090000
            SUMMARY:Short after lunch walk
            UID:my-custom-id
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurring dates
        var expectedOccurrenceDates = """
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            
            DTEND;TZID=Europe/Zurich:20250712T100000
            DTSTART;TZID=Europe/Zurich:20250712T090000

            DTEND;TZID=Europe/Zurich:20250713T131300
            DTSTART;TZID=Europe/Zurich:20250713T130000
            
            DTEND;TZID=Europe/Zurich:20250716T100000
            DTSTART;TZID=Europe/Zurich:20250716T090000
            """;

        // Asserts
        Assert.That(calendarAsIcs, Is.Not.Null);
        var calendarTestString = ToTestableCalendarString(calendarAsIcs, allowUid: true);
        Assert.That(calendarTestString, Is.EqualTo(expectedIcsCalendar));

        var occurrenceTestString = ToTestablePeriodString(occurrences);
        Assert.That(occurrenceTestString, Is.EqualTo(expectedOccurrenceDates));
    }

    private static string ToTestableCalendarString(string calendarAsIcs, bool allowUid = false)
        => calendarAsIcs
            .Split('\n')
            .Select(e => e.Replace("\r", ""))
            .Where(e => !e.StartsWith("PRODID"))
            .Where(e => !e.StartsWith("DTSTAMP"))
            .Where(e => allowUid || !e.StartsWith("UID"))
            .Where(e => !e.StartsWith("SEQUENCE"))
            .Where(e => !e.StartsWith("VERSION"))
            .Aggregate(new StringBuilder(), (acc, e) => acc.AppendLine(e), e => e.ToString().TrimEnd());

    private static string ToTestablePeriodString(IEnumerable<Occurrence> occurrences)
        => occurrences
            .Select(e => GetPeriodString(e.Period))
            .Aggregate(new StringBuilder(), (acc, e) => acc.AppendLine(e), e => e.ToString().TrimEnd());

    private static string GetPeriodString(Period p)
    {
        var start = new CalendarProperty("DTSTART", p.StartTime);
        var end = new CalendarProperty("DTEND", p.EffectiveEndTime ?? p.StartTime.Add(p.Duration!.Value));
        var serializer = new PropertySerializer();

        var endSerialized = serializer.SerializeToString(end)?.TrimEnd('\n').TrimEnd('\r');
        var startSerialized = serializer.SerializeToString(start)?.TrimEnd('\n').TrimEnd('\r');

        return $"{endSerialized}{Environment.NewLine}{startSerialized}{Environment.NewLine}";
    }
}
