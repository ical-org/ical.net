// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
#nullable enable
using System;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Tests.TestHelpers;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// RFC errata 1913 (https://www.rfc-editor.org/errata_search.php?rfc=1913&amp;eid=1913):
/// 'The numeric value in a BYDAY rule part with the
/// FREQ rule part set to YEARLY corresponds  
/// to an offset within the month when the BYMONTH rule part is present,
/// and corresponds to an offset within the year when the
/// BYWEEKNO or BYMONTH rule parts are NOT present.'
/// <para/>
/// These tests verify the behavior with the interpretation that
/// <list type="bullet">
/// <item>when only BYMONTH is present, the numeric BYDAY offset applies within the month,</item>
/// <item>when BYWEEKNO is present, the numeric BYDAY offset applies within the week.</item>
/// </list>
/// <para/>
/// Disclaimer: Other iCalendar libraries may interpret this differently.
/// </summary>
[TestFixture]
public class RecurrenceYearlyWithOffsetsTests
{
    private void EventOccurrenceTest(
        Calendar cal,
        CalDateTime? fromDate,
        CalDateTime? toDate,
        Period[] expectedPeriods,
        string[]? timeZones,
        int eventIndex
    ) => OccurrenceTester.AssertOccurrences(cal, fromDate, toDate, expectedPeriods, timeZones, eventIndex);

    private void EventOccurrenceTest(
        Calendar cal,
        CalDateTime? fromDate,
        CalDateTime? toDate,
        Period[] expectedPeriods,
        string[]? timeZones
    ) => OccurrenceTester.AssertOccurrences(cal, fromDate, toDate, expectedPeriods, timeZones, 0);

