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
using Ical.Net.Tests.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Ical.Net.Tests.WikiSamples;
#pragma warning disable IDE0007

[TestFixture, Category("Wiki")]
internal class RecurrenceWikiTests
{
    private TestLoggingManager _loggingManager = null!;
    private ILoggerFactory _loggerFactory = null!;
    private ILogger _logger;

    [OneTimeSetUp]
    public void Setup()
    {
        // Enable logging of occurrences for test output comparison.
        _loggingManager = new TestLoggingManager(
            new Options
            {
                DebugModeOnly = false,
                MinLogLevel = LogLevel.Debug,
                Filters = [new Filter(LogLevel.Debug, GetType().FullName!)],
                OutputTemplate = "{Message:lj}{NewLine}---{NewLine}",
                // Output for easy copy-paste to wiki.
                LogToConsole = true
            });

        _loggerFactory = _loggingManager.TestFactory;
        _logger = _loggerFactory.CreateLogger<RecurrenceWikiTests>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _loggingManager.Dispose();
        _loggerFactory.Dispose();
    }

    [Test]
    public void Introduction()
    {
        // Wiki code start

        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Daily,
            Interval = 2,
            Count = 5
            // Add other properties like ByDay, ByMonth, etc.
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = new CalDateTime(2025, 07, 10),
            // Add the rule to the event.
            RecurrenceRule = recurrence
        };

        // Get all occurrences of the series.
        IEnumerable<Occurrence> allOccurrences = calendarEvent.GetOccurrences();
        Assert.That(allOccurrences.Count(), Is.EqualTo(5));

        // Wiki code end

        const string expectedOccurrences =
            """
            5 occurrences:
            Start: 07/10/2025
              Period: P1D
              End: 07/11/2025
            Start: 07/12/2025
              Period: P1D
              End: 07/13/2025
            Start: 07/14/2025
              Period: P1D
              End: 07/15/2025
            Start: 07/16/2025
              Period: P1D
              End: 07/17/2025
            Start: 07/18/2025
              Period: P1D
              End: 07/19/2025
            """;

