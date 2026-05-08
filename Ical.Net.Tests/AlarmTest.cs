//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.IO;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class AlarmTests
{
    #region Examples from RFC 5545

    [Test]
    public void ExactTimeAlarmWithRepeat()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(1997, 3, 18)
        };

        var valarm = """
            BEGIN:VALARM
            TRIGGER;VALUE=DATE-TIME:19970317T133000Z
            REPEAT:4
            DURATION:PT15M
            ACTION:AUDIO
            ATTACH;FMTTYPE=audio/basic:ftp://example.com/pub/
                sounds/bell-01.aud
            END:VALARM
            """;

        var alarm = SimpleDeserializer.Default
            .Deserialize(new StringReader(valarm))
            .Cast<Alarm>()
            .Single();

        e.Alarms.Add(alarm);

        var results = e.PollAlarms(new CalDateTime(1997, 3, 10), null)
            .Select(x => x.DateTime)
            .ToList();

        var expectedAlarms = new[]
        {
            new CalDateTime(new DateTime(1997, 3, 17, 13, 30, 0, DateTimeKind.Utc)),
            new CalDateTime(new DateTime(1997, 3, 17, 13, 45, 0, DateTimeKind.Utc)),
            new CalDateTime(new DateTime(1997, 3, 17, 14, 0, 0, DateTimeKind.Utc)),
            new CalDateTime(new DateTime(1997, 3, 17, 14, 15, 0, DateTimeKind.Utc)),
            new CalDateTime(new DateTime(1997, 3, 17, 14, 30, 0, DateTimeKind.Utc)),
        };

        Assert.That(results, Is.EquivalentTo(expectedAlarms));
    }

    [Test]
    public void RelativeAlarmWithRepeat()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(1997, 3, 18, 8, 30, 0, "America/New_York")
        };

        var valarm = """
            BEGIN:VALARM
            TRIGGER:-PT30M
            REPEAT:2
            DURATION:PT15M
            ACTION:DISPLAY
            DESCRIPTION:Breakfast meeting with executive
                team at 8:30 AM EST.
            END:VALARM
            """;

        var alarm = SimpleDeserializer.Default
            .Deserialize(new StringReader(valarm))
            .Cast<Alarm>()
            .Single();

        e.Alarms.Add(alarm);

        var results = e.PollAlarms(new CalDateTime(1997, 3, 18), null)
            .Select(x => x.DateTime)
            .ToList();

        var expectedAlarms = new[]
        {
            new CalDateTime(1997, 3, 18, 8, 0, 0, "America/New_York"),
            new CalDateTime(1997, 3, 18, 8, 15, 0, "America/New_York"),
            new CalDateTime(1997, 3, 18, 8, 30, 0, "America/New_York"),
        };

        Assert.That(results, Is.EquivalentTo(expectedAlarms));
    }

    [Test]
    public void RelativeAlarmDaysBefore()
    {
        Todo todo = new()
        {
            Start = new CalDateTime(1997, 3, 18, 7, 30, 0, "America/New_York"),
            Due = new CalDateTime(1997, 3, 18, 8, 30, 0, "America/New_York"),
        };

        var valarm = """
            BEGIN:VALARM
            TRIGGER;RELATED=END:-P2D
            ACTION:EMAIL
            ATTENDEE:mailto:john_doe@example.com
            SUMMARY:*** REMINDER: SEND AGENDA FOR WEEKLY STAFF MEETING ***
            DESCRIPTION:A draft agenda needs to be sent out to the attendees
              to the weekly managers meeting (MGR-LIST). Attached is a
              pointer the document template for the agenda file.
            ATTACH;FMTTYPE=application/msword:http://example.com/
             templates/agenda.doc
            END:VALARM
            """;

        var alarm = SimpleDeserializer.Default
            .Deserialize(new StringReader(valarm))
            .Cast<Alarm>()
            .Single();

        todo.Alarms.Add(alarm);

        var results = todo.PollAlarms(new CalDateTime(1997, 3, 10), new CalDateTime(1997, 3, 20))
            .Select(x => x.DateTime)
            .ToList();

        var expectedAlarms = new[]
        {
            new CalDateTime(1997, 3, 16, 8, 30, 0, "America/New_York"),
        };

        Assert.That(results, Is.EquivalentTo(expectedAlarms));
    }

    #endregion


    [Test]
    public void AlarmWithExactTime()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7)
        };

        e.Alarms.Add(new Alarm()
        {
            Trigger = new Trigger
            {
                DateTime = new CalDateTime(new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc))
            }
        });

        var alarmOccurrences = e.PollAlarms(e.Start, e.Start.AddDays(1))
            .Select(x => x.DateTime!)
            .ToList();

        var expectedAlarms = new[]
        {
            new CalDateTime(new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc)),
        };

        Assert.That(alarmOccurrences, Is.EquivalentTo(expectedAlarms));
    }

    [Test]
    public void RecurringAlarm()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7),
            RecurrenceRule = new RecurrencePattern(FrequencyType.Weekly, 1)
        };

        e.Alarms.Add(new Alarm()
        {
            Trigger = new Trigger(new Duration(days: -1))
        });

        var alarmOccurrences = e.PollAlarms(e.Start, e.Start.AddDays(21))
            .Select(x => x.DateTime!)
            .ToList();

        var expectedAlarms = new[]
        {
            new CalDateTime(2026, 4, 6),
            new CalDateTime(2026, 4, 13),
            new CalDateTime(2026, 4, 20),
        };

        Assert.That(alarmOccurrences, Is.EquivalentTo(expectedAlarms));
    }

    [Test]
    public void AlarmWithoutParentIsEmpty()
    {
        var alarm = new Alarm()
        {
            Trigger = new()
            {
                DateTime = new CalDateTime(new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc))
            }
        };

        var occurrences = alarm.Poll(null, null);

        Assert.That(occurrences, Is.Empty);
    }
}