    [Test, Category("Recurrence")]
    public void Yearly_ByMonth_With_NumericByDay_Offset_Produces_NthWeekdayPerMonth()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2026, 6, 1, 9, 0, 0),
            Duration = Duration.FromHours(1),
            // 2nd Monday of each BYMONTH
            RecurrenceRules = [new RecurrencePattern("FREQ=YEARLY;BYMONTH=6,9;BYDAY=2MO;COUNT=4")]
        };

        var cal = new Calendar();
        cal.Events.Add(evt);

        CalDateTime[] expected =
        [
            new(2026, 6, 8, 9, 0, 0),
            new(2026, 9, 14, 9, 0, 0),
            new(2027, 6, 14, 9, 0, 0),
            new(2027, 9, 13, 9, 0, 0)
        ];

        var expectedPeriods = expected
            .Select(dt => new Period(dt, Duration.FromHours(1)))
            .ToArray();

        EventOccurrenceTest(cal, new CalDateTime(2026, 1, 1), null, expectedPeriods, null);
    }

    [Test, Category("Recurrence")]
    public void Yearly_Without_ByMonth_NumericByDay_Interpreted_As_NthWeekdayOfYear()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2026, 1, 1, 9, 0, 0),
            Duration = Duration.FromHours(1),
            // 20th Monday of the YEAR
            RecurrenceRules = [new RecurrencePattern("FREQ=YEARLY;BYDAY=20MO;COUNT=3")]
        };

        var cal = new Calendar();
        cal.Events.Add(evt);

        CalDateTime[] expected =
        [
            new(2026, 5, 18, 9, 0, 0),
            new(2027, 5, 17, 9, 0, 0),
            new(2028, 5, 15, 9, 0, 0)
        ];

        var expectedPeriods = expected
            .Select(dt => new Period(dt, Duration.FromHours(1)))
            .ToArray();

        EventOccurrenceTest(cal, new CalDateTime(1997, 1, 1), null, expectedPeriods, null);
    }

    [Test, Category("Recurrence")]
    public void Yearly_ByMonth_With_NegativeNumericByDay_Returns_LastWeekdayOfEachMonth()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2026, 6, 1, 9, 0, 0),
            Duration = Duration.FromHours(1),
            // last Sunday of each BYMONTH
            RecurrenceRules = [new RecurrencePattern("FREQ=YEARLY;BYMONTH=6,9;BYDAY=-1SU;COUNT=2")]
        };

        var cal = new Calendar();
        cal.Events.Add(evt);

        CalDateTime[] expected =
        [
            new(2026, 6, 28, 9, 0, 0),
            new(2026, 9, 27, 9, 0, 0)
        ];

        var expectedPeriods = expected
            .Select(dt => new Period(dt, Duration.FromHours(1)))
            .ToArray();

        EventOccurrenceTest(cal, new CalDateTime(2026, 1, 1), null, expectedPeriods, null);
    }

    [Test, Category("Recurrence")]
    [TestCase("BYDAY=1MO", false)]
    [TestCase("BYDAY=6MO", true)] // 6th Monday in March doesn't exist, should throw
    public void Yearly_WithByMonth_ByDayOffsetIsWithinMonth(string byDay, bool shouldThrow)
    {
        // Rule: Yearly on the nth Monday (1MO or 6MO) of March.
        // The numeric offset (1) applies WITHIN the specified month (March).
        var start = new CalDateTime(2024, 1, 1, 9, 0, 0);

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRules = [new RecurrencePattern("FREQ=YEARLY;BYMONTH=3;" + byDay)]
        };

        var cal = new Calendar();
        cal.Events.Add(calendarEvent);

        var from = new CalDateTime(2024, 1, 1);
        var to = new CalDateTime(2027, 1, 1);

        if (shouldThrow)
        {
            Assert.That(() => cal.GetOccurrences(from).TakeWhileBefore(to).ToList(),
                Throws.TypeOf<EvaluationOutOfRangeException>(),
                "Invalid BYDAY offset within month.");
            return;
        }

        CalDateTime[] expected =
        [
            new (2024, 3, 4, 9, 0, 0), // 1st Monday of March 2024
            new (2025, 3, 3, 9, 0, 0), // 1st Monday of March 2025
            new (2026, 3, 2, 9, 0, 0)  // 1st Monday of March 2026
        ];

        var expectedPeriods = expected
            .Select(dt => new Period(dt, Duration.FromHours(1)))
            .ToArray();

        EventOccurrenceTest(cal, from, to, expectedPeriods, null);
    }

    [Test, Category("Recurrence")]
    [TestCase("BYDAY=1MO", false)]
    [TestCase("BYDAY=2MO", true)] // 2nd Monday in a week doesn't exist, should throw
    public void Yearly_WithByWeekNo_WithoutByMonth_ByDayOffsetIsWithinWeek(string byDay, bool shouldThrow)
    {
        // Rule: Yearly on the nth Monday (1MO or 2MO) of ISO week 10.
        // BYMONTH is NOT present. The numeric offset (2) applies WITHIN the specified week.
        var start = new CalDateTime(2024, 1, 1, 9, 0, 0);

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRules = [new RecurrencePattern("FREQ=YEARLY;BYWEEKNO=10;" + byDay)]
        };

        var cal = new Calendar();
        cal.Events.Add(calendarEvent);

        var from = new CalDateTime(2024, 1, 1);
        var to = new CalDateTime(2027, 1, 1);

        if (shouldThrow)
        {
            Assert.That(() => cal.GetOccurrences(from).TakeWhileBefore(to).ToList(),
                Throws.TypeOf<EvaluationOutOfRangeException>(),
                "Invalid BYDAY offset within week.");
            return;
        }

        var occurrences = cal.GetOccurrences(from).TakeWhileBefore(to).ToList();

        CalDateTime[] expected =
        [
            new (2024, 3, 4, 9, 0, 0), // Monday of week 10, 2024
            new (2025, 3, 3, 9, 0, 0), // Monday of week 10, 2025
            new (2026, 3, 2, 9, 0, 0)  // Monday of week 10, 2026
        ];

        Assert.That(occurrences.Select(o => o.Period.StartTime), Is.EquivalentTo(expected));
    }

    [Test, Category("Recurrence")]
    public void Yearly_WithoutByMonthOrByWeekNo_ByDayOffsetIsWithinYear()
    {
        // Rule: Yearly on the 15th Monday of the year.
        // Neither BYMONTH nor BYWEEKNO is present.
        // The numeric offset (15) applies WITHIN the entire year.
        var start = new CalDateTime(2024, 1, 1, 9, 0, 0);

        var calendarEvent = new CalendarEvent
        {
            DtStart = start,
            DtEnd = start.AddHours(1),
            RecurrenceRules = [new RecurrencePattern("FREQ=YEARLY;BYDAY=15MO")]
        };

        var cal = new Calendar();
        cal.Events.Add(calendarEvent);

        var from = new CalDateTime(2024, 1, 1);
        var to = new CalDateTime(2027, 1, 1);
        var occurrences = cal.GetOccurrences(from).TakeWhileBefore(to).ToList();

        CalDateTime[] expected =
        [
            new(2024, 4, 8, 9, 0, 0),  // 15th Monday of 2024
            new(2025, 4, 14, 9, 0, 0), // 15th Monday of 2025
            new (2026, 4, 13, 9, 0, 0) // 15th Monday of 2026
        ];

        Assert.That(occurrences.Select(o => o.Period.StartTime), Is.EquivalentTo(expected));
    }

    [Test, Category("Recurrence")]
    public void ByMonthDay_With_ByDay_SimpleCase()
    {
        const string tzId = "Europe/Berlin";
        const string ics = """
                   BEGIN:VCALENDAR
                   VERSION:2.0
                   BEGIN:VEVENT
                   DTSTART;TZID=Europe/Berlin:20250913T090000
                   DURATION:PT1H
                   RRULE:FREQ=MONTHLY;BYMONTHDAY=13;BYDAY=MO
                   END:VEVENT
                   END:VCALENDAR
                   """;

        var cal = Calendar.Load(ics)!;
        var from = new CalDateTime(2025, 1, 1);
        var to = new CalDateTime(2028, 1, 1);

        // Expected occurrences: only months, where the 13th is a Monday
        var expected = new[]
        {
            new Period(new CalDateTime(2025, 10, 13, 9, 0, 0, tzId), Duration.FromHours(1)),
            new Period(new CalDateTime(2026, 4, 13, 9, 0, 0, tzId), Duration.FromHours(1)),
            new Period(new CalDateTime(2026, 7, 13, 9, 0, 0, tzId), Duration.FromHours(1)),
            new Period(new CalDateTime(2027, 9, 13, 9, 0, 0, tzId), Duration.FromHours(1)),
            new Period(new CalDateTime(2027, 12, 13, 9, 0, 0, tzId), Duration.FromHours(1)),
        };

        EventOccurrenceTest(cal, from, to, expected, null);
    }

    [Test, Category("Recurrence")]
    public void ByMonthDay_With_ByDay_YearlyByMonthDay_ExpandMatrix_Note2()
    {
        const string ics = """
                    BEGIN:VCALENDAR
                    VERSION:2.0
                    BEGIN:VEVENT
                    DTSTART:20260601
                    RRULE:FREQ=YEARLY;BYMONTHDAY=1,8;BYDAY=22MO,23TU,25MO,36TU;COUNT=4
                    END:VEVENT
                    END:VCALENDAR
                    """;

        var cal = Calendar.Load(ics)!;
        Period[] expected =
        [
            new (new CalDateTime(2026, 6, 1), Duration.FromDays(1)),
            new (new CalDateTime(2027, 6, 8), Duration.FromDays(1)),
            new (new CalDateTime(2032, 6, 8), Duration.FromDays(1))
        ];

        EventOccurrenceTest(cal, new CalDateTime(2026, 1, 1), new CalDateTime(2035, 1, 1), expected, null);
    }

    [Test, Category("Recurrence")]
    public void ByMonthDay_With_ByDay_MonthlyByMonthDay_WithOffsets_LimitingBehavior()
    {
        const string ics = """
                    BEGIN:VCALENDAR
                    VERSION:2.0
                    BEGIN:VEVENT
                    DTSTART:20260601
                    RRULE:FREQ=MONTHLY;BYMONTHDAY=1,8;BYDAY=1MO,2TU;COUNT=3
                    END:VEVENT
                    END:VCALENDAR
                    """;

        var cal = Calendar.Load(ics)!;
        Period[] expected =
        [
            new (new CalDateTime(2026, 6, 1), Duration.FromDays(1)),
            new (new CalDateTime(2026, 9, 8), Duration.FromDays(1)),
            new (new CalDateTime(2026, 12, 8), Duration.FromDays(1))
        ];

        EventOccurrenceTest(cal, new CalDateTime(2026, 1, 1), new CalDateTime(2027, 1, 1), expected, null);
    }

    [Test, Category("Recurrence")]
    public void ByMonthDay_With_ByDay_YearlyByMonthAndMonthDay_WithOffsets()
    {
        const string ics = """
                    BEGIN:VCALENDAR
                    VERSION:2.0
                    BEGIN:VEVENT
                    DTSTART:20260601
                    RRULE:FREQ=YEARLY;BYMONTH=6,7,8,9,10,11,12;BYMONTHDAY=1,8;BYDAY=1MO,2TU;COUNT=3
                    END:VEVENT
                    END:VCALENDAR
                    """;

        var cal = Calendar.Load(ics)!;
        Period[] expected =
        [
            new (new CalDateTime(2026, 6, 1), Duration.FromDays(1)),
            new (new CalDateTime(2026, 9, 8), Duration.FromDays(1)),
            new (new CalDateTime(2026, 12, 8), Duration.FromDays(1))
        ];

        EventOccurrenceTest(cal, new CalDateTime(2026, 1, 1), new CalDateTime(2027, 1, 1), expected, null);
    }

    [Test, Category("Recurrence")]
    public void ByMonthDay_With_ByDay_YearlyByYearDay_WithOffsets_LimitingBehavior()
    {
        const string ics = """
                    BEGIN:VCALENDAR
                    VERSION:2.0
                    BEGIN:VEVENT
                    DTSTART:20260601
                    RRULE:FREQ=YEARLY;BYYEARDAY=152,173,251,272,321,342;BYDAY=22MO,26MO,36TU,37TU,49TU;COUNT=3
                    END:VEVENT
                    END:VCALENDAR
                    """;

        var cal = Calendar.Load(ics)!;
        Period[] expected =
        [
            new (new CalDateTime(2026, 6, 1), Duration.FromDays(1)),
            new (new CalDateTime(2026, 9, 8), Duration.FromDays(1)),
            new (new CalDateTime(2026, 12, 8), Duration.FromDays(1))
        ];

        EventOccurrenceTest(cal, new CalDateTime(2026, 1, 1), new CalDateTime(2030, 1, 1), expected, null);
    }
}
