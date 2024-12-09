﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NUnit.Framework;

namespace Ical.Net.Tests;

internal class GetOccurrenceTests
{
    public static CalendarCollection GetCalendars(string incoming) => CalendarCollection.Load(incoming);

    [Test]
    public void WrongDurationTest()
    {
        var firstStart = new CalDateTime(DateTime.Parse("2016-01-01"));
        var firstEnd = new CalDateTime(DateTime.Parse("2016-01-05"));
        var vEvent = new CalendarEvent { DtStart = firstStart, DtEnd = firstEnd, };

        var secondStart = new CalDateTime(DateTime.Parse("2016-03-01"));
        var secondEnd = new CalDateTime(DateTime.Parse("2016-03-05"));
        var vEvent2 = new CalendarEvent { DtStart = secondStart, DtEnd = secondEnd, };

        var calendar = new Calendar();
        calendar.Events.Add(vEvent);
        calendar.Events.Add(vEvent2);

        var searchStart = DateTime.Parse("2015-12-29");
        var searchEnd = DateTime.Parse("2017-02-10");
        var occurrences = calendar.GetOccurrences(searchStart, searchEnd).OrderBy(o => o.Period.StartTime).ToList();

        var firstOccurrence = occurrences.First();
        var firstStartCopy = firstStart.Copy<CalDateTime>();
        var firstEndCopy = firstEnd.Copy<CalDateTime>();
        Assert.Multiple(() =>
        {
            Assert.That(firstOccurrence.Period.StartTime, Is.EqualTo(firstStartCopy));
            Assert.That(firstOccurrence.Period.EndTime, Is.EqualTo(firstEndCopy));
        });

        var secondOccurrence = occurrences.Last();
        var secondStartCopy = secondStart.Copy<CalDateTime>();
        var secondEndCopy = secondEnd.Copy<CalDateTime>();
        Assert.Multiple(() =>
        {
            Assert.That(secondOccurrence.Period.StartTime, Is.EqualTo(secondStartCopy));
            Assert.That(secondOccurrence.Period.EndTime, Is.EqualTo(secondEndCopy));
        });
    }

