//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// The class contains the tests for submitted issues from the GitHub repository,
/// slightly modified to fit the testing environment and the current version of the library.
/// </summary>
[TestFixture]
public class RecurrenceWithExDateTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void ShouldNotOccurOnLocalExceptionDate(bool useExDateWithTime)
    {
        // Arrange
        var id = Guid.NewGuid();
        const string timeZoneId = "Europe/London"; // IANA Time Zone ID
        var start = new CalDateTime(2024, 10, 19, 18, 0, 0, timeZoneId);
        var end = new CalDateTime(2024, 10, 19, 19, 0, 0, timeZoneId);

        var exceptionDate = useExDateWithTime
            ? new CalDateTime(2024, 10, 19, 21, 0, 0, timeZoneId)
            : new CalDateTime(2024, 10, 19);

        var recurrencePattern = new RecurrencePattern(FrequencyType.Hourly)
        {
            Count = 2,
            Interval = 3
        };

        var recurringEvent = new CalendarEvent
        {
            Summary = "My Recurring Event",
            Uid = id.ToString(),
            Start = start,
            End = end
        };
        recurringEvent.RecurrenceRules.Add(recurrencePattern);
        recurringEvent.ExceptionDates.Add(exceptionDate);

        var calendar = new Calendar();
        calendar.Events.Add(recurringEvent);

        // Act
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(calendar)!;

        var deserializedCalendar = Calendar.Load(ics)!;
        var occurrences = deserializedCalendar.GetOccurrences<CalendarEvent>(DateUtil.GetZone(timeZoneId)).ToList();

        Assert.Multiple(() =>
        {
            if (useExDateWithTime)
            {
                Assert.That(occurrences.Single().Period, Is.EqualTo((start.ToZonedDateTime(timeZoneId), end.ToZonedDateTime(timeZoneId))));
                Assert.That(ics, Does.Contain("EXDATE;TZID=Europe/London:20241019T210000"));
            }
            else
            {
                Assert.That(occurrences, Has.Count.EqualTo(0));
                Assert.That(ics, Does.Contain("EXDATE;VALUE=DATE:20241019"));
            }
        });
    }

    [Test]
    public void ShouldNotOccurOnUtcExceptionDate()
    {
        // Using Windows Time Zone ID
        var ics = """
                  BEGIN:VCALENDAR
                  PRODID:-//github.com/ical-org/ical.net//NONSGML ical.net 4.0//EN
                  VERSION:2.0
                  BEGIN:VEVENT
                  DTEND;TZID=GMT Standard Time:20241019T190000
                  DTSTAMP:20241018T083839Z
                  DTSTART;TZID=GMT Standard Time:20241019T180000
                  EXDATE:20241019T190000Z
                  RRULE:FREQ=HOURLY;INTERVAL=1;COUNT=4
                  SEQUENCE:0
                  SUMMARY:My Recurring Event
                  UID:c9f3a28d-97d6-43f7-872e-6cd79f67093d
                  END:VEVENT
                  END:VCALENDAR
                  """;

        var cal = Calendar.Load(ics)!;
        var occurrences = cal.GetOccurrences<CalendarEvent>(DateUtil.GetZone("GMT")).ToList();

        var serializer = new CalendarSerializer();
        ics = serializer.SerializeToString(cal);
        // serialize and deserialize to ensure the exclusion dates de/serialized
        cal = Calendar.Load(new CalendarSerializer(cal).SerializeToString()!)!;

        // Start date: 2024-10-19 at 18:00 (GMT Standard Time)
        // Recurrence: Every hour, 4 occurrences
        // Occurrences:
        // 2024-10-19 18:00 (UTC Offset: +0100)
        // 2024-10-19 19:00 (UTC Offset: +0100)
        // 2024-10-19 21:00 (UTC Offset: +0100)
        // Excluded dates impact:
        // 2024-10-19 at 19:00 UTC (= 2024-10-19 20:00 in "GMT Standard Time")
        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count, Is.EqualTo(3));
            Assert.That(
                occurrences.All(
                    o => !cal
                        .Events[0]!
                        .ExceptionDates.GetAllDates()
                        .Any(ex => ex.ToInstant().Equals(o.Start.ToInstant()))), Is.True);
            Assert.That(ics, Does.Contain("EXDATE:20241019T190000Z"));
        });
    }

    [Test]
    public void MultipleExclusionDatesSameTimeZoneShouldBeExcluded()
    {
        var ics = """
                  BEGIN:VCALENDAR
                  VERSION:2.0
                  PRODID:-//github.com/ical-org/ical.net//NONSGML ical.net 4.0//EN
                  BEGIN:VEVENT
                  UID:uid2@example.com
                  DTSTAMP:20231021T162159Z
                  DTSTART;TZID=Europe/Berlin:20231025T090000
                  DTEND;TZID=Europe/Berlin:20231025T100000
                  RRULE:FREQ=WEEKLY;COUNT=10
                  EXDATE;TZID=Europe/Berlin:20231029T090000,20231105T090000,20231112T090000
                  SUMMARY:Weekly Meeting
                  END:VEVENT
                  END:VCALENDAR
                  """;

        var cal = Calendar.Load(ics)!;
        var occurrences = cal.GetOccurrences<CalendarEvent>(DateUtil.GetZone("Europe/Berlin")).ToList();

        var serializer = new CalendarSerializer();
        ics = serializer.SerializeToString(cal);
        // serialize and deserialize to ensure the exclusion dates de/serialized
        cal = Calendar.Load(new CalendarSerializer(cal).SerializeToString()!)!;

        // Occurrences:
        // 2023-10-25 09:00 (UTC Offset: +0200)
        // 2023-11-01 09:00 (UTC Offset: +0100)
        // 2023-11-08 09:00 (UTC Offset: +0100)
        // 2023-11-15 09:00 (UTC Offset: +0100)
        // 2023-11-22 09:00 (UTC Offset: +0100)
        // 2023-11-29 09:00 (UTC Offset: +0100)
        // 2023-12-06 09:00 (UTC Offset: +0100)
        // 2023-12-13 09:00 (UTC Offset: +0100)
        // 2023-12-20 09:00 (UTC Offset: +0100)
        // 2023-12-27 09:00 (UTC Offset: +0100)
        // Exclusion Dates impact:
        // 2023-10-29 09:00 (UTC Offset: +0200)
        // 2023-11-05 09:00 (UTC Offset: +0100)
        // 2023-11-12 09:00 (UTC Offset: +0100)
        // The recurrences are adjusted for the switch from Daylight Saving Time to
        // Standard Time, which occurs on October 29, 2023, in the Europe/Berlin time zone.

        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count, Is.EqualTo(10));
            Assert.That(cal.Events[0]!.ExceptionDates.GetAllDates().Count(), Is.EqualTo(3));
            Assert.That(
                occurrences.All(
                    o => !cal
                        .Events[0]!
                        .ExceptionDates.GetAllDates()
                        .Any(ex => ex.ToInstant().Equals(o.Start.ToInstant()))), Is.True);
            Assert.That(ics, Does.Contain("EXDATE;TZID=Europe/Berlin:20231029T090000,20231105T090000,20231112T090000"));
        });
    }

    [Test]
    public void MultipleExclusionDatesDifferentZoneShouldBeExcluded()
    {
        var ics = """
                  BEGIN:VCALENDAR
                  VERSION:2.0
                  PRODID:-//github.com/ical-org/ical.net//NONSGML ical.net 4.0//EN
                  BEGIN:VEVENT
                  UID:uid5@example.com
                  DTSTAMP:20231021T162159Z
                  DTSTART;TZID=America/New_York:20231025T090000
                  DTEND;TZID=America/New_York:20231025T100000
                  RRULE:FREQ=WEEKLY;COUNT=10
                  EXDATE;TZID=America/New_York:20231029T090000
                  EXDATE;TZID=Europe/London:20231101T130000
                  SUMMARY:Weekly Meeting
                  END:VEVENT
                  END:VCALENDAR
                  """;

        var cal = Calendar.Load(ics)!;
        // serialize and deserialize to ensure the exclusion dates de/serialized
        cal = Calendar.Load(new CalendarSerializer(cal).SerializeToString()!)!;
        var occurrences = cal.GetOccurrences<CalendarEvent>(DateUtil.GetZone("America/New_York")).ToList();

        // Occurrences:
        // October 25, 2023, 09:00 AM (EDT, UTC-4)
        // November 8, 2023, 09:00 AM (EST, UTC-5)
        // November 15, 2023, 09:00 AM (EST, UTC-5)
        // November 22, 2023, 09:00 AM (EST, UTC-5)
        // November 29, 2023, 09:00 AM (EST, UTC-5)
        // December 6, 2023, 09:00 AM (EST, UTC-5)
        // December 13, 2023, 09:00 AM (EST, UTC-5)
        // December 20, 2023, 09:00 AM (EST, UTC-5)
        // December 27, 2023, 09:00 AM (EST, UTC-5)
        // Exclusion Dates Impact
        // October 29, 2023, 09:00 AM (America/New_York): Excluded
        // November 1, 2023, 01:00 PM (Europe/London): Excluded - November 1, 2023, 09:00 AM (EDT, UTC-4)

        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count, Is.EqualTo(9));
            Assert.That(cal.Events[0]!.ExceptionDates.GetAllDates().Count(), Is.EqualTo(2));
            Assert.That(
                occurrences.All(
                    o => !cal
                        .Events[0]!
                        .ExceptionDates.GetAllDates()
                        .Any(ex => ex.ToInstant().Equals(o.Start.ToInstant()))), Is.True);
        });
    }
}