        var generatedOccurrences = ToWikiPeriodString(allOccurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void DailyIntervalCount()
    {
        // Wiki code start

        // Pattern: Daily every second day, two times.

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Daily,
            Interval = 2,
            Count = 2
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRule = recurrence
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var generatedIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();
        Assert.That(occurrences.Count(), Is.EqualTo(2));

        // Wiki code end

        // Calendar output (irrelevant properties are excluded)
        const string expectedIcs =
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=DAILY;INTERVAL=2;COUNT=2
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurrences
        const string expectedOccurrences =
            """
            2 occurrences:
            Start: 07/10/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/10/2025 10:00:00 +02:00 Europe/Zurich
            Start: 07/12/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/12/2025 10:00:00 +02:00 Europe/Zurich
            """;
        
        // Non-Wiki Asserts
        Assert.That(RemoveIrrelevantProperties(generatedIcs), Is.EqualTo(expectedIcs));

        var generatedOccurrences = ToWikiPeriodString(occurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedIcs);
        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void YearlyByMonthDayUntil()
    {
        // Pattern: Yearly at 10 and 12th of July, until +2 years.
        // Note: TimeZone of the until-date.
        // Note: Inclusive manner of the until-date.

        // Wiki code start

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Yearly,
            ByMonthDay = [10, 12],
            // 2027-07-10 09:00:00 Europe/Zurich (07:00:00 UTC)
            Until = start.AddYears(2).ToTimeZone("UTC")
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRule = recurrence
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var generatedIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Wiki code end

        // Calendar output (irrelevant properties are excluded)
        const string expectedIcs =
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=YEARLY;UNTIL=20270710T070000Z;BYMONTHDAY=10,12
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurrences
        const string expectedOccurrences =
            """
            5 occurrences:
            Start: 07/10/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/10/2025 10:00:00 +02:00 Europe/Zurich
            Start: 07/12/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/12/2025 10:00:00 +02:00 Europe/Zurich
            Start: 07/10/2026 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/10/2026 10:00:00 +02:00 Europe/Zurich
            Start: 07/12/2026 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/12/2026 10:00:00 +02:00 Europe/Zurich
            Start: 07/10/2027 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/10/2027 10:00:00 +02:00 Europe/Zurich
            """;

        // Non-Wiki Asserts

        var calendarTestString = RemoveIrrelevantProperties(generatedIcs!);
        Assert.That(RemoveIrrelevantProperties(calendarTestString), Is.EqualTo(expectedIcs));

        var generatedOccurrences = ToWikiPeriodString(occurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedIcs);
        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void MonthlyByDayCountRDate()
    {
        // Pattern: Monthly every last Sunday, for 3 times - plus July 10th.

        // Wiki code start

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 06, 29, 16, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Monthly,
            ByDay = [new(DayOfWeek.Sunday, FrequencyOccurrence.Last)],
            Count = 3
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddHours(4),
            RecurrenceRule = recurrence,
        };
        // Add additional an occurrence to the series.
        calendarEvent.RecurrenceDates
            .Add(new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich"));

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var generatedIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Wiki code end

        // Calendar output (irrelevant properties are excluded)
        const string expectedIcs =
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250629T200000
            DTSTART;TZID=Europe/Zurich:20250629T160000
            RDATE;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=MONTHLY;COUNT=3;BYDAY=-1SU
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurrences
        const string expectedOccurrences =
            """
            4 occurrences:
            Start: 06/29/2025 16:00:00 +02:00 Europe/Zurich
              Period: PT4H
              End: 06/29/2025 20:00:00 +02:00 Europe/Zurich
            Start: 07/10/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT4H
              End: 07/10/2025 13:00:00 +02:00 Europe/Zurich
            Start: 07/27/2025 16:00:00 +02:00 Europe/Zurich
              Period: PT4H
              End: 07/27/2025 20:00:00 +02:00 Europe/Zurich
            Start: 08/31/2025 16:00:00 +02:00 Europe/Zurich
              Period: PT4H
              End: 08/31/2025 20:00:00 +02:00 Europe/Zurich
            """;

        // Non-Wiki Asserts
        Assert.That(RemoveIrrelevantProperties(generatedIcs!), Is.EqualTo(expectedIcs));

        var generatedOccurrences = ToWikiPeriodString(occurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedIcs);
        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void HourlyUntilExDate()
    {
        // Pattern: Hourly every hour, until midnight (inclusive) - except 22:00.

        // Wiki code start

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 20, 00, 00, "UTC");
        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Hourly,
            Until = start.AddHours(4)
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddMinutes(15),
            RecurrenceRule = recurrence,
        };
        // Add the exception date to the series.
        calendarEvent.ExceptionDates
            .Add(new CalDateTime(2025, 07, 10, 22, 00, 00, "UTC"));

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var generatedIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Wiki code end

        // Calendar output (irrelevant properties are excluded)
        const string expectedIcs =
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND:20250710T201500Z
            DTSTART:20250710T200000Z
            EXDATE:20250710T220000Z
            RRULE:FREQ=HOURLY;UNTIL=20250711T000000Z
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurrences
        const string expectedOccurrences =
            """
            4 occurrences:
            Start: 07/10/2025 20:00:00 +00:00 UTC
              Period: PT15M
              End: 07/10/2025 20:15:00 +00:00 UTC
            Start: 07/10/2025 21:00:00 +00:00 UTC
              Period: PT15M
              End: 07/10/2025 21:15:00 +00:00 UTC
            Start: 07/10/2025 23:00:00 +00:00 UTC
              Period: PT15M
              End: 07/10/2025 23:15:00 +00:00 UTC
            Start: 07/11/2025 00:00:00 +00:00 UTC
              Period: PT15M
              End: 07/11/2025 00:15:00 +00:00 UTC
            """;

        // Non-Wiki Asserts
        Assert.That(RemoveIrrelevantProperties(generatedIcs!), Is.EqualTo(expectedIcs));

        var generatedOccurrences = ToWikiPeriodString(occurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedOccurrences);
        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void DailyIntervalCountMoved()
    {
        // Pattern: Daily every second day, four times - third is moved.
        // Note: Link moved events with series-master by same UID.
        // Note: For chained events with RECURRENCE-ID, SEQUENCE should be set. 

        // Wiki code start

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Daily,
            Interval = 2,
            Count = 4
        };

        var calendarEvent = new CalendarEvent
        {
            // UID links master with child.
            Uid = "my-custom-id",
            Summary = "Walking",
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRule = recurrence,
            Sequence = 0 // default value
        };

        var startMoved = new CalDateTime(2025, 07, 13, 13, 00, 00, "Europe/Zurich");
        var movedEvent = new CalendarEvent
        {
            // UID links master with child.
            Uid = "my-custom-id",
            // Overwrite properties of the original occurrence.
            Summary = "Short after lunch walk",
            // Set new start and end time.
            DtStart = startMoved,
            DtEnd = startMoved.AddMinutes(13),
            // Set the original date of the occurrence (2025-07-14 09:00:00).
            RecurrenceId = start.AddDays(4),
            // The first change for this RecurrenceId
            Sequence = 1
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);
        calendar.Events.Add(movedEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var generatedIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Wiki code end

        // Calendar output (irrelevant properties are excluded)
        const string expectedIcs =
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250710T100000
            DTSTART;TZID=Europe/Zurich:20250710T090000
            RRULE:FREQ=DAILY;INTERVAL=2;COUNT=4
            SEQUENCE:0
            SUMMARY:Walking
            UID:my-custom-id
            END:VEVENT
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250713T131300
            DTSTART;TZID=Europe/Zurich:20250713T130000
            RECURRENCE-ID;TZID=Europe/Zurich:20250714T090000
            SEQUENCE:1
            SUMMARY:Short after lunch walk
            UID:my-custom-id
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurrences
        const string expectedOccurrences =
            """
            4 occurrences:
            Start: 07/10/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/10/2025 10:00:00 +02:00 Europe/Zurich
            Start: 07/12/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/12/2025 10:00:00 +02:00 Europe/Zurich
            Start: 07/13/2025 13:00:00 +02:00 Europe/Zurich
              Period: PT13M
              End: 07/13/2025 13:13:00 +02:00 Europe/Zurich
            Start: 07/16/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 07/16/2025 10:00:00 +02:00 Europe/Zurich
            """;
        
        // Non-Wiki Asserts
        Assert.That(RemoveIrrelevantProperties(generatedIcs!, ["UID", "SEQUENCE"]), Is.EqualTo(expectedIcs));

        var generatedOccurrences = ToWikiPeriodString(occurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedIcs);
        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void RecurrenceWithTimeZoneChanges()
    {
        // Pattern: Recurrence weekly on Mondays, three times - before, on, and after DST change.

        // Wiki code start

        // Create the CalendarEvent
        var start = new CalDateTime(2025, 03, 24, 09, 00, 00, "Europe/Zurich"); // Before DST starts
        var recurrence = new RecurrencePattern
        {
            Frequency = FrequencyType.Weekly,
            Count = 3 // Three Mondays: before, on, and after DST change
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRule = recurrence
        };

        // Add CalendarEvent to Calendar
        var calendar = new Calendar();
        calendar.Events.Add(calendarEvent);

        // Serialize Calendar to string
        var calendarSerializer = new CalendarSerializer();
        var generatedIcs = calendarSerializer.SerializeToString(calendar);

        // Calculate all occurrences
        IEnumerable<Occurrence> occurrences = calendar.GetOccurrences();

        // Wiki code end

        // Calendar output (irrelevant properties are excluded)
        const string expectedIcs =
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTEND;TZID=Europe/Zurich:20250324T100000
            DTSTART;TZID=Europe/Zurich:20250324T090000
            RRULE:FREQ=WEEKLY;COUNT=3
            END:VEVENT
            END:VCALENDAR
            """;
        // Occurrences
        const string expectedOccurrences =
            """
            3 occurrences:
            Start: 03/24/2025 09:00:00 +01:00 Europe/Zurich
              Period: PT1H
              End: 03/24/2025 10:00:00 +01:00 Europe/Zurich
            Start: 03/31/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 03/31/2025 10:00:00 +02:00 Europe/Zurich
            Start: 04/07/2025 09:00:00 +02:00 Europe/Zurich
              Period: PT1H
              End: 04/07/2025 10:00:00 +02:00 Europe/Zurich
            """;

        // Non-Wiki Asserts
        Assert.That(RemoveIrrelevantProperties(generatedIcs!), Is.EqualTo(expectedIcs));

        var generatedOccurrences = ToWikiPeriodString(occurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedIcs);
        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void GetFirstOccurrenceOfAllCalendarEvents()
    {
        // Wiki code start

        var calendar = new Calendar();
        var start = new CalDateTime(2025, 9, 1, 10, 0, 0, CalDateTime.UtcTzId);

        // Event that recurs daily
        calendar.Events.Add(new CalendarEvent
        {
            Summary = "Daily event",
            Start = start,
            End = start.AddHours(1),
            RecurrenceRule = new RecurrencePattern(FrequencyType.Daily, interval: 1)
        });

        // Simple event in far future
        calendar.Events.Add(new CalendarEvent
        {
            Summary = "Far future event",
            Start = start.AddYears(10),
            End = start.AddYears(10).AddHours(1)
        });
        
        var occurrences =
            calendar.Events
                .SelectMany(ev => ev.GetOccurrences().Take(1))
                .ToArray();

        // Wiki code end

        // Occurrences
        const string expectedOccurrences =
            """
            2 occurrences:
            Start: 09/01/2025 10:00:00 +00:00 UTC
              Period: PT1H
              End: 09/01/2025 11:00:00 +00:00 UTC
            Start: 09/01/2035 10:00:00 +00:00 UTC
              Period: PT1H
              End: 09/01/2035 11:00:00 +00:00 UTC
            """;

        // Non-Wiki Asserts

        var generatedOccurrences = ToWikiPeriodString(occurrences);
        Assert.That(generatedOccurrences, Is.EqualTo(expectedOccurrences));

        _logger.LogDebug(expectedOccurrences);
    }

    [Test]
    public void MoreRecurrenceRuleExamples()
    {
        // Every other Tuesday until the end of the year
        var rrule1 = new RecurrencePattern(FrequencyType.Weekly, 2)
        {
            Until = new CalDateTime(2026, 1, 1)
        };

        // The 2nd day of every month for 5 occurrences
        var rrule2 = new RecurrencePattern(FrequencyType.Monthly)
        {
            ByMonthDay = [2],  // Your day of the month goes here
            Count = 5
        };

        // The 4th Thursday of November every year
        var rrule3 = new RecurrencePattern(FrequencyType.Yearly, 1)
        {
            Frequency = FrequencyType.Yearly,
            Interval = 1,
            ByMonth = [11],
            ByDay = [new WeekDay { DayOfWeek = DayOfWeek.Thursday, Offset = 4 }],
        };

        // Every day in 2025, except Sundays
        var rrule4 = new RecurrencePattern(FrequencyType.Daily)
        {
            // Start: 2025-01-01, End: 2025-12-31
            Until = new CalDateTime(2025, 12, 31),
            // Exclude Sundays
            ByDay = [
                new WeekDay(DayOfWeek.Monday),
                new WeekDay(DayOfWeek.Tuesday),
                new WeekDay(DayOfWeek.Wednesday),
                new WeekDay(DayOfWeek.Thursday),
                new WeekDay(DayOfWeek.Friday),
                new WeekDay(DayOfWeek.Saturday)
            ]
        };

        Assert.That(() =>
        {
            _ = new CalendarEvent
            {
                ExceptionRules = [rrule1, rrule2, rrule3, rrule4]
            };
        }, Throws.Nothing);
    }

    private static string RemoveIrrelevantProperties(string generatedIcs, string[]? keep = null)
        => generatedIcs
            .Split('\n')
            .Select(e => e.Replace("\r", ""))
            .Where(e => !e.StartsWith("PRODID"))
            .Where(e => !e.StartsWith("VERSION"))
            .Where(e => !e.StartsWith("DTSTAMP"))
            .Where(e => !(e.StartsWith("UID") && keep?.Contains("UID") != true))
            .Where(e => !(e.StartsWith("SEQUENCE") && keep?.Contains("SEQUENCE") != true))
            .Aggregate(new StringBuilder(), (acc, e) => acc.AppendLine(e), e => e.ToString().TrimEnd());

    private static string ToWikiPeriodString(IEnumerable<Occurrence> occurrences)
        => occurrences.ToLog();
}