    [Test]
    public void SkippedOccurrenceOnWeeklyPattern()
    {
        const int evaluationsCount = 1000;
        var eventStart = new CalDateTime(new DateTime(2016, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var eventEnd = new CalDateTime(new DateTime(2016, 1, 1, 11, 0, 0, DateTimeKind.Utc));
        var vEvent = new CalendarEvent
        {
            DtStart = eventStart,
            DtEnd = eventEnd,
        };

        var pattern = new RecurrencePattern
        {
            Frequency = FrequencyType.Weekly,
            ByDay = new List<WeekDay> { new WeekDay(DayOfWeek.Friday) }
        };
        vEvent.RecurrenceRules.Add(pattern);
        var calendar = new Calendar();
        calendar.Events.Add(vEvent);

        var intervalStart = eventStart;
        var intervalEnd = intervalStart.AddDays(7 * evaluationsCount);

        var occurrences = RecurrenceUtil.GetOccurrences(
            recurrable: vEvent,
            periodStart: intervalStart,
            periodEnd: intervalEnd,
            includeReferenceDateInResults: false);
        var occurrenceSet = new HashSet<IDateTime>(occurrences.Select(o => o.Period.StartTime));

        Assert.That(occurrenceSet, Has.Count.EqualTo(evaluationsCount));

        for (var currentOccurrence = intervalStart; currentOccurrence.CompareTo(intervalEnd) < 0; currentOccurrence = (CalDateTime) currentOccurrence.AddDays(7))
        {
            var contains = occurrenceSet.Contains(currentOccurrence);
            Assert.That(contains, Is.True, $"Collection does not contain {currentOccurrence}, but it is a {currentOccurrence.DayOfWeek}");
        }
    }


    [Test]
    public void EnumerationChangedException()
    {
        const string ical = @"BEGIN:VCALENDAR
PRODID:-//Google Inc//Google Calendar 70.9054//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
X-WR-CALNAME:name
X-WR-TIMEZONE:America/New_York
BEGIN:VTIMEZONE
TZID:America/New_York
X-LIC-LOCATION:America/New_York
BEGIN:DAYLIGHT
TZOFFSETFROM:-0500
TZOFFSETTO:-0400
TZNAME:EDT
DTSTART:19700308T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:-0400
TZOFFSETTO:-0500
TZNAME:EST
DTSTART:19701101T020000
RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU
END:STANDARD
END:VTIMEZONE

BEGIN:VEVENT
DTSTART;TZID=America/New_York:20161011T170000
DTEND;TZID=America/New_York:20161011T180000
DTSTAMP:20160930T115710Z
UID:blablabla
RECURRENCE-ID;TZID=America/New_York:20161011T170000
CREATED:20160830T144559Z
DESCRIPTION:
LAST-MODIFIED:20160928T142659Z
LOCATION:Location1
SEQUENCE:0
STATUS:CONFIRMED
SUMMARY:Summary1
TRANSP:OPAQUE
END:VEVENT

END:VCALENDAR";

        var calendar = GetCalendars(ical);
        var date = new DateTime(2016, 10, 11);
        var occurrences = calendar.GetOccurrences(date, date.AddDays(1)).ToList();

        //We really want to make sure this doesn't explode
        Assert.That(occurrences, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetOccurrencesWithRecurrenceIdShouldEnumerate()
    {
        const string ical = """
            BEGIN:VCALENDAR
            PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
            VERSION:2.0
            BEGIN:VTIMEZONE
            TZID:W. Europe Standard Time
            BEGIN:STANDARD
            DTSTART:16010101T030000
            RRULE:FREQ=YEARLY;BYDAY=SU;BYMONTH=10;BYSETPOS=-1
            TZNAME:Mitteleuropäische Zeit
            TZOFFSETFROM:+0200
            TZOFFSETTO:+0100
            END:STANDARD
            BEGIN:DAYLIGHT
            DTSTART:00010101T020000
            RRULE:FREQ=YEARLY;BYDAY=SU;BYMONTH=3;BYSETPOS=-1
            TZNAME:Mitteleuropäische Sommerzeit
            TZOFFSETFROM:+0100
            TZOFFSETTO:+0200
            END:DAYLIGHT
            END:VTIMEZONE
            BEGIN:VEVENT
            BACKGROUND:BUSY
            DESCRIPTION:Backup Daten
            DTEND;TZID=W. Europe Standard Time:20150305T043000
            DTSTAMP:20161122T120652Z
            DTSTART;TZID=W. Europe Standard Time:20150305T000100
            RESOURCES:server
            RRULE:FREQ=WEEKLY;BYDAY=MO;BYHOUR=0,12
            SUMMARY:Server
            UID:a30ed847-8000-4c53-9e58-99c8f9cf7c4b
            X-LIGHTSOUT-ACTION:START=WakeUp\;END=Reboot\,Force
            X-LIGHTSOUT-MODE:TimeSpan
            X-MICROSOFT-CDO-BUSYSTATUS:BUSY
            END:VEVENT
            BEGIN:VEVENT
            BACKGROUND:BUSY
            DESCRIPTION:Backup Daten
            DTEND;TZID=W. Europe Standard Time:20161128T043000
            DTSTAMP:20161122T120652Z
            DTSTART;TZID=W. Europe Standard Time:20161128T150100
            RECURRENCE-ID:20161128T000100
            RESOURCES:server
            SEQUENCE:0
            SUMMARY:Server
            UID:a30ed847-8000-4c53-9e58-99c8f9cf7c4b
            X-LIGHTSOUT-ACTION:START=WakeUp\;END=Reboot\,Force
            X-LIGHTSOUT-MODE:TimeSpan
            X-MICROSOFT-CDO-BUSYSTATUS:BUSY
            END:VEVENT
            END:VCALENDAR
            """;

        var collection = Calendar.Load(ical);
        var startCheck = new DateTime(2016, 11, 11);
        var occurrences = collection.GetOccurrences<CalendarEvent>(startCheck, startCheck.AddMonths(1)).ToList();

        CalDateTime[] expectedStartDates = [
            new CalDateTime("20161114T000100", "W. Europe Standard Time"),
            new CalDateTime("20161114T120100", "W. Europe Standard Time"),
            new CalDateTime("20161121T000100", "W. Europe Standard Time"),
            new CalDateTime("20161121T120100", "W. Europe Standard Time"),
            new CalDateTime("20161128T120100", "W. Europe Standard Time"),
            new CalDateTime("20161128T150100", "W. Europe Standard Time"), // The replaced entry
            new CalDateTime("20161205T000100", "W. Europe Standard Time"),
            new CalDateTime("20161205T120100", "W. Europe Standard Time")
        ];

        // Specify end time that is between the original occurrence ta 20161128T0001 and the overridden one at 20161128T0030.
        // The overridden one shouldn't be returned, because it was replaced and the other one is in the future.
        var occurrences2 = collection.GetOccurrences<CalendarEvent>(new CalDateTime(startCheck), new CalDateTime("20161128T002000", "W. Europe Standard Time"))
            .ToList();

        Assert.Multiple(() =>
        {
            // endTime = 20161211T000000
            Assert.That(occurrences.Select(x => x.Period.StartTime), Is.EqualTo(expectedStartDates));

            // endTime = 20161128T002000
            Assert.That(occurrences2.Select(x => x.Period.StartTime), Is.EqualTo(expectedStartDates.Take(4)));
        });
    }

    [Test]
    public void GetOccurrencesWithRecurrenceId_DateOnly_ShouldEnumerate()
    {
        const string ical = """
            BEGIN:VCALENDAR
            PRODID:-//github.com/ical-org/ical.net//NONSGML ical.net 5.0//EN
            VERSION:2.0
            BEGIN:VEVENT
            UID:789012
            DTSTART;VALUE=DATE:20231001
            DTEND;VALUE=DATE:20231002
            RRULE:FREQ=MONTHLY;BYMONTHDAY=1
            SUMMARY:Monthly Report Due
            END:VEVENT
            BEGIN:VEVENT
            UID:789012
            RECURRENCE-ID;VALUE=DATE:20231101
            DTSTART;VALUE=DATE:20231115
            DTEND;VALUE=DATE:20231116
            SUMMARY:Monthly Report Due (Rescheduled)
            END:VEVENT
            END:VCALENDAR
            """;

        var collection = Calendar.Load(ical);
        var startCheck = new DateTime(2023, 10, 1);
        var occurrences = collection.GetOccurrences<CalendarEvent>(startCheck, startCheck.AddMonths(1))
            .ToList();

        var occurrences2 = collection.GetOccurrences<CalendarEvent>(new CalDateTime(startCheck), new CalDateTime(2023, 12, 31))
            .ToList();

        CalDateTime[] expectedStartDates = [
            new CalDateTime(2023, 10, 1),
            new CalDateTime(2023, 11, 15), // the replaced occurrence
            new CalDateTime(2023, 12,1)
        ];

        Assert.Multiple(() =>
        {
            // For endTime=20231002
            Assert.That(occurrences.Select(x => x.Period.StartTime), Is.EqualTo(expectedStartDates.Take(1)));

            // For endTime=20231231
            Assert.That(occurrences2.Select(x => x.Period.StartTime), Is.EqualTo(expectedStartDates.Take(3)));
        });
    }

    [TestCase]
    public void TestOccurenceEquals()
    {
        var occurrence = new Occurrence(new CalendarEvent() { Description = "o1" }, new Period(new CalDateTime(2023, 10, 1), new CalDateTime(2023, 10, 2)));

        Assert.Multiple(() =>
        {
            Assert.That(occurrence.Equals((object)new Occurrence(
                new CalendarEvent() { Description = "o1" }, new Period(new CalDateTime(2023, 10, 1), new CalDateTime(2023, 10, 2)))),
                Is.True);

            Assert.That(occurrence.Equals((object)new Occurrence(
                new CalendarEvent() { Description = "different" }, new Period(new CalDateTime(2023, 10, 1), new CalDateTime(2023, 10, 2)))),
                Is.False);

            Assert.That(occurrence.Equals((object)new Occurrence(
                new CalendarEvent() { Description = "o1" }, new Period(new CalDateTime(2000, 10, 1), new CalDateTime(2023, 10, 2)))),
                Is.False);

            Assert.That(occurrence.Equals((object)null),
                Is.False);
        });
    }
}
