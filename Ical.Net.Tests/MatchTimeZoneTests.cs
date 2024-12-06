//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class MatchTimeZoneTests
{
    [Test, Category("Recurrence")]
    public void MatchTimeZone_LocalTimeUsaWithTimeZone()
    {
        // DTSTART with local time and time zone reference (negative offset), UNTIL as UTC
        const string ical =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//Example Corp//NONSGML Event//EN
            BEGIN:VEVENT
            UID:example1
            SUMMARY:Event with local time and time zone
            DTSTART;TZID=America/New_York:20231101T090000
            RRULE:FREQ=DAILY;UNTIL=20231105T130000Z
            DTEND;TZID=America/New_York:20231101T100000
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical);
        var evt = calendar.Events.First();
        var until = evt.RecurrenceRules.First().Until;

        var expectedUntil = new DateTime(2023, 11, 05, 13, 00, 00, DateTimeKind.Utc);
        var occurrences = evt.GetOccurrences(new CalDateTime(2023, 11, 01), new CalDateTime(2023, 11, 06));

        Assert.Multiple(() =>
        {
            Assert.That(until, Is.EqualTo(expectedUntil));
            Assert.That(occurrences.Count, Is.EqualTo(4));
            /*
               Should have 4 occurrences:
               November 1, 2023: 09:00 AM - 10:00 AM (UTC-0400) (America/New_York)
               November 2, 2023: 09:00 AM - 10:00 AM (UTC-0400) (America/New_York)
               November 3, 2023: 09:00 AM - 10:00 AM (UTC-0400) (America/New_York)
               November 4, 2023: 09:00 AM - 10:00 AM (UTC-0400) (America/New_York)

               November 5, 2023: 09:00 AM - 10:00 AM (UTC-0500) (America/New_York)
               must NOT be included, because 20231105T130000Z => November 5, 2023: 08:00 AM (America/New_York)
               (Daylight Saving Time in America/New_York ended on Sunday, November 5, 2023, at 2:00 AM)
           */
        });
    }

    [Test, Category("Recurrence")]
    public void MatchTimeZone_LocalTimeJapanWithTimeZone()
    {
        // DTSTART with local time and time zone reference (positive offset), UNTIL as UTC
        const string ical =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//Example Corp//NONSGML Event//EN
            BEGIN:VEVENT
            UID:example1
            SUMMARY:Event with local time and time zone
            DTSTART;TZID=Asia/Tokyo:20231101T090000
            RRULE:FREQ=DAILY;UNTIL=20231105T130000Z
            DTEND;TZID=Asia/Tokyo:20231101T100000
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical);
        var evt = calendar.Events.First();
        var until = evt.RecurrenceRules.First().Until;

        var expectedUntil = new DateTime(2023, 11, 05, 13, 00, 00, DateTimeKind.Utc);
        var occurrences = evt.GetOccurrences(new CalDateTime(2023, 11, 01), new CalDateTime(2023, 11, 06));

        Assert.Multiple(() =>
        {
            Assert.That(until, Is.EqualTo(expectedUntil));
            Assert.That(occurrences.Count, Is.EqualTo(5));
            /*
               Should have 5 occurrences:
               November 1, 2023: 09:00 AM - 10:00 AM (UTC+0900) (Asia/Tokyo)
               November 2, 2023: 09:00 AM - 10:00 AM (UTC+0900) (Asia/Tokyo)
               November 3, 2023: 09:00 AM - 10:00 AM (UTC+0900) (Asia/Tokyo)
               November 4, 2023: 09:00 AM - 10:00 AM (UTC+0900) (Asia/Tokyo)
               November 5, 2023: 09:00 AM - 10:00 AM (UTC+0900) (Asia/Tokyo)
             */
        });
    }

    [Test, Category("Recurrence")]
    public void MatchTimeZone_UTCTime()
    {
        // DTSTART and UNTIL with UTC time
        const string ical =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//Example Corp//NONSGML Event//EN
            BEGIN:VEVENT
            UID:example2
            SUMMARY:Event with UTC time
            DTSTART:20231101T090000Z
            RRULE:FREQ=DAILY;UNTIL=20231105T090000Z
            DTEND:20231101T100000Z
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical);
        var evt = calendar.Events.First();
        var until = evt.RecurrenceRules.First().Until;

        var expectedUntil = new DateTime(2023, 11, 05, 09, 00, 00, DateTimeKind.Utc);
        var occurrences = evt.GetOccurrences(new CalDateTime(2023, 11, 01), new CalDateTime(2023, 11, 06));
        
        Assert.Multiple(() =>
        {
            Assert.That(until, Is.EqualTo(expectedUntil));
            Assert.That(occurrences.Count, Is.EqualTo(5));
        });
    }

    [Test, Category("Recurrence")]
    public void MatchTimeZone_FloatingTime()
    {
        // DTSTART AND UNTIL with floating time
        const string ical =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//Example Corp//NONSGML Event//EN
            BEGIN:VEVENT
            UID:example3
            SUMMARY:Event with floating time
            DTSTART:20231101T090000
            RRULE:FREQ=DAILY;UNTIL=20231105T090000
            DTEND:20231101T100000
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical);
        var evt = calendar.Events.First();
        var until = evt.RecurrenceRules.First().Until;

        var expectedUntil = new DateTime(2023, 11, 05, 09, 00, 00, DateTimeKind.Unspecified);
        var occurrences = evt.GetOccurrences(new CalDateTime(2023, 11, 01), new CalDateTime(2023, 11, 06));

        Assert.Multiple(() =>
        {
            Assert.That(until, Is.EqualTo(expectedUntil));
            Assert.That(occurrences.Count, Is.EqualTo(5));
        });

    }

    [Test, Category("Recurrence")]
    public void MatchTimeZone_LocalTimeNoTimeZone()
    {
        // DTSTART with local time and no time zone reference
        const string ical =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//Example Corp//NONSGML Event//EN
            BEGIN:VEVENT
            UID:example4
            SUMMARY:Event with local time and no time zone reference
            DTSTART:20231101T090000
            RRULE:FREQ=DAILY;UNTIL=20231105T090000
            DTEND:20231101T100000
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical);
        var evt = calendar.Events.First();
        var until = evt.RecurrenceRules.First().Until;

        var expectedUntil = new DateTime(2023, 11, 05, 09, 00, 00, DateTimeKind.Unspecified);
        var occurrences = evt.GetOccurrences(new CalDateTime(2023, 11, 01), new CalDateTime(2023, 11, 06));
        
        Assert.Multiple(() =>
        {
            Assert.That(until, Is.EqualTo(expectedUntil));
            Assert.That(occurrences.Count, Is.EqualTo(5));
        });
    }

    [Test, Category("Recurrence")]
    public void MatchTimeZone_DateOnly()
    {
        // DTSTART with date-only value
        const string ical =
            """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//Example Corp//NONSGML Event//EN
            BEGIN:VEVENT
            UID:example5
            SUMMARY:Event with date-only value
            DTSTART;VALUE=DATE:20231101
            RRULE:FREQ=DAILY;UNTIL=20231105
            DTEND;VALUE=DATE:20231102
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical);
        var evt = calendar.Events.First();
        var until = evt.RecurrenceRules.First().Until;

        var expectedUntil = new DateTime(2023, 11, 05, 00, 00, 00, DateTimeKind.Unspecified);
        var occurrences = evt.GetOccurrences(new CalDateTime(2023, 11, 01), new CalDateTime(2023, 11, 06));

        Assert.Multiple(() =>
        {
            Assert.That(until, Is.EqualTo(expectedUntil));
            Assert.That(occurrences.Count, Is.EqualTo(5));
        });
    }
}
