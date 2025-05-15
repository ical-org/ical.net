//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Ical.Net.Tests;

[TestFixture]
public class RecurrenceTests
{
    private const string _tzid = "US-Eastern";

    private void EventOccurrenceTest(
        Calendar cal,
        CalDateTime? fromDate,
        CalDateTime? toDate,
        Period[] expectedPeriods,
        string[]? timeZones,
        int eventIndex
    )
    {
        var evt = cal.Events.Skip(eventIndex).First();
        var rule = evt.RecurrenceRules.FirstOrDefault();

        var occurrences = evt.GetOccurrences(fromDate).TakeBefore(toDate)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(
                occurrences,
                Has.Count.EqualTo(expectedPeriods.Length),
                "There should have been " + expectedPeriods.Length + " occurrences; there were " + occurrences.Count);

            if (evt.RecurrenceRules.Count > 0)
            {
                Assert.That(evt.RecurrenceRules, Has.Count.EqualTo(1));
            }

            for (var i = 0; i < expectedPeriods.Length; i++)
            {
                var period = new Period(expectedPeriods[i].StartTime, expectedPeriods[i].EffectiveEndTime);

                Assert.That(occurrences[i].Period, Is.EqualTo(period), "Event should occur on " + period);
                if (timeZones != null)
                    Assert.That(period.StartTime.TimeZoneName, Is.EqualTo(timeZones[i]),
                        "Event " + period + " should occur in the " + timeZones[i] + " timezone");
            }
        });
    }

    private void EventOccurrenceTest(
        Calendar cal,
        CalDateTime fromDate,
        CalDateTime toDate,
        Period[] expectedPeriods,
        string[]? timeZones
    )
    {
        EventOccurrenceTest(cal, fromDate, toDate, expectedPeriods, timeZones, 0);
    }

    private static TestCaseData[] EventOccurrenceTestCases = new TestCaseData[]
    {
        new("""
            DTSTART;TZID=Europe/Amsterdam:20201024T023000
            DURATION:PT5M
            RRULE:FREQ=DAILY;UNTIL=20201025T010000Z
            """,
            new []
            {
                "20201024T023000/PT5M",
                "20201025T023000/PT5M"
            }
        ),
    };

    [Test, Category("Recurrence")]
    [TestCaseSource(nameof(EventOccurrenceTestCases))]
    public void EventOccurrenceTest(
        string eventIcal,
        string[] expectedPeriods)
    {
        var eventSerializer = new EventSerializer();
        var calendarIcalStr = $"""
            BEGIN:VCALENDAR
            VERSION:2.0
            BEGIN:VEVENT
            {eventIcal}
            END:VEVENT
            END:VCALENDAR
            """;

        var cal = Calendar.Load(calendarIcalStr)!;
        var tzid = cal.Events.Single().Start!.TzId;

        var periodSerializer = new PeriodSerializer();
        var periods = expectedPeriods
            .Select(p => (Period) periodSerializer.Deserialize(new StringReader(p))!)
            .Select(p =>
                p.Duration is null
                    ? new Period(p.StartTime.ToTimeZone(tzid), p.EndTime)
                    : new Period(p.StartTime.ToTimeZone(tzid), p.Duration.Value))
            .ToArray();

        EventOccurrenceTest(cal, null, null, periods, null, 0);
    }

    /// <summary>
    /// See Page 45 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=2;BYMONTH=1;BYDAY=SU;BYHOUR=8,9;BYMINUTE=30
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyComplex1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyComplex1)!;
        ProgramTest.TestCal(iCal);
        var evt = iCal.Events.First();
        var occurrences = evt.GetOccurrences(
            new CalDateTime(2006, 1, 1))
            .TakeBefore(new CalDateTime(2011, 1, 1)).ToList();

        var dt = new CalDateTime(2007, 1, 1, 8, 30, 0, _tzid);
        var i = 0;

        while (dt.Year < 2011)
        {
            if (dt.GreaterThan(evt.Start!) &&
                (dt.Year % 2 == 1) && // Every-other year from 2005
                (dt.Month == 1) &&
                (dt.DayOfWeek == DayOfWeek.Sunday))
            {
                var dt1 = dt.AddHours(1);
                Assert.Multiple(() =>
                {
                    Assert.That(occurrences[i].Period.StartTime, Is.EqualTo(dt), "Event should occur at " + dt);
                    Assert.That(occurrences[i + 1].Period.StartTime, Is.EqualTo(dt1), "Event should occur at " + dt);
                });
                i += 2;
            }

            dt = dt.AddDays(1);
        }
    }

    /// <summary>
    /// See Page 118 of RFC 2445 - RRULE:FREQ=DAILY;COUNT=10;INTERVAL=2
    /// </summary>
    [Test, Category("Recurrence")]
    public void DailyCount1()
    {
        var iCal = Calendar.Load(IcsFiles.DailyCount1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2006, 7, 1),
            new CalDateTime(2006, 9, 1),
            [
                new(new CalDateTime(2006, 07, 18, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 07, 20, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 07, 22, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 07, 24, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 07, 26, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 07, 28, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 07, 30, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 08, 01, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 08, 03, 10, 00, 00, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(2006, 08, 05, 10, 00, 00, _tzid), Duration.FromHours(1))
            ],
            null
        );
    }

    /// <summary>
    /// See Page 118 of RFC 2445 - RRULE:FREQ=DAILY;UNTIL=19971224T000000Z
    /// </summary>
    [Test, Category("Recurrence")]
    public void DailyUntil1()
    {
        var iCal = Calendar.Load(IcsFiles.DailyUntil1)!;
        ProgramTest.TestCal(iCal);
        var evt = iCal.Events.First();

        var occurrences = evt.GetOccurrences(
            new CalDateTime(1997, 9, 1))
            .TakeBefore(new CalDateTime(1998, 1, 1)).ToList();

        var dt = new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid);
        var i = 0;
        while (dt.Year < 1998)
        {
            if (dt.GreaterThanOrEqual(evt.Start!) &&
                dt.LessThan(new CalDateTime(1997, 12, 24, 0, 0, 0, _tzid)))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(occurrences[i].Period.StartTime, Is.EqualTo(dt), "Event should occur at " + dt);
                    Assert.That(
                        (dt.LessThan(new CalDateTime(1997, 10, 26)) && dt.TimeZoneName == "US-Eastern") ||
                        (dt.GreaterThan(new CalDateTime(1997, 10, 26)) && dt.TimeZoneName == "US-Eastern"),
                        Is.True,
                        "Event " + dt + " doesn't occur in the correct time zone (including Daylight & Standard time zones)");
                });
                i++;
            }

            dt = dt.AddDays(1);
        }
    }

    /// <summary>
    /// See Page 118 of RFC 2445 - RRULE:FREQ=DAILY;INTERVAL=2
    /// </summary>
    [Test, Category("Recurrence")]
    public void Daily1()
    {
        var iCal = Calendar.Load(IcsFiles.Daily1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 1),
            new CalDateTime(1997, 12, 4),
            [
                new(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 6, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 8, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 20, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 22, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 24, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 26, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 6, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 8, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 20, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 22, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 24, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 26, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 21, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 23, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 27, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 12, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 12, 3, 9, 0, 0, _tzid), Duration.FromHours(1))
            ],
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 119 of RFC 2445 - RRULE:FREQ=DAILY;INTERVAL=10;COUNT=5
    /// </summary>
    [Test, Category("Recurrence")]
    public void DailyCount2()
    {
        var iCal = Calendar.Load(IcsFiles.DailyCount2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 1),
            new CalDateTime(1998, 1, 1),
            [
                new(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 22, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 12, 9, 0, 0, _tzid), Duration.FromHours(1))
            ],
            null
        );
    }

    /// <summary>
    /// See Page 119 of RFC 2445 - RRULE:FREQ=DAILY;UNTIL=20000131T090000Z;BYMONTH=1
    /// </summary>
    [Test, Category("Recurrence")]
    public void ByMonth1()
    {
        var iCal = Calendar.Load(IcsFiles.ByMonth1)!;
        ProgramTest.TestCal(iCal);
        var evt = iCal.Events.First();

        var occurrences = evt.GetOccurrences(
            new CalDateTime(1998, 1, 1))
            .TakeBefore(new CalDateTime(2000, 12, 31)).ToList();

        var dt = new CalDateTime(1998, 1, 1, 9, 0, 0, _tzid);
        var i = 0;
        while (dt.Year < 2001)
        {
            if (dt.GreaterThanOrEqual(evt.Start!) &&
                dt.Month == 1 &&
                dt.LessThanOrEqual(new CalDateTime(2000, 1, 31, 9, 0, 0, _tzid)))
            {
                Assert.That(occurrences[i].Period.StartTime, Is.EqualTo(dt), "Event should occur at " + dt);
                i++;
            }

            dt = dt.AddDays(1);
        }
    }

    /// <summary>
    /// See Page 119 of RFC 2445 - RRULE:FREQ=YEARLY;UNTIL=20000131T150000Z;BYMONTH=1;BYDAY=SU,MO,TU,WE,TH,FR,SA
    /// <note>
    ///     The example was slightly modified to fix a suspected flaw in the design of
    ///     the example RRULEs.  UNTIL is always UTC time, but it expected the actual
    ///     time to correspond to other time zones.  Odd.
    /// </note>        
    /// </summary>
    [Test, Category("Recurrence")]
    public void ByMonth2()
    {
        var iCal1 = Calendar.Load(IcsFiles.ByMonth1)!;
        var iCal2 = Calendar.Load(IcsFiles.ByMonth2)!;
        ProgramTest.TestCal(iCal1);
        ProgramTest.TestCal(iCal2);
        var evt1 = iCal1.Events.First();
        var evt2 = iCal2.Events.First();

        var evt1Occurrences = evt1.GetOccurrences(new CalDateTime(1997, 9, 1)).TakeBefore(new CalDateTime(2000, 12, 31)).ToList();
        var evt2Occurrences = evt2.GetOccurrences(new CalDateTime(1997, 9, 1)).TakeBefore(new CalDateTime(2000, 12, 31)).ToList();
        Assert.That(evt1Occurrences.Count == evt2Occurrences.Count, Is.True, "ByMonth1 does not match ByMonth2 as it should");
        for (var i = 0; i < evt1Occurrences.Count; i++)
            Assert.That(evt2Occurrences[i].Period, Is.EqualTo(evt1Occurrences[i].Period), "PERIOD " + i + " from ByMonth1 (" + evt1Occurrences[i] + ") does not match PERIOD " + i + " from ByMonth2 (" + evt2Occurrences[i] + ")");
    }

    /// <summary>
    /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;COUNT=10
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyCount1()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyCount1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 1),
            new CalDateTime(1998, 1, 1),
            [
                new(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 21, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid), Duration.FromHours(1))
            ],
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;UNTIL=19971224T000000Z
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyUntil1()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyUntil1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 1),
            new CalDateTime(1999, 1, 1),
            [
                new(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 21, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 12, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 12, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 12, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new(new CalDateTime(1997, 12, 23, 9, 0, 0, _tzid), Duration.FromHours(1))
            ],
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;WKST=SU
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyWkst1()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyWkst1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 1),
            new CalDateTime(1998, 1, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 23, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 6, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 20, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;UNTIL=19971007T000000Z;WKST=SU;BYDAY=TU,TH
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyUntilWkst1()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyUntilWkst1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 1),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 25, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;COUNT=10;WKST=SU;BYDAY=TU,TH
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyCountWkst1()
    {
        var iCal1 = Calendar.Load(IcsFiles.WeeklyUntilWkst1)!;
        var iCal2 = Calendar.Load(IcsFiles.WeeklyCountWkst1)!;
        ProgramTest.TestCal(iCal1);
        ProgramTest.TestCal(iCal2);
        var evt1 = iCal1.Events.First();
        var evt2 = iCal2.Events.First();

        var evt1Occ = evt1.GetOccurrences(new CalDateTime(1997, 9, 1)).TakeBefore(new CalDateTime(1999, 1, 1)).ToList();
        var evt2Occ = evt2.GetOccurrences(new CalDateTime(1997, 9, 1)).TakeBefore(new CalDateTime(1999, 1, 1)).ToList();
        Assert.That(evt2Occ, Has.Count.EqualTo(evt1Occ.Count), "WeeklyCountWkst1() does not match WeeklyUntilWkst1() as it should");
        for (var i = 0; i < evt1Occ.Count; i++)
        {
            Assert.That(evt2Occ[i].Period, Is.EqualTo(evt1Occ[i].Period), "PERIOD " + i + " from WeeklyUntilWkst1 (" + evt1Occ[i].Period + ") does not match PERIOD " + i + " from WeeklyCountWkst1 (" + evt2Occ[i].Period + ")");
        }
    }

    /// <summary>
    /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;UNTIL=19971224T000000Z;WKST=SU;BYDAY=MO,WE,FR
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyUntilWkst2()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyUntilWkst2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 27, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 31, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 24, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 26, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 8, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 22, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// Tests to ensure FREQUENCY=WEEKLY with INTERVAL=2 works when starting evaluation from an "off" week
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyUntilWkst2_1()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyUntilWkst2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 9),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 27, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 31, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 24, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 26, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 8, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 22, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=8;WKST=SU;BYDAY=TU,TH
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyCountWkst2()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyCountWkst2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 16, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYDAY=1FR
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyCountByDay1()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyCountByDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 2, 6, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 6, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 4, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 6, 5, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;UNTIL=19971224T000000Z;BYDAY=1FR
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyUntilByDay1()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyUntilByDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 5, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=2;COUNT=10;BYDAY=1SU,-1SU
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyCountByDay2()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyCountByDay2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 25, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 31, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=6;BYDAY=-2MO
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyCountByDay3()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyCountByDay3)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 22, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 20, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 22, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 2, 16, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;BYMONTHDAY=-3
    /// </summary>
    [Test, Category("Recurrence")]
    public void ByMonthDay1()
    {
        var iCal = Calendar.Load(IcsFiles.ByMonthDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 3, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 2, 26, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYMONTHDAY=2,15
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyCountByMonthDay1()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyCountByMonthDay1)!;

        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 3, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 15, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYMONTHDAY=1,-1
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyCountByMonthDay2()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyCountByMonthDay2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 3, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 31, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 31, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 31, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 2, 1, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=18;COUNT=10;BYMONTHDAY=10,11,12,13,14,15
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyCountByMonthDay3()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyCountByMonthDay3)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(2000, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 13, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 122 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=2;BYDAY=TU
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyByDay1()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyByDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 4, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 6, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 20, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 27, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 24, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 31, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;COUNT=10;BYMONTH=6,7
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByMonth1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByMonth1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(2002, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 6, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 7, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 6, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 7, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 6, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 7, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2000, 6, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2000, 7, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2001, 6, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2001, 7, 10, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=2;COUNT=10;BYMONTH=1,2,3
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyCountByMonth1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyCountByMonth1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(2003, 4, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 3, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 1, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 2, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2001, 1, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2001, 2, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2001, 3, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2003, 1, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2003, 2, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2003, 3, 10, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=3;COUNT=10;BYYEARDAY=1,100,200
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyCountByYearDay1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyCountByYearDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(2007, 1, 1),
            new[]
            {
                new Period(new CalDateTime(1997, 1, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 4, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 7, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2000, 1, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2000, 4, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2000, 7, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2003, 1, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2003, 4, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2003, 7, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2006, 1, 1, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYDAY=20MO
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByDay1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 5, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// Ordering of byweekno should not matter
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeekNoOrderingShouldNotMatter()
    {
        var start = new CalDateTime(2019, 1, 1);
        var end = new CalDateTime(2019, 12, 31);
        var rpe1 = new RecurrencePatternEvaluator(new RecurrencePattern("FREQ=YEARLY;WKST=MO;BYDAY=MO;BYWEEKNO=1,3,5,7,9,11,13,15,17,19,21,23,25,27,29,31,33,35,37,39,41,43,45,47,49,51,53"));
        var rpe2 = new RecurrencePatternEvaluator(new RecurrencePattern("FREQ=YEARLY;WKST=MO;BYDAY=MO;BYWEEKNO=53,51,49,47,45,43,41,39,37,35,33,31,29,27,25,23,21,19,17,15,13,11,9,7,5,3,1"));

        var recurringPeriods1 = rpe1.Evaluate(new CalDateTime(start), start, null).TakeBefore(end).ToList();
        var recurringPeriods2 = rpe2.Evaluate(new CalDateTime(start), start, null).TakeBefore(end).ToList();

        Assert.That(recurringPeriods2, Has.Count.EqualTo(recurringPeriods1.Count));
    }

    /// <summary>
    /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByWeekNo1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByWeekNo1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 5, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// DTSTART;TZID=US-Eastern:19970512T090000
    /// RRULE:FREQ=YEARLY;BYWEEKNO=20
    /// Includes Monday in week 20 (since 19970512 is a Monday)
    /// of each year.
    /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
    /// and related threads for a fairly in-depth discussion about this topic.
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByWeekNo2()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByWeekNo2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 5, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// DTSTART;TZID=US-Eastern:20020101T100000
    /// RRULE:FREQ=YEARLY;BYWEEKNO=1
    /// Ensures that 20021230 part of week 1 in 2002.
    /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
    /// and related threads for a fairly in-depth discussion about this topic.
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByWeekNo3()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByWeekNo3)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2001, 1, 1),
            new CalDateTime(2003, 1, 31),
            new[]
            {
                new Period(new CalDateTime(2002, 1, 1, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 12, 31, 10, 0, 0, _tzid), Duration.FromMinutes(30))
            },
            null
        );
    }

    /// <summary>
    /// RRULE:FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO,TU,WE,TH,FR,SA,SU
    /// Includes every day in week 20.
    /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
    /// and related threads for a fairly in-depth discussion about this topic.
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByWeekNo4()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByWeekNo4)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 5, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 5, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 5, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 5, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 5, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 5, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 5, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 20, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 21, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 22, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 5, 23, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// DTSTART;TZID=US-Eastern:20020101T100000
    /// RRULE:FREQ=YEARLY;BYWEEKNO=1;BYDAY=MO,TU,WE,TH,FR,SA,SU
    /// Ensures that 20021230 and 20021231 are in week 1.
    /// Also ensures 20011231 is NOT in the result.
    /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
    /// and related threads for a fairly in-depth discussion about this topic.
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByWeekNo5()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByWeekNo5)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2001, 1, 1),
            new CalDateTime(2003, 1, 31),
            new[]
            {
                new Period(new CalDateTime(2002, 1, 1, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 1, 2, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 1, 3, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 1, 4, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 1, 5, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 1, 6, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 12, 30, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2002, 12, 31, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2003, 1, 1, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2003, 1, 2, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2003, 1, 3, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2003, 1, 4, 10, 0, 0, _tzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2003, 1, 5, 10, 0, 0, _tzid), Duration.FromMinutes(30))
            },
            null
        );
    }

    /// <summary>
    /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=TH
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByMonth2()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByMonth2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 3, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 3, 20, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 3, 27, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 26, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 3, 25, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYDAY=TH;BYMONTH=6,7,8
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByMonth3()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByMonth3)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1999, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 6, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 6, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 6, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 6, 26, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 7, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 7, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 7, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 7, 24, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 7, 31, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 14, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 21, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 28, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 6, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 6, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 6, 18, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 6, 25, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 7, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 7, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 7, 16, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 7, 23, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 7, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 8, 6, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 8, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 8, 20, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 8, 27, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 6, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 6, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 6, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 6, 24, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 7, 1, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 7, 8, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 7, 15, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 7, 22, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 7, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 8, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 8, 12, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 8, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 8, 26, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 123 of RFC 2445:
    /// EXDATE;TZID=US-Eastern:19970902T090000
    /// RRULE:FREQ=MONTHLY;BYDAY=FR;BYMONTHDAY=13
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyByMonthDay1()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyByMonthDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(2000, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1998, 2, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 11, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1999, 8, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2000, 10, 13, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;BYDAY=SA;BYMONTHDAY=7,8,9,10,11,12,13
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyByMonthDay2()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyByMonthDay2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 6, 30),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 8, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 13, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 2, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 4, 11, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 5, 9, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 6, 13, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 124 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=4;BYMONTH=11;BYDAY=TU;BYMONTHDAY=2,3,4,5,6,7,8
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyByMonthDay1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyByMonthDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(2004, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1996, 11, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2000, 11, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(2004, 11, 2, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=3;BYDAY=TU,WE,TH;BYSETPOS=3
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyBySetPos1()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyBySetPos1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(2004, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 7, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 6, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;BYDAY=MO,TU,WE,TH,FR;BYSETPOS=-2
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyBySetPos2()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyBySetPos2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 3, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 10, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 11, 27, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 12, 30, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 1, 29, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 2, 26, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1998, 3, 30, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            }
        );
    }

    /// <summary>
    /// See Page 125 of RFC 2445 - RRULE:FREQ=HOURLY;INTERVAL=3;UNTIL=19970902T170000Z        
    /// FIXME: The UNTIL time on this item has been altered to 19970902T190000Z to
    /// match the local EDT time occurrence of 3:00pm.  Is the RFC example incorrect?
    /// </summary>
    [Test, Category("Recurrence")]
    public void HourlyUntil1()
    {
        var iCal = Calendar.Load(IcsFiles.HourlyUntil1)!;
        EventOccurrenceTest(
            iCal,
            fromDate: new CalDateTime(1996, 1, 1),
            toDate: new CalDateTime(1998, 3, 31),
            expectedPeriods: new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 12, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 15, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 18, 0, 0, _tzid), Duration.FromHours(1)),
            },
            timeZones: null
        );
    }

    /// <summary>
    /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=15;COUNT=6
    /// </summary>
    [Test, Category("Recurrence")]
    public void MinutelyCount1()
    {
        var iCal = Calendar.Load(IcsFiles.MinutelyCount1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 2),
            new CalDateTime(1997, 9, 3),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 9, 15, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 9, 30, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 9, 45, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 10, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 10, 15, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=90;COUNT=4
    /// </summary>
    [Test, Category("Recurrence")]
    public void MinutelyCount2()
    {
        var iCal = Calendar.Load(IcsFiles.MinutelyCount2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 10, 30, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 12, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 13, 30, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3827441
    /// </summary>
    [Test, Category("Recurrence")]
    public void MinutelyCount3()
    {
        var iCal = Calendar.Load(IcsFiles.MinutelyCount3)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2010, 8, 27),
            new CalDateTime(2010, 8, 28),
            new[]
            {
                new Period(new CalDateTime(2010, 8, 27, 11, 0, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 1, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 2, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 3, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 4, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 5, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 6, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 7, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 8, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 9, 0, _tzid), Duration.FromMinutes(3))
            },
            null
        );
    }

    /// <summary>
    /// See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3827441
    /// </summary>
    [Test, Category("Recurrence")]
    public void MinutelyCount4()
    {
        var iCal = Calendar.Load(IcsFiles.MinutelyCount4)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2010, 8, 27),
            new CalDateTime(2010, 8, 28),
            new[]
            {
                new Period(new CalDateTime(2010, 8, 27, 11, 0, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 7, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 14, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 21, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 28, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 35, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 42, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 49, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 11, 56, 0, _tzid), Duration.FromMinutes(3)),
                new Period(new CalDateTime(2010, 8, 27, 12, 3, 0, _tzid), Duration.FromMinutes(3))
            },
            null
        );
    }

    /// <summary>
    /// See Page 125 of RFC 2445 - RRULE:FREQ=DAILY;BYHOUR=9,10,11,12,13,14,15,16;BYMINUTE=0,20,40
    /// </summary>
    [Test, Category("Recurrence")]
    public void DailyByHourMinute1()
    {
        var iCal = Calendar.Load(IcsFiles.DailyByHourMinute1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1997, 9, 2),
            new CalDateTime(1997, 9, 4),
            new[]
            {
                new Period(new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 9, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 9, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 10, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 10, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 10, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 11, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 11, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 11, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 12, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 12, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 12, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 13, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 13, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 13, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 14, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 14, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 14, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 15, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 15, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 15, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 16, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 16, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 2, 16, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 9, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 9, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 10, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 10, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 10, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 11, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 11, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 11, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 12, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 12, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 12, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 13, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 13, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 13, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 14, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 14, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 14, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 15, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 15, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 15, 40, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 16, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 16, 20, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 9, 3, 16, 40, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=20;BYHOUR=9,10,11,12,13,14,15,16
    /// </summary>
    [Test, Category("Recurrence")]
    public void MinutelyByHour1()
    {
        var iCal1 = Calendar.Load(IcsFiles.DailyByHourMinute1)!;
        var iCal2 = Calendar.Load(IcsFiles.MinutelyByHour1)!;
        ProgramTest.TestCal(iCal1);
        ProgramTest.TestCal(iCal2);
        var evt1 = iCal1.Events.First();
        var evt2 = iCal2.Events.First();

        var evt1Occ = evt1.GetOccurrences(new CalDateTime(1997, 9, 1)).TakeBefore(new CalDateTime(1997, 9, 3)).ToList();
        var evt2Occ = evt2.GetOccurrences(new CalDateTime(1997, 9, 1)).TakeBefore(new CalDateTime(1997, 9, 3)).ToList();
        Assert.That(evt1Occ.Count == evt2Occ.Count, Is.True, "MinutelyByHour1() does not match DailyByHourMinute1() as it should");
        for (var i = 0; i < evt1Occ.Count; i++)
            Assert.That(evt2Occ[i].Period, Is.EqualTo(evt1Occ[i].Period), "PERIOD " + i + " from DailyByHourMinute1 (" + evt1Occ[i].Period + ") does not match PERIOD " + i + " from MinutelyByHour1 (" + evt2Occ[i].Period + ")");
    }

    /// <summary>
    /// See Page 125 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=MO
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyCountWkst3()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyCountWkst3)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 8, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 10, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 24, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// See Page 125 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=SU
    /// This is the same as WeeklyCountWkst3, except WKST is SU, which changes the results.
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyCountWkst4()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyCountWkst4)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(1996, 1, 1),
            new CalDateTime(1998, 12, 31),
            new[]
            {
                new Period(new CalDateTime(1997, 8, 5, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 17, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 19, 9, 0, 0, _tzid), Duration.FromHours(1)),
                new Period(new CalDateTime(1997, 8, 31, 9, 0, 0, _tzid), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// Tests WEEKLY Frequencies to ensure that those with an INTERVAL > 1
    /// are correctly handled.  See Bug #1741093 - WEEKLY frequency eval behaves strangely.
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug1741093()
    {
        var iCal = Calendar.Load(IcsFiles.Bug1741093)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 7, 1),
            new CalDateTime(2007, 8, 1),
            new[]
            {
                new Period(new CalDateTime(2007, 7, 2, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 3, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 4, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 5, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 6, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 16, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 17, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 18, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 19, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 20, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 30, 8, 0, 0, _tzid), Duration.FromHours(9)),
                new Period(new CalDateTime(2007, 7, 31, 8, 0, 0, _tzid), Duration.FromHours(9))
            },
            null
        );
    }

    [Test, Category("Recurrence")]
    public void Secondly_DefinedNumberOfOccurrences_ShouldSucceed()
    {
        var iCal = Calendar.Load(IcsFiles.Secondly1)!;

        var start = new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid);
        var end = new CalDateTime(2007, 6, 21, 8, 1, 0, _tzid); // End period is exclusive, not inclusive.

        var periods = new List<Period>();
        for (var dt = start; dt.LessThan(end); dt = (CalDateTime) dt.AddSeconds(1))
        {
            periods.Add(new Period(new CalDateTime(dt), Duration.FromHours(9)));
        }

        EventOccurrenceTest(iCal, start, end, periods.ToArray(), null);
    }

    [Test, Category("Recurrence")]
    public void Minutely_DefinedNumberOfOccurrences_ShouldSucceed()
    {
        var iCal = Calendar.Load(IcsFiles.Minutely1)!;

        var start = new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid);
        var end = new CalDateTime(2007, 6, 21, 12, 0, 1, _tzid); // End period is exclusive, not inclusive.

        var periods = new List<Period>();
        for (var dt = start; dt.LessThan(end); dt = (CalDateTime)dt.AddMinutes(1))
        {
            periods.Add(new Period(new CalDateTime(dt), Duration.FromHours(9)));
        }

        EventOccurrenceTest(iCal, start, end, periods.ToArray(), null);
    }

    [Test, Category("Recurrence")]
    public void Hourly_DefinedNumberOfOccurrences_ShouldSucceed()
    {
        var iCal = Calendar.Load(IcsFiles.Hourly1)!;

        var start = new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid);
        var end = new CalDateTime(2007, 6, 25, 8, 0, 1, _tzid); // End period is exclusive, not inclusive.

        var periods = new List<Period>();
        for (var dt = start; dt.LessThan(end); dt = (CalDateTime)dt.AddHours(1))
        {
            periods.Add(new Period(new CalDateTime(dt), Duration.FromHours(9)));
        }

        EventOccurrenceTest(iCal, start, end, periods.ToArray(), null);
    }

    /// <summary>
    /// Ensures that "off-month" calculation works correctly
    /// </summary>
    [Test, Category("Recurrence")]
    public void MonthlyInterval1()
    {
        var iCal = Calendar.Load(IcsFiles.MonthlyInterval1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2008, 1, 1, 7, 0, 0, _tzid),
            new CalDateTime(2008, 2, 29, 7, 0, 0, _tzid),
            new[]
            {
                new Period(new CalDateTime(2008, 2, 11, 7, 0, 0, _tzid), Duration.FromHours(24)),
                new Period(new CalDateTime(2008, 2, 12, 7, 0, 0, _tzid), Duration.FromHours(24))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that "off-year" calculation works correctly
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyInterval1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyInterval1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2006, 1, 1, 7, 0, 0, _tzid),
            new CalDateTime(2007, 1, 31, 7, 0, 0, _tzid),
            new[]
            {
                new Period(new CalDateTime(2007, 1, 8, 7, 0, 0, _tzid), Duration.FromHours(24)),
                new Period(new CalDateTime(2007, 1, 9, 7, 0, 0, _tzid), Duration.FromHours(24))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that "off-day" calculation works correctly
    /// </summary>
    [Test, Category("Recurrence")]
    public void DailyInterval1()
    {
        var iCal = Calendar.Load(IcsFiles.DailyInterval1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 4, 11, 7, 0, 0, _tzid),
            new CalDateTime(2007, 4, 16, 7, 0, 0, _tzid),
            new[]
            {
                new Period(new CalDateTime(2007, 4, 12, 7, 0, 0, _tzid), Duration.FromHours(24)),
                new Period(new CalDateTime(2007, 4, 15, 7, 0, 0, _tzid), Duration.FromHours(24))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that "off-hour" calculation works correctly
    /// </summary>
    [Test, Category("Recurrence")]
    public void HourlyInterval1()
    {
        var iCal = Calendar.Load(IcsFiles.HourlyInterval1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 4, 9, 10, 0, 0, _tzid),
            new CalDateTime(2007, 4, 10, 20, 0, 0, _tzid),
            new[]
            {
                // NOTE: this instance is included in the result set because it ends
                // after the start of the evaluation period.
                // See bug #3007244.
                // https://sourceforge.net/tracker/?func=detail&aid=3007244&group_id=187422&atid=921236
                new Period(new CalDateTime(2007, 4, 9, 7, 0, 0, _tzid), Duration.FromHours(24)),
                new Period(new CalDateTime(2007, 4, 10, 1, 0, 0, _tzid), Duration.FromHours(24)),
                new Period(new CalDateTime(2007, 4, 10, 19, 0, 0, _tzid), Duration.FromHours(24))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that the following recurrence functions properly.
    /// The desired result is "The last Weekend-day of September for the next 10 years."
    /// This specifically tests the BYSETPOS=-1 to accomplish this.
    /// </summary>
    [Test, Category("Recurrence")]
    public void YearlyBySetPos1()
    {
        var iCal = Calendar.Load(IcsFiles.YearlyBySetPos1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2009, 1, 1, 0, 0, 0, _tzid),
            new CalDateTime(2020, 1, 1, 0, 0, 0, _tzid),
            new[]
            {
                new Period(new CalDateTime(2009, 9, 27, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 9, 26, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2011, 9, 25, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2012, 9, 30, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2013, 9, 29, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2014, 9, 28, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2015, 9, 27, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2016, 9, 25, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2017, 9, 30, 5, 30, 0), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2018, 9, 30, 5, 30, 0), Duration.FromMinutes(30))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that GetOccurrences() always returns a single occurrence
    /// for a non-recurring event.
    /// </summary>
    [Test, Category("Recurrence")]
    public void Empty1()
    {
        var iCal = Calendar.Load(IcsFiles.Empty1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2009, 1, 1, 0, 0, 0, _tzid),
            new CalDateTime(2010, 1, 1, 0, 0, 0, _tzid),
            new[]
            {
                new Period(new CalDateTime(2009, 9, 27, 5, 30, 0), new Duration(days: 3, minutes: 30))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an HOURLY frequency.
    /// </summary>
    [Test, Category("Recurrence")]
    public void HourlyInterval2()
    {
        var iCal = Calendar.Load(IcsFiles.HourlyInterval2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 4, 9, 7, 0, 0),
            new CalDateTime(2007, 4, 10, 23, 0, 1), // End time is exclusive, not inclusive
            new[]
            {
                new Period(new CalDateTime(2007, 4, 9, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 11, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 15, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 19, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 23, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 10, 3, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 10, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 10, 11, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 10, 15, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 10, 19, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 10, 23, 0, 0), Duration.FromDays(1))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an MINUTELY frequency.
    /// </summary>
    [Test, Category("Recurrence")]
    public void MinutelyInterval1()
    {
        var iCal = Calendar.Load(IcsFiles.MinutelyInterval1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 4, 9, 7, 0, 0),
            new CalDateTime(2007, 4, 9, 12, 0, 1), // End time is exclusive, not inclusive
            new[]
            {
                new Period(new CalDateTime(2007, 4, 9, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 7, 30, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 8, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 8, 30, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 9, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 9, 30, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 10, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 10, 30, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 11, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 11, 30, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 9, 12, 0, 0), Duration.FromDays(1))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an DAILY frequency with an INTERVAL.
    /// </summary>
    [Test, Category("Recurrence")]
    public void DailyInterval2()
    {
        var iCal = Calendar.Load(IcsFiles.DailyInterval2)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 4, 9, 7, 0, 0),
            new CalDateTime(2007, 4, 27, 7, 0, 1), // End time is exclusive, not inclusive
            new[]
            {
                new Period(new CalDateTime(2007, 4, 9, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 11, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 13, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 15, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 17, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 19, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 21, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 23, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 25, 7, 0, 0), Duration.FromDays(1)),
                new Period(new CalDateTime(2007, 4, 27, 7, 0, 0), Duration.FromDays(1))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an DAILY frequency with a BYDAY value.
    /// </summary>
    [Test, Category("Recurrence")]
    public void DailyByDay1()
    {
        var iCal = Calendar.Load(IcsFiles.DailyByDay1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 9, 10, 7, 0, 0),
            new CalDateTime(2007, 9, 27, 7, 0, 1), // End time is exclusive, not inclusive
            new[]
            {
                new Period(new CalDateTime(2007, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 9, 13, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 9, 17, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 9, 20, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 9, 24, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 9, 27, 7, 0, 0), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that DateUtil.AddWeeks works properly when week number is for previous year for selected date.
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyWeekStartsLastYear()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyWeekStartsLastYear)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2012, 1, 1, 7, 0, 0),
            new CalDateTime(2012, 1, 15, 11, 59, 59),
            new[]
            {
                new Period(new CalDateTime(2012, 1, 2, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 3, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 4, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 5, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 6, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 9, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 10, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 11, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 12, 7, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2012, 1, 13, 7, 0, 0), Duration.Zero)
            },
            null
        );
    }

    /// <summary>
    /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a WEEKLY frequency with an INTERVAL.
    /// </summary>
    [Test, Category("Recurrence")]
    public void WeeklyInterval1()
    {
        var iCal = Calendar.Load(IcsFiles.WeeklyInterval1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 9, 10, 7, 0, 0),
            new CalDateTime(2007, 12, 31, 11, 59, 59),
            new[]
            {
                new Period(new CalDateTime(2007, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 9, 24, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 10, 8, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 10, 22, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 11, 5, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 11, 19, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 12, 3, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 12, 17, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 12, 31, 7, 0, 0), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a MONTHLY frequency.
    /// </summary>
    [Test, Category("Recurrence")]
    public void Monthly1()
    {
        var iCal = Calendar.Load(IcsFiles.Monthly1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 9, 10, 7, 0, 0),
            new CalDateTime(2008, 9, 10, 7, 0, 1), // Period end is exclusive, not inclusive
            new[]
            {
                new Period(new CalDateTime(2007, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 10, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 11, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2007, 12, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 1, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 2, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 3, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 4, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 5, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 6, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 7, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 8, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 9, 10, 7, 0, 0), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a YEARLY frequency.
    /// </summary>
    [Test, Category("Recurrence")]
    public void Yearly1()
    {
        var iCal = Calendar.Load(IcsFiles.Yearly1)!;
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2007, 9, 10, 7, 0, 0),
            new CalDateTime(2020, 9, 10, 7, 0, 1), // Period end is exclusive, not inclusive
            new[]
            {
                new Period(new CalDateTime(2007, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2008, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2009, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2010, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2011, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2012, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2013, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2014, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2015, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2016, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2017, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2018, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2019, 9, 10, 7, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2020, 9, 10, 7, 0, 0), Duration.FromHours(1))
            },
            null
        );
    }

    /// <summary>
    /// Tests a bug with WEEKLY recurrence values used with UNTIL.
    /// https://sourceforge.net/tracker/index.php?func=detail&aid=2912657&group_id=187422&atid=921236
    /// Sourceforge.net bug #2912657
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug2912657()
    {
        var iCal = Calendar.Load(IcsFiles.Bug2912657)!;
        var localTzid = iCal.Events.First().Start!.TzId;

        // Daily recurrence
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2009, 12, 4, 0, 0, 0, localTzid),
            new CalDateTime(2009, 12, 12, 0, 0, 0, localTzid),
            new[]
            {
                new Period(new CalDateTime(2009, 12, 4, 2, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2009, 12, 5, 2, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2009, 12, 6, 2, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2009, 12, 7, 2, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2009, 12, 8, 2, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2009, 12, 9, 2, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2009, 12, 10, 2, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            0
        );

        // Weekly with UNTIL value
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2009, 12, 4),
            new CalDateTime(2009, 12, 10),
            new[]
            {
                new Period(new CalDateTime(2009, 12, 4, 2, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            1
        );

        // Weekly with COUNT=2
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2009, 12, 4),
            new CalDateTime(2009, 12, 12),
            new[]
            {
                new Period(new CalDateTime(2009, 12, 4, 2, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2009, 12, 11, 2, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            2
        );
    }

    /// <summary>
    /// Tests a bug with WEEKLY recurrence values that cross year boundaries.
    /// https://sourceforge.net/tracker/?func=detail&aid=2916581&group_id=187422&atid=921236
    /// Sourceforge.net bug #2916581
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug2916581()
    {
        var iCal = Calendar.Load(IcsFiles.Bug2916581)!;
        var localTzid = iCal.TimeZones[0]!.TzId!;

        // Weekly across year boundary
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2009, 12, 25, 0, 0, 0, localTzid),
            new CalDateTime(2010, 1, 3, 0, 0, 0, localTzid),
            new[]
            {
                new Period(new CalDateTime(2009, 12, 25, 11, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 1, 1, 11, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            0
        );

        // Weekly across year boundary
        EventOccurrenceTest(
            iCal,
            new CalDateTime(2009, 12, 25, 0, 0, 0, localTzid),
            new CalDateTime(2010, 1, 3, 0, 0, 0, localTzid),
            new[]
            {
                new Period(new CalDateTime(2009, 12, 26, 11, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 1, 2, 11, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            1
        );
    }

    /// <summary>
    /// Tests a bug with WEEKLY recurrence values
    /// https://sourceforge.net/tracker/?func=detail&aid=2959692&group_id=187422&atid=921236
    /// Sourceforge.net bug #2959692
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug2959692()
    {
        var iCal = Calendar.Load(IcsFiles.Bug2959692)!;
        var localTzid = iCal.TimeZones[0]!.TzId!;

        EventOccurrenceTest(
            iCal,
            new CalDateTime(2008, 1, 1, 0, 0, 0, localTzid),
            new CalDateTime(2008, 4, 1, 0, 0, 0, localTzid),
            new[]
            {
                new Period(new CalDateTime(2008, 1, 3, 17, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2008, 1, 17, 17, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2008, 1, 31, 17, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2008, 2, 14, 17, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2008, 2, 28, 17, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2008, 3, 13, 17, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2008, 3, 27, 17, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            0
        );
    }

    /// <summary>
    /// Tests a bug with DAILY recurrence values
    /// https://sourceforge.net/tracker/?func=detail&aid=2966236&group_id=187422&atid=921236
    /// Sourceforge.net bug #2966236
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug2966236()
    {
        var iCal = Calendar.Load(IcsFiles.Bug2966236)!;
        var localTzid = iCal.TimeZones[0]!.TzId;

        EventOccurrenceTest(
            iCal,
            new CalDateTime(2010, 1, 1, 0, 0, 0, localTzid),
            new CalDateTime(2010, 3, 1, 0, 0, 0, localTzid),
            new[]
            {
                new Period(new CalDateTime(2010, 1, 19, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 1, 26, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 2, 2, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 2, 9, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 2, 16, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 2, 23, 8, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            0
        );

        EventOccurrenceTest(
            iCal,
            new CalDateTime(2010, 2, 1, 0, 0, 0, localTzid),
            new CalDateTime(2010, 3, 1, 0, 0, 0, localTzid),
            new[]
            {
                new Period(new CalDateTime(2010, 2, 2, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 2, 9, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 2, 16, 8, 00, 00, localTzid), Duration.FromMinutes(30)),
                new Period(new CalDateTime(2010, 2, 23, 8, 00, 00, localTzid), Duration.FromMinutes(30))
            },
            null,
            0
        );
    }

    /// <summary>
    /// Tests a bug with events that span a very long period of time. (i.e. weeks, months, etc.)
    /// https://sourceforge.net/tracker/?func=detail&aid=3007244&group_id=187422&atid=921236
    /// Sourceforge.net bug #3007244
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug3007244()
    {
        var iCal = Calendar.Load(IcsFiles.Bug3007244)!;

        // date only cannot have a time zone
        EventOccurrenceTest(
            cal: iCal,
            fromDate: new CalDateTime(2010, 7, 18),
            toDate: new CalDateTime(2010, 7, 26),
            expectedPeriods: new[] { new Period(new CalDateTime(2010, 05, 23), Duration.FromDays(102)) },
            timeZones: null,
            eventIndex: 0
        );

        // date only cannot have a time zone
        EventOccurrenceTest(
            cal: iCal,
            fromDate: new CalDateTime(2011, 7, 18),
            toDate: new CalDateTime(2011, 7, 26),
            expectedPeriods: new[] { new Period(new CalDateTime(2011, 05, 23), Duration.FromDays(102)) },
            timeZones: null,
            eventIndex: 0
        );
    }

    [Test, Category("Recurrence")]
    // If duaration is specified via DTEND or time-only, then the ducation is exact.
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DTEND;TZID=Europe/Vienna:20241020T040000", "20241020T010000/PT3H", "20241027T010000/PT3H", "20241103T010000/PT3H")]
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DURATION:PT3H",                            "20241020T010000/PT3H", "20241027T010000/PT3H", "20241103T010000/PT3H")]

    // specified via DTEND: exact
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DTEND;TZID=Europe/Vienna:20241021T040000", "20241020T010000/PT27H", "20241027T010000/PT27H", "20241103T010000/PT27H")]
    // First days are applied nominal, then time exact
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DURATION:P1DT3H",                          "20241020T010000/PT27H", "20241027T010000/PT28H", "20241103T010000/PT27H")]
    // Exact, because duration is time-only
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DURATION:PT27H",                           "20241020T010000/PT27H", "20241027T010000/PT27H", "20241103T010000/PT27H")]

    // specified via DTEND: exact
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DTEND;TZID=Europe/Vienna:20241027T040000", "20241020T010000/PT172H", "20241027T010000/PT172H", "20241103T010000/PT172H")]
    // First days are applied nominal, then time exact
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DURATION:P7DT3H",                          "20241020T010000/PT171H", "20241027T010000/PT172H", "20241103T010000/PT171H")]
    // Exact, because duration is time-only
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DURATION:PT171H",                          "20241020T010000/PT171H", "20241027T010000/PT171H", "20241103T010000/PT171H")]

    // specified via DTEND: exact
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DTEND;TZID=Europe/Vienna:20241020T023000", "20241020T010000/PT1H30M", "20241027T010000/PT1H30M", "20241103T010000/PT1H30M")]
    // First days are applied nominal, then time exact
    [TestCase("DTSTART;TZID=Europe/Vienna:20241020T010000", "DURATION:PT1H30M",                         "20241020T010000/PT1H30M", "20241027T010000/PT1H30M", "20241103T010000/PT1H30M")]

    // The following cases cover cases where DTSTART or DTEND are nonexistent.
    //
    // There seems to be a conflicting specification in RFC 5545 section 3.3.10 regarding nonexistent recurrences.
    //
    // 1)
    // Recurrence rules may generate recurrence instances with an invalid
    // date (e.g., February 30) or nonexistent local time (e.g., 1:30 AM
    // on a day where the local time is moved forward by an hour at 1:00
    // AM).  Such recurrence instances MUST be ignored and MUST NOT be
    // counted as part of the recurrence set.
    //
    // 2)
    // If the computed local start time of a recurrence instance does not
    // exist, or occurs more than once, for the specified time zone, the
    // time of the recurrence instance is interpreted in the same manner
    // as an explicit DATE-TIME value describing that date and time, as
    // specified in Section 3.3.5.
    //
    // see https://github.com/ical-org/ical.net/issues/681
    [TestCase("DTSTART;TZID=Europe/Vienna:20250316T023000", "DTEND;TZID=Europe/Vienna:20250323T023000", "20250316T023000/PT168H", "20250323T023000/PT168H", "20250330T033000/PT168H")]
    [TestCase("DTSTART;TZID=Europe/Vienna:20250316T023000", "DURATION:P1W",                             "20250316T023000/PT168H", "20250323T023000/PT168H", "20250330T033000/PT168H")]
    [TestCase("DTSTART;TZID=Europe/Vienna:20250316T023000", "DURATION:P7D",                             "20250316T023000/PT168H", "20250323T023000/PT168H", "20250330T033000/PT168H")]

    public void DurationOfRecurrencesOverDst(string dtStart, string dtEnd, string? d1, string? d2, string? d3)
    {
        var iCal = Calendar.Load($"""
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            {dtStart}
            {dtEnd}
            RRULE:FREQ=WEEKLY;COUNT=3
            END:VEVENT
            END:VCALENDAR
            """)!;

        var start = iCal.Events.First().Start;

        var periodSerializer = new PeriodSerializer();
        var expectedPeriods =
            new[] { d1, d2, d3 }
            .Where(x => x != null)
            .Select(x => (Period)periodSerializer.Deserialize(new StringReader(x!))!)
            .ToArray();
        
        for (var index = 0; index < expectedPeriods.Length; index++)
        {
            var p = expectedPeriods[index];
            var newStart = p.StartTime.ToTimeZone(start!.TzId);
            expectedPeriods[index] = Period.Create(newStart, end: newStart.Add(p.Duration!.Value));
        }

        // date only cannot have a time zone
        EventOccurrenceTest(
            cal: iCal,
            fromDate: null,
            toDate: null,
            expectedPeriods: expectedPeriods,
            timeZones: null,
            eventIndex: 0
        );
    }

    /// <summary>
    /// Tests bug BYWEEKNO not working
    /// </summary>
    [Test, Category("Recurrence")]
    public void BugByWeekNoNotWorking()
    {
        var start = new CalDateTime(2019, 1, 1);
        var end = new CalDateTime(2019, 12, 31);
        var rpe = new RecurrencePatternEvaluator(new RecurrencePattern("FREQ=WEEKLY;BYDAY=MO;BYWEEKNO=2"));

        var recurringPeriods = rpe.Evaluate(start, start, default).TakeBefore(end).ToList();

        Assert.That(recurringPeriods, Has.Count.EqualTo(1));
        Assert.That(recurringPeriods.First().StartTime, Is.EqualTo(new CalDateTime(2019, 1, 7)));
    }

    /// <summary>
    /// Tests bug BYMONTH while FREQ=WEEKLY not working
    /// </summary>
    [Test, Category("Recurrence")]
    public void BugByMonthWhileFreqIsWeekly()
    {
        var start = new CalDateTime(2020, 1, 1);
        var end = new CalDateTime(2020, 12, 31);
        var rpe = new RecurrencePatternEvaluator(new RecurrencePattern("FREQ=WEEKLY;BYDAY=MO;BYMONTH=1"));

        var recurringPeriods = rpe.Evaluate(start, start, default).TakeBefore(end).ToList();

        Assert.That(recurringPeriods, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(recurringPeriods[0].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 6)));
            Assert.That(recurringPeriods[1].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 13)));
            Assert.That(recurringPeriods[2].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 20)));
            Assert.That(recurringPeriods[3].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 27)));
        });
    }

    [Test, Category("Recurrence")]
    public void ReccurencePattern_MaxDate_StopsOnCount()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2018, 1, 1, 12, 0, 0),
            Duration = Duration.FromHours(1)
        };

        var pattern = new RecurrencePattern
        {
            Frequency = FrequencyType.Daily,
            Count = 10
        };

        evt.RecurrenceRules.Add(pattern);

        var occurrences = evt.GetOccurrences(new CalDateTime(2018, 1, 1)).TakeBefore(new CalDateTime(DateTime.MaxValue, false)).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(10), "There should be 10 occurrences of this event.");
    }

    /// <summary>
    /// Tests bug BYMONTH while FREQ=MONTHLY not working
    /// </summary>
    [Test, Category("Recurrence")]
    public void BugByMonthWhileFreqIsMonthly()
    {
        var start = new CalDateTime(2020, 1, 1);
        var end = new CalDateTime(2020, 12, 31);
        var rpe = new RecurrencePatternEvaluator(new RecurrencePattern("FREQ=MONTHLY;BYDAY=MO;BYMONTH=1"));

        var recurringPeriods = rpe.Evaluate(start, start, default).TakeBefore(end).ToList();

        Assert.That(recurringPeriods, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(recurringPeriods[0].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 6)));
            Assert.That(recurringPeriods[1].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 13)));
            Assert.That(recurringPeriods[2].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 20)));
            Assert.That(recurringPeriods[3].StartTime, Is.EqualTo(new CalDateTime(2020, 1, 27)));
        });
    }

    /// <summary>
    /// Tests bug #3119920 - missing weekly occurences
    /// See https://sourceforge.net/tracker/?func=detail&aid=3119920&group_id=187422&atid=921236
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug3119920()
    {
        using var sr = new StringReader("FREQ=WEEKLY;UNTIL=20251126T120000;INTERVAL=1;BYDAY=MO");
        var start = new CalDateTime(2010, 11, 27, 9, 0, 0);
        var serializer = new RecurrencePatternSerializer();
        var rp = (RecurrencePattern)serializer.Deserialize(sr)!;
        var rpe = new RecurrencePatternEvaluator(rp);
        var recurringPeriods = rpe.Evaluate(start, start, default).TakeBefore(rp.Until).ToList();

        var period = recurringPeriods.ElementAt(recurringPeriods.Count - 1);

        Assert.That(period.StartTime, Is.EqualTo(new CalDateTime(2025, 11, 24, 9, 0, 0)));
    }

    /// <summary>
    /// Tests bug #3178652 - 29th day of February in recurrence problems
    /// See https://sourceforge.net/tracker/?func=detail&aid=3178652&group_id=187422&atid=921236
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug3178652()
    {
        var evt = new CalendarEvent
        {
            Start = new CalDateTime(2011, 1, 29, 11, 0, 0),
            Duration = Duration.FromMinutes(90),
            Summary = "29th February Test"
        };

        var pattern = new RecurrencePattern
        {
            Frequency = FrequencyType.Monthly,
            Until = new CalDateTime(2011, 12, 25, 0, 0, 0, CalDateTime.UtcTzId),
            FirstDayOfWeek = DayOfWeek.Sunday,
            ByMonthDay = new List<int>(new[] { 29 })
        };

        evt.RecurrenceRules.Add(pattern);

        var occurrences = evt.GetOccurrences(new CalDateTime(2011, 1, 1)).TakeBefore(new CalDateTime(2012, 1, 1)).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(10), "There should be 10 occurrences of this event, one for each month except February and December.");
    }

    /// <summary>
    /// Tests bug #3292737 - Google Repeating Task Until Time  Bug
    /// See https://sourceforge.net/tracker/?func=detail&aid=3292737&group_id=187422&atid=921236
    /// </summary>
    [Test, Category("Recurrence")]
    public void Bug3292737()
    {
        using var sr = new StringReader("FREQ=WEEKLY;UNTIL=20251126");
        var serializer = new RecurrencePatternSerializer();
        var rp = (RecurrencePattern)serializer.Deserialize(sr)!;

        Assert.That(rp, Is.Not.Null);
        Assert.That(rp.Until, Is.EqualTo(new CalDateTime(2025, 11, 26)));
    }

    /// <summary>
    /// Tests Issue #432
    /// See https://github.com/rianjs/ical.net/issues/432
    /// </summary>
    [Test, Category("Recurrence")]
    public void Issue432()
    {
        var rrule = new RecurrencePattern
        {
            Frequency = FrequencyType.Daily,
            Until = CalDateTime.Today.AddMonths(4)
        };
        var vEvent = new CalendarEvent
        {
            Start = new CalDateTime(DateTime.Parse("2019-01-04T08:00Z", CultureInfo.InvariantCulture).ToUniversalTime()),
        };

        vEvent.RecurrenceRules.Add(rrule);

        //Testing on both the first day and the next, results used to be different
        for (var i = 0; i <= 1; i++)
        {
            var checkTime = new CalDateTime(2019, 01, 04, 08, 00, 00, CalDateTime.UtcTzId);
            checkTime = checkTime.AddDays(i);
            //Valid asking for the exact moment
            var occurrences = vEvent.GetOccurrences(checkTime).TakeWhile(p => p.Period.StartTime == checkTime).ToList();
            Assert.That(occurrences, Has.Count.EqualTo(1));

            //Valid if asking for a range starting at the same moment
            occurrences = vEvent.GetOccurrences(checkTime).TakeBefore(checkTime.AddSeconds(1)).ToList();
            Assert.That(occurrences, Has.Count.EqualTo(1));

            //Valid if asking for a range starting before and ending after
            occurrences = vEvent.GetOccurrences(checkTime.AddSeconds(-1)).TakeBefore(checkTime.AddSeconds(1)).ToList();
            Assert.That(occurrences, Has.Count.EqualTo(1));

            //Not valid if asking for a range starting before but ending at the same moment
            occurrences = vEvent.GetOccurrences(checkTime.AddSeconds(-1)).TakeBefore(checkTime).ToList();
            Assert.That(occurrences.Count, Is.EqualTo(0));
        }
    }

    [Test, Category("Recurrence")]
    public void Issue432_AllDay()
    {
        var vEvent = new CalendarEvent
        {
            Start = new CalDateTime(DateTime.Parse("2020-01-11", CultureInfo.InvariantCulture)), // no time means all day
            End = new CalDateTime(DateTime.Parse("2020-01-11T00:00", CultureInfo.InvariantCulture)),
        };

        var occurrences = vEvent.GetOccurrences(new CalDateTime(2020,01, 10, 0, 0, 0)).TakeBefore(new CalDateTime(2020, 01, 11, 00, 00, 00));
        Assert.That(occurrences.Count, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests the iCal holidays downloaded from apple.com
    /// </summary>
    [Test, Category("Recurrence")]
    public void UsHolidays()
    {
        var iCal = Calendar.Load(IcsFiles.UsHolidays);
        Assert.That(iCal, Is.Not.Null, "iCalendar was not loaded.");
        var items = new Dictionary<string, CalDateTime>
        {
            { "Christmas", new CalDateTime(2006, 12, 25)},
            {"Thanksgiving", new CalDateTime(2006, 11, 23)},
            {"Veteran's Day", new CalDateTime(2006, 11, 11)},
            {"Halloween", new CalDateTime(2006, 10, 31)},
            {"Daylight Saving Time Ends", new CalDateTime(2006, 10, 29)},
            {"Columbus Day", new CalDateTime(2006, 10, 9)},
            {"Labor Day", new CalDateTime(2006, 9, 4)},
            {"Independence Day", new CalDateTime(2006, 7, 4)},
            {"Father's Day", new CalDateTime(2006, 6, 18)},
            {"Flag Day", new CalDateTime(2006, 6, 14)},
            {"John F. Kennedy's Birthday", new CalDateTime(2006, 5, 29)},
            {"Memorial Day", new CalDateTime(2006, 5, 29)},
            {"Mother's Day", new CalDateTime(2006, 5, 14)},
            {"Cinco de Mayo", new CalDateTime(2006, 5, 5)},
            {"Earth Day", new CalDateTime(2006, 4, 22)},
            {"Easter", new CalDateTime(2006, 4, 16)},
            {"Tax Day", new CalDateTime(2006, 4, 15)},
            {"Daylight Saving Time Begins", new CalDateTime(2006, 4, 2)},
            {"April Fool's Day", new CalDateTime(2006, 4, 1)},
            {"St. Patrick's Day", new CalDateTime(2006, 3, 17)},
            {"Washington's Birthday", new CalDateTime(2006, 2, 22)},
            {"President's Day", new CalDateTime(2006, 2, 20)},
            {"Valentine's Day", new CalDateTime(2006, 2, 14)},
            {"Lincoln's Birthday", new CalDateTime(2006, 2, 12)},
            {"Groundhog Day", new CalDateTime(2006, 2, 2)},
            {"Martin Luther King, Jr. Day", new CalDateTime(2006, 1, 16)},
            { "New Year's Day", new CalDateTime(2006, 1, 1)},
        };

        var occurrences = iCal.GetOccurrences(
            new CalDateTime(2006, 1, 1))
            .TakeBefore(new CalDateTime(2006, 12, 31))
            .ToList();

        Assert.That(occurrences, Has.Count.EqualTo(items.Count), "The number of holidays did not evaluate correctly.");
        foreach (var o in occurrences)
        {
            var evt = o.Source as CalendarEvent;
            Assert.That(evt, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(items.ContainsKey(evt.Summary!), Is.True, "Holiday text '" + evt.Summary + "' did not match known holidays.");
                Assert.That(o.Period.StartTime, Is.EqualTo(items[evt.Summary!]), "Date/time of holiday '" + evt.Summary + "' did not match.");
            });
        }
    }

    /// <summary>
    /// Ensures that the StartTime and EndTime of periods have
    /// HasTime set to true if the beginning time had HasTime set
    /// to false.
    /// </summary>
    [Category("Recurrence")]
    [TestCase("SECONDLY", 1, true)]
    [TestCase("MINUTELY", 60, true)]
    [TestCase("HOURLY", 3600, true)]
    [TestCase("DAILY", 24 * 3600, false)]
    public void Evaluate1(string freq, int secsPerInterval, bool hasTime)
    {
        var cal = new Calendar();

        var evt = cal.Create<CalendarEvent>();
        evt.Summary = "Event summary";

        // Start at midnight, UTC time
        evt.Start = new CalDateTime(DateTime.UtcNow.Date, false);

        // This case (DTSTART of type DATE and FREQ=MINUTELY) is undefined in RFC 5545.
        // ical.net handles the case by pretending DTSTART has the time set to midnight.
        evt.RecurrenceRules.Add(new RecurrencePattern($"FREQ={freq};INTERVAL=10;COUNT=5"));

        var occurrences = evt.GetOccurrences(evt.Start.AddDays(-1)).TakeBefore(evt.Start.AddDays(100))
            .ToList();

        var startDates = occurrences.Select(x => x.Period.StartTime.Value).ToList();

        var expectedStartDates = Enumerable.Range(0, 5)
            .Select(i => DateTime.UtcNow.Date.AddSeconds(i * secsPerInterval * 10))
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Select(x => x.Period.StartTime.HasTime == hasTime), Is.All.True);
            Assert.That(startDates, Is.EqualTo(expectedStartDates));
        });
    }

    [Test, Category("Recurrence")]
    public void RecurrencePattern1()
    {
        // NOTE: evaluators are not generally meant to be used directly like this.
        // However, this does make a good test to ensure they behave as they should.
        var pattern = new RecurrencePattern("FREQ=SECONDLY;INTERVAL=10");

        var us = new CultureInfo("en-US");

        var startDate = new CalDateTime(DateTime.Parse("3/30/08 11:59:40 PM", us));
        var fromDate = new CalDateTime(DateTime.Parse("3/30/08 11:59:40 PM", us));
        var toDate = new CalDateTime(DateTime.Parse("3/31/08 12:00:11 AM", us));

        var evaluator = new RecurrencePatternEvaluator(pattern);
        var occurrences = evaluator.Evaluate(
            startDate,
            fromDate,
            default)
            .TakeBefore(toDate)
            .ToList();
        Assert.That(occurrences, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(occurrences[0].StartTime, Is.EqualTo(new CalDateTime(DateTime.Parse("03/30/08 11:59:40 PM", us))));
            Assert.That(occurrences[1].StartTime, Is.EqualTo(new CalDateTime(DateTime.Parse("03/30/08 11:59:50 PM", us))));
            Assert.That(occurrences[2].StartTime, Is.EqualTo(new CalDateTime(DateTime.Parse("03/31/08 12:00:00 AM", us))));
            Assert.That(occurrences[3].StartTime, Is.EqualTo(new CalDateTime(DateTime.Parse("03/31/08 12:00:10 AM", us))));
        });
    }

    [Test, Category("Recurrence")]
    public void RecurrencePattern2()
    {
        // NOTE: evaluators are generally not meant to be used directly like this.
        // However, this does make a good test to ensure they behave as they should.
        var pattern = new RecurrencePattern("FREQ=MINUTELY;INTERVAL=1");

        var us = new CultureInfo("en-US");

        var startDate = new CalDateTime(DateTime.Parse("3/31/2008 12:00:10 AM", us));
        var fromDate = new CalDateTime(DateTime.Parse("4/1/2008 10:08:10 AM", us));
        var toDate = new CalDateTime(DateTime.Parse("4/1/2008 10:43:23 AM", us));

        var evaluator = new RecurrencePatternEvaluator(pattern);

        var occurrences = evaluator.Evaluate(
            startDate,
            fromDate,
            default)
            .TakeBefore(toDate);
        Assert.That(occurrences.Count, Is.Not.EqualTo(0));
    }

    [Test, Category("Recurrence")]
    public void GetOccurrences1()
    {
        var cal = new Calendar();
        var evt = cal.Create<CalendarEvent>();
        evt.Start = new CalDateTime(2009, 11, 18, 5, 0, 0);
        evt.End = new CalDateTime(2009, 11, 18, 5, 10, 0);
        evt.RecurrenceRules.Add(new RecurrencePattern(FrequencyType.Daily));
        evt.Summary = "xxxxxxxxxxxxx";

        var previousDateAndTime = new CalDateTime(2009, 11, 17, 0, 15, 0);
        var previousDateOnly = new CalDateTime(2009, 11, 17, 23, 15, 0);
        var laterDateOnly = new CalDateTime(2009, 11, 19, 3, 15, 0);
        var laterDateAndTime = new CalDateTime(2009, 11, 19, 11, 0, 0);
        var end = new CalDateTime(2009, 11, 23, 0, 0, 0);

        var occurrences = evt.GetOccurrences(previousDateAndTime).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(5));

        occurrences = evt.GetOccurrences(previousDateOnly).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(5));

        occurrences = evt.GetOccurrences(laterDateOnly).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(4));

        occurrences = evt.GetOccurrences(laterDateAndTime).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(3));

        // Add ByHour "9" and "12"            
        evt.RecurrenceRules[0].ByHour.Add(9);
        evt.RecurrenceRules[0].ByHour.Add(12);

        occurrences = evt.GetOccurrences(previousDateAndTime).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(10));

        occurrences = evt.GetOccurrences(previousDateOnly).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(10));

        occurrences = evt.GetOccurrences(laterDateOnly).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(8));

        occurrences = evt.GetOccurrences(laterDateAndTime).TakeBefore(end).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(7));
    }

    [Test, Category("Recurrence")]
    public void TryingToSetInvalidFrequency_ShouldThrow()
    {
        Assert.Multiple(() =>
        {
            // Using the constructor
            Assert.That(() => _ = new RecurrencePattern((FrequencyType) int.MaxValue, 1),
                Throws.TypeOf<ArgumentOutOfRangeException>());

            // Using the property
            Assert.That(() => _ = new RecurrencePattern {Frequency = (FrequencyType) 9876543 },
                Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }

    [Test, Category("Recurrence")]
    public void Test2()
    {
        var cal = new Calendar();
        var evt = cal.Create<CalendarEvent>();
        evt.Summary = "Event summary";
        evt.Start = new CalDateTime(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc));

        var recur = new RecurrencePattern();
        recur.Frequency = FrequencyType.Daily;
        recur.Count = 3;
        recur.ByDay.Add(new WeekDay(DayOfWeek.Monday));
        recur.ByDay.Add(new WeekDay(DayOfWeek.Wednesday));
        recur.ByDay.Add(new WeekDay(DayOfWeek.Friday));
        evt.RecurrenceRules.Add(recur);

        var serializer = new RecurrencePatternSerializer();
        Assert.That(string.Compare(serializer.SerializeToString(recur), "FREQ=DAILY;COUNT=3;BYDAY=MO,WE,FR", StringComparison.Ordinal) == 0,
            Is.True,
            "Serialized recurrence string is incorrect");
    }

    [Test, Category("Recurrence")]
    public void Test4()
    {
        var rpattern = new RecurrencePattern();
        rpattern.ByDay.Add(new WeekDay(DayOfWeek.Saturday));
        rpattern.ByDay.Add(new WeekDay(DayOfWeek.Sunday));

        rpattern.Frequency = FrequencyType.Weekly;

        var evtStart = new CalDateTime(2006, 12, 1);
        var evtEnd = new CalDateTime(2007, 1, 1);

        var evaluator = new RecurrencePatternEvaluator(rpattern);

        // Add the exception dates
        var periods = evaluator.Evaluate(
            evtStart,
            evtStart,
            default)
            .TakeBefore(evtEnd)
            .ToList();
        Assert.That(periods, Has.Count.EqualTo(10));
        Assert.Multiple(() =>
        {
            Assert.That(periods[0].StartTime.Day, Is.EqualTo(2));
            Assert.That(periods[1].StartTime.Day, Is.EqualTo(3));
            Assert.That(periods[2].StartTime.Day, Is.EqualTo(9));
            Assert.That(periods[3].StartTime.Day, Is.EqualTo(10));
            Assert.That(periods[4].StartTime.Day, Is.EqualTo(16));
            Assert.That(periods[5].StartTime.Day, Is.EqualTo(17));
            Assert.That(periods[6].StartTime.Day, Is.EqualTo(23));
            Assert.That(periods[7].StartTime.Day, Is.EqualTo(24));
            Assert.That(periods[8].StartTime.Day, Is.EqualTo(30));
            Assert.That(periods[9].StartTime.Day, Is.EqualTo(31));
        });
    }

    [Test]

    // RRULE and RDATE both exceed y10k when converted to UTC, which is done when
    // ordering the occurrences.
    [TestCase("""
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART;TZID=America/New_York:99991231T220000
            RRULE:FREQ=DAILY;BYHOUR=22,23;COUNT=2
            RDATE;TZID=America/Chicago:99991231T221000
            END:VEVENT
            END:VCALENDAR
            """)]

    // y10k exceeded due to the event duration
    [TestCase("""
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART;TZID=America/New_York:99991230T220000
            DURATION:PT24H
            RRULE:FREQ=DAILY;BYHOUR=22,23;COUNT=2
            END:VEVENT
            END:VCALENDAR
            """)]

    // Events are merged in different places than individual RRULES of a single event
    [TestCase("""
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART;TZID=America/New_York:99991231T220000
            END:VEVENT
            BEGIN:VEVENT
            DTSTART;TZID=America/Chicago:99991231T221000
            END:VEVENT
            END:VCALENDAR
            """)]
    public void Recurrence_WithOutOfBoundsUtc_ShouldFailWithCorrectException(string ical)
    {
        var cal = Calendar.Load(ical)!;
        Assert.That(() => cal.GetOccurrences().ToList(), Throws.InstanceOf<EvaluationOutOfRangeException>());
    }

    [Test, Category("Recurrence")]
    public void ExDateShouldFilterOutAllPeriods()
    {
        //One-day event starting Aug 23 (inclusive), ending Aug 24 (exclusive), repeating daily until Aug 24 (exclusive).
        //I.e. an event that occupies all of Aug 23, and no more, with zero recurrences.
        //Then exclude Aug 23 and Aug 24 from the set of recurrences.
        const string ical = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTSTART;VALUE=DATE:20120823
DTEND;VALUE=DATE:20120824
RRULE:FREQ=DAILY;UNTIL=20120824
EXDATE;VALUE=DATE:20120824
EXDATE;VALUE=DATE:20120823
DTSTAMP:20131031T111655Z
CREATED:20120621T142631Z
TRANSP:TRANSPARENT
END:VEVENT
END:VCALENDAR";
        var calendar = Calendar.Load(ical)!;
        var firstEvent = calendar.Events.First();
        var startSearch = new CalDateTime(2010, 1, 1);
        var endSearch = new CalDateTime(2016, 12, 31);

        var occurrences = firstEvent.GetOccurrences(startSearch).TakeBefore(endSearch).Select(o => o.Period).ToList();
        Assert.That(occurrences.Count == 0, Is.True);
    }

    [Test, Category("Recurrence")]
    public void RDateShouldBeUnionedWithRecurrenceSet()
    {
        //Issues #118 and #107 on Github
        const string ical =
@"BEGIN:VCALENDAR
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
VERSION:2.0
BEGIN:VEVENT
DTSTART;TZID=US-Eastern:20160829T080000
DTEND;TZID=US-Eastern:20160829T090000
EXDATE;TZID=US-Eastern:20160830T080000
EXDATE;TZID=US-Eastern:20160831T080000
RDATE;TZID=US-Eastern:20160830T100000
RDATE;TZID=US-Eastern:20160831T100000
RRULE:FREQ=DAILY
UID:abab717c-1786-4efc-87dd-6859c2b48eb6
END:VEVENT
END:VCALENDAR";

        var calendar = Calendar.Load(ical)!;
        var firstEvent = calendar.Events.First();
        var startSearch = new CalDateTime(DateTime.Parse("2015-08-28T07:00:00", CultureInfo.InvariantCulture), _tzid);
        var endSearch = new CalDateTime(DateTime.Parse("2016-08-28T07:00:00", CultureInfo.InvariantCulture).AddDays(7), _tzid);

        var occurrences = firstEvent.GetOccurrences(startSearch).TakeBefore(endSearch)
            .Select(o => o.Period)
            .ToList();

        var firstExpectedOccurrence = new CalDateTime(DateTime.Parse("2016-08-29T08:00:00", CultureInfo.InvariantCulture), _tzid);
        Assert.That(occurrences.First().StartTime, Is.EqualTo(firstExpectedOccurrence));

        var firstExpectedRDate = new CalDateTime(DateTime.Parse("2016-08-30T10:00:00", CultureInfo.InvariantCulture), _tzid);
        Assert.That(occurrences[1].StartTime.Equals(firstExpectedRDate), Is.True);

        var secondExpectedRDate = new CalDateTime(DateTime.Parse("2016-08-31T10:00:00", CultureInfo.InvariantCulture), _tzid);
        Assert.That(occurrences[2].StartTime.Equals(secondExpectedRDate), Is.True);
    }

    [Test]
    public void OccurrenceMustBeCompletelyContainedWithinSearchRange()
    {
        //https://github.com/rianjs/ical.net/issues/121

        const string ical = @"BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
VERSION:2.0
BEGIN:VEVENT
SUMMARY:This is an event
DTEND;TZID=UTC:20160801T080000
DTSTAMP:20160905T142724Z
DTSTART;TZID=UTC:20160801T070000
RRULE:FREQ=WEEKLY;INTERVAL=1;BYDAY=WE;UNTIL=20160831T070000
UID:abab717c-1786-4efc-87dd-6859c2b48eb6
END:VEVENT
END:VCALENDAR";

        var rrule = new RecurrencePattern(FrequencyType.Weekly, interval: 1)
        {
            Until = new CalDateTime("20160831T070000"),
            ByDay = new List<WeekDay> { new WeekDay(DayOfWeek.Wednesday) },
        };

        var start = DateTime.Parse("2016-08-01T07:00:00", CultureInfo.InvariantCulture);
        var end = start.AddHours(1);
        var e = new CalendarEvent
        {
            DtStart = new CalDateTime(start, "UTC"),
            DtEnd = new CalDateTime(end, "UTC"),
            RecurrenceRules = new List<RecurrencePattern> { rrule },
            Summary = "This is an event",
            Uid = "abab717c-1786-4efc-87dd-6859c2b48eb6",
        };

        var deserializedCalendar = Calendar.Load(ical)!;
        var firstEvent = deserializedCalendar.Events.First();
        var calendar = new Calendar();
        calendar.Events.Add(e);

        Assert.That(firstEvent, Is.EqualTo(e));

        var startSearch = new CalDateTime(DateTime.Parse("2016-07-01T00:00:00", CultureInfo.InvariantCulture), "UTC");
        var endSearch = new CalDateTime(DateTime.Parse("2016-08-31T07:00:00", CultureInfo.InvariantCulture), "UTC");

        var lastExpected = new CalDateTime(DateTime.Parse("2016-08-31T07:00:00", CultureInfo.InvariantCulture), "UTC");
        var occurrences = firstEvent.GetOccurrences(startSearch).TakeBefore(endSearch)
                .Select(o => o.Period)
                .ToList();

        Assert.That(occurrences.Last().StartTime.Equals(lastExpected), Is.False);

        //Create 1 second of overlap
        endSearch = new CalDateTime(endSearch.Value.AddSeconds(1), "UTC");
        occurrences = firstEvent.GetOccurrences(startSearch).TakeBefore(endSearch)
            .Select(o => o.Period)
            .ToList();

        Assert.That(occurrences.Last().StartTime.Equals(lastExpected), Is.True);
    }

    [Test]
    public void EventsWithShareUidsShouldGenerateASingleRecurrenceSet()
    {
        // This test goes back to issue #120
        const string ical = """
            BEGIN:VCALENDAR
            PRODID:-//Google Inc//Google Calendar 70.9054//EN
            VERSION:2.0
            CALSCALE:GREGORIAN
            METHOD:PUBLISH
            X-WR-CALNAME:Calendar 2
            X-WR-TIMEZONE:Europe/Bucharest
            BEGIN:VEVENT
            DTSTART;TZID=Europe/Bucharest:20160829T110000
            DTEND;TZID=Europe/Bucharest:20160829T163000
            RRULE:FREQ=DAILY
            DTSTAMP:20160901T104339Z
            UID:gknfcr66sb7rpangtprsthmpn8@google.com
            CREATED:20160901T104300Z
            DESCRIPTION:
            LAST-MODIFIED:20160901T104311Z
            LOCATION:
            SEQUENCE:1
            STATUS:CONFIRMED
            SUMMARY:testRScuAD
            TRANSP:OPAQUE
            END:VEVENT
            BEGIN:VEVENT
            DTSTART;TZID=Europe/Bucharest:20160901T163000
            DTEND;TZID=Europe/Bucharest:20160901T220000
            DTSTAMP:20160901T104339Z
            UID:gknfcr66sb7rpangtprsthmpn8@google.com
            RECURRENCE-ID;TZID=Europe/Bucharest:20160901T110000
            CREATED:20160901T104300Z
            DESCRIPTION:
            LAST-MODIFIED:20160901T104314Z
            LOCATION:
            SEQUENCE:2
            STATUS:CONFIRMED
            SUMMARY:testRScuAD
            TRANSP:OPAQUE
            END:VEVENT
            BEGIN:VEVENT
            DTSTART;TZID=Europe/Bucharest:20160903T070000
            DTEND;TZID=Europe/Bucharest:20160903T123000
            DTSTAMP:20160901T104339Z
            UID:gknfcr66sb7rpangtprsthmpn8@google.com
            RECURRENCE-ID;TZID=Europe/Bucharest:20160903T110000
            CREATED:20160901T104300Z
            DESCRIPTION:
            LAST-MODIFIED:20160901T104315Z
            LOCATION:
            SEQUENCE:2
            STATUS:CONFIRMED
            SUMMARY:testRScuAD
            TRANSP:OPAQUE
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical)!;

        //The API should be something like:
        var orderedOccurrences = calendar.GetOccurrences()
            .Take(10)
            .Select(o => o.Period)
            .ToList();

        var expectedSept1Start = new CalDateTime(DateTime.Parse("2016-09-01T16:30:00", CultureInfo.InvariantCulture), "Europe/Bucharest");
        var expectedSept1End = new CalDateTime(DateTime.Parse("2016-09-01T22:00:00", CultureInfo.InvariantCulture), "Europe/Bucharest");
        Assert.Multiple(() =>
        {
            Assert.That(orderedOccurrences[3].StartTime, Is.EqualTo(expectedSept1Start));
            Assert.That(orderedOccurrences[3].EndTime, Is.EqualTo(expectedSept1End));
        });

        var expectedSept3Start = new CalDateTime(DateTime.Parse("2016-09-03T07:00:00", CultureInfo.InvariantCulture), "Europe/Bucharest");
        var expectedSept3End = new CalDateTime(DateTime.Parse("2016-09-03T12:30:00", CultureInfo.InvariantCulture), "Europe/Bucharest");
        Assert.Multiple(() =>
        {
            Assert.That(orderedOccurrences[5].StartTime, Is.EqualTo(expectedSept3Start));
            Assert.That(orderedOccurrences[5].EndTime, Is.EqualTo(expectedSept3End));
        });
    }

    [Test]
    public void AddExDateToEventAfterGetOccurrencesShouldRecomputeResult()
    {
        var searchStart = _now.AddDays(-1);
        var searchEnd = _now.AddDays(7);
        var e = GetEventWithRecurrenceRules();
        var occurrences = e.GetOccurrences(searchStart).TakeBefore(searchEnd).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(5));

        var exDate = _now.AddDays(1);
        e.ExceptionDates.Add(exDate);
        occurrences = e.GetOccurrences(searchStart).TakeBefore(searchEnd).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(4));

        //Specifying just a date should "black out" that date
        var excludeTwoDaysFromNow = new CalDateTime(_now.Date).AddDays(2);
        e.ExceptionDates.Add(excludeTwoDaysFromNow);
        occurrences = e.GetOccurrences(searchStart).TakeBefore(searchEnd).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(3));
    }

    private static readonly CalDateTime _now = CalDateTime.Now;
    private static readonly CalDateTime _later = _now.AddHours(1);
    private static CalendarEvent GetEventWithRecurrenceRules()
    {
        var dailyForFiveDays = new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Count = 5,
        };

        var calendarEvent = new CalendarEvent
        {
            Start = new CalDateTime(_now),
            End = new CalDateTime(_later),
            RecurrenceRules = new List<RecurrencePattern> { dailyForFiveDays },
            Resources = new List<string>(new[] { "Foo", "Bar", "Baz" }),
        };
        return calendarEvent;
    }

    [Test]
    public void ExDatesShouldGetMergedInOutput()
    {
        var start = _now.AddYears(-1);
        var end = start.AddHours(1);
        var rrule = new RecurrencePattern(FrequencyType.Daily) { Until = new CalDateTime(start.AddYears(2)) };
        var e = new CalendarEvent
        {
            DtStart = new CalDateTime(start),
            DtEnd = new CalDateTime(end),
            RecurrenceRules = new List<RecurrencePattern> { rrule }
        };

        var firstExclusion = new CalDateTime(start.AddDays(4));
        e.ExceptionDates.Add(firstExclusion);
        var serialized = SerializationHelpers.SerializeToString(e);
        Assert.That(Regex.Matches(serialized, "EXDATE:"), Has.Count.EqualTo(1));

        var secondExclusion = new CalDateTime(start.AddDays(5));
        e.ExceptionDates.Add(secondExclusion);
        serialized = SerializationHelpers.SerializeToString(e);
        Assert.That(Regex.Matches(serialized, "EXDATE:"), Has.Count.EqualTo(1));
    }

    [Test]
    public void ExDateTimeZone_Tests()
    {
        const string tzid = "Europe/Stockholm";

        //Repeat daily for 10 days
        var rrule = GetSimpleRecurrencePattern(10);

        var e = new CalendarEvent
        {
            DtStart = new CalDateTime(_now.Date, _now.Time, tzid),
            DtEnd = new CalDateTime(_later.Date, _later.Time, tzid),
            RecurrenceRules = new List<RecurrencePattern> { rrule },
        };

        e.ExceptionDates.Add(new CalDateTime(_now.Date, _now.Time, tzid).AddDays(1));

        var serialized = SerializationHelpers.SerializeToString(e);
        const string expected = "TZID=Europe/Stockholm";
        Assert.That(Regex.Matches(serialized, expected), Has.Count.EqualTo(3));

        e.ExceptionDates.Add(new CalDateTime(_now.Date, _now.Time, tzid).AddDays(2));
        serialized = SerializationHelpers.SerializeToString(e);
        Assert.That(Regex.Matches(serialized, expected), Has.Count.EqualTo(3));
    }

    [Test, Category("Recurrence")]
    public void OneDayRange()
    {
        var vEvent = new CalendarEvent
        {
            Start = new CalDateTime(DateTime.Parse("2019-06-07 0:00:00", CultureInfo.InvariantCulture)),
            End = new CalDateTime(DateTime.Parse("2019-06-08 00:00:00", CultureInfo.InvariantCulture))
        };

        //Testing on both the first day and the next, results used to be different
        for (var i = 0; i <= 1; i++)
        {
            var checkTime = new CalDateTime(2019, 06, 07, 00, 00, 00);
            checkTime = checkTime.AddDays(i);

            //Valid if asking for a range starting at the same moment
            var occurrences = vEvent.GetOccurrences(checkTime).TakeBefore(checkTime.AddDays(1)).ToList();
            Assert.That(occurrences, Has.Count.EqualTo(i == 0 ? 1 : 0));
        }
    }

    [Test, Category("Recurrence")]
    public void SpecificMinute()
    {
        var rrule = new RecurrencePattern
        {
            Frequency = FrequencyType.Daily
        };
        var vEvent = new CalendarEvent
        {
            Start = new CalDateTime(DateTime.Parse("2009-01-01 09:00:00", CultureInfo.InvariantCulture)),
            End = new CalDateTime(DateTime.Parse("2009-01-01 17:00:00", CultureInfo.InvariantCulture))
        };

        vEvent.RecurrenceRules.Add(rrule);

        // Exactly on start time
        var testingTime = new CalDateTime(2019, 6, 7, 9, 0, 0);

        var occurrences = vEvent.GetOccurrences(testingTime).TakeWhile(p => p.Period.StartTime == testingTime).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(1));

        // One second before end time
        testingTime = new CalDateTime(2019, 6, 7, 16, 59, 59);

        occurrences = vEvent.GetOccurrences(testingTime).TakeBefore(testingTime).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(1));

        // Exactly on end time
        testingTime = new CalDateTime(2019, 6, 7, 17, 0, 0);

        occurrences = vEvent.GetOccurrences(testingTime).TakeBefore(testingTime).ToList();
        Assert.That(occurrences.Count, Is.EqualTo(0));
    }

    private static RecurrencePattern GetSimpleRecurrencePattern(int count) => new RecurrencePattern(FrequencyType.Daily, 1) { Count = count, };

    private static CalendarEvent GetSimpleEvent()
    {
        var e = new CalendarEvent
        {
            DtStart = new CalDateTime(_now.Date, _now.Time, _tzid),
            DtEnd = new CalDateTime(_later.Date, _later.Time, _tzid),
        };
        return e;
    }

    [Test]
    public void RecurrenceRuleTests()
    {
        var five = GetSimpleRecurrencePattern(5);
        var ten = GetSimpleRecurrencePattern(10);
        Assert.That(ten, Is.Not.EqualTo(five));
        var eventA = GetSimpleEvent();
        eventA.RecurrenceRules.Add(five);
        eventA.RecurrenceRules.Add(ten);

        var eventB = GetSimpleEvent();
        eventB.RecurrenceRules.Add(ten);
        eventB.RecurrenceRules.Add(five);

        Assert.That(eventB, Is.EqualTo(eventA));

        const string aString =
            """
               BEGIN:VCALENDAR
               PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
               VERSION:2.0
               BEGIN:VEVENT
               DTEND;TZID=UTC:20170228T140000
               DTSTAMP;TZID=UTC:20170413T135927
               DTSTART;TZID=UTC:20170228T060000
               EXDATE;TZID=UTC:20170302T060000,20170303T060000,20170306T060000,20170307T0
                60000,20170308T060000,20170309T060000,20170310T060000,20170313T060000,201
                70314T060000,20170317T060000,20170320T060000,20170321T060000,20170322T060
                000,20170323T060000,20170324T060000,20170327T060000,20170328T060000,20170
                329T060000,20170330T060000,20170331T060000,20170403T060000,20170405T06000
                0,20170406T060000,20170407T060000,20170410T060000,20170411T060000,2017041
                2T060000,20170413T060000,20170417T060000
               IMPORTANCE:None
               RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR
               UID:001b7e43-98df-4fcc-b9ec-345a28a4fc14
               END:VEVENT
               END:VCALENDAR
               """;

        const string bString =
            """
               BEGIN:VCALENDAR
               PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
               VERSION:2.0
               BEGIN:VEVENT
               DTEND;TZID=UTC:20170228T140000
               DTSTAMP:20170428T171444Z
               DTSTART;TZID=UTC:20170228T060000
               EXDATE;TZID=UTC:20170302T060000,20170303T060000,20170306T060000,20170307T060000,
                20170308T060000,20170309T060000,20170310T060000,20170313T060000,20170314T060000,
                20170317T060000,20170320T060000,20170321T060000,20170322T060000,20170323T060000,
                20170324T060000,20170327T060000,20170328T060000,20170329T060000,20170330T060000,
                20170331T060000,20170403T060000,20170405T060000,20170406T060000,20170407T060000,
                20170410T060000,20170411T060000,20170412T060000,20170413T060000,20170417T060000
               IMPORTANCE:None
               RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR
               UID:001b7e43-98df-4fcc-b9ec-345a28a4fc14
               END:VEVENT
               END:VCALENDAR
               """;

        var simpleA = Calendar.Load(aString)!;
        var normalA = Calendar.Load(aString)!;
        var simpleB = Calendar.Load(bString)!;
        var normalB = Calendar.Load(bString)!;

        var calendarList = new List<Calendar> { simpleA, normalA, simpleB, normalB };
        var eventList = new List<CalendarEvent>
        {
            simpleA.Events.Single(),
            normalA.Events.Single(),
            simpleB.Events.Single(),
            normalB.Events.Single(),
        };

        //GetHashCode tests also tests Equals()
        var calendarSet = new HashSet<Calendar>(calendarList);
        Assert.That(calendarSet, Has.Count.EqualTo(1));
        var eventSet = new HashSet<CalendarEvent>(eventList);
        Assert.That(eventSet, Has.Count.EqualTo(1));

        var newEventList = new HashSet<CalendarEvent>();
        newEventList.UnionWith(eventList);
        Assert.That(newEventList, Has.Count.EqualTo(1));
    }

    [Test]
    public void ManyExclusionDatesEqualityTesting()
    {
        const string icalA =
            """
             BEGIN:VCALENDAR
             PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
             VERSION:2.0
             BEGIN:VEVENT
             DTEND;TZID=UTC:20170228T140000
             DTSTAMP;TZID=UTC:20170428T145334
             DTSTART;TZID=UTC:20170228T060000
             EXDATE;TZID=UTC:20170302T060000,20170303T060000,20170306T060000,20170307T0
              60000,20170308T060000,20170309T060000,20170310T060000,20170313T060000,201
              70314T060000,20170317T060000,20170320T060000,20170321T060000,20170322T060
              000,20170323T060000,20170324T060000,20170327T060000,20170328T060000,20170
              329T060000,20170330T060000,20170331T060000,20170403T060000,20170405T06000
              0,20170406T060000,20170407T060000,20170410T060000,20170411T060000,2017041
              2T060000,20170413T060000,20170417T060000,20170418T060000,20170419T060000,
              20170420T060000,20170421T060000,20170424T060000,20170425T060000,20170427T
              060000,20170428T060000,20170501T060000
             IMPORTANCE:None
             RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR
             UID:001b7e43-98df-4fcc-b9ec-345a28a4fc14
             END:VEVENT
             END:VCALENDAR
             """;

        const string icalB =
            """
             BEGIN:VCALENDAR
             PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 2.2//EN
             VERSION:2.0
             BEGIN:VEVENT
             DTEND;TZID=UTC:20170228T140000
             DTSTAMP;TZID=UTC:20170501T131355
             DTSTART;TZID=UTC:20170228T060000
             EXDATE;TZID=UTC:20170302T060000,20170303T060000,20170306T060000,20170307T0
              60000,20170308T060000,20170309T060000,20170310T060000,20170313T060000,201
              70314T060000,20170317T060000,20170320T060000,20170321T060000,20170322T060
              000,20170323T060000,20170324T060000,20170327T060000,20170328T060000,20170
              329T060000,20170330T060000,20170331T060000,20170403T060000,20170405T06000
              0,20170406T060000,20170407T060000,20170410T060000,20170411T060000,2017041
              2T060000,20170413T060000,20170417T060000,20170418T060000,20170419T060000,
              20170420T060000,20170421T060000,20170424T060000,20170425T060000,20170427T
              060000,20170428T060000,20170501T060000
             IMPORTANCE:None
             RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR
             UID:001b7e43-98df-4fcc-b9ec-345a28a4fc14
             END:VEVENT
             END:VCALENDAR
             """;

        // The only textual difference between A and B
        // is a different DTSTAMP, which is not considered significant for equality or hashing

        var collectionA = CalendarCollection.Load(icalA);
        var collectionB = CalendarCollection.Load(icalB);
        var calendarA = collectionA.First();
        var calendarB = collectionB.First();
        var eventA = calendarA.Events.First();
        var eventB = calendarB.Events.First();
        var exDatesA = eventA.ExceptionDates.GetAllDates();
        var exDatesB = eventB.ExceptionDates.GetAllDates();

        Assert.Multiple(() =>
        {
            //Comparing the two...
            Assert.That(collectionB, Is.EqualTo(collectionA));
            Assert.That(collectionB.GetHashCode(), Is.EqualTo(collectionA.GetHashCode()));
            Assert.That(calendarB, Is.EqualTo(calendarA));
            Assert.That(calendarB.GetHashCode(), Is.EqualTo(calendarA.GetHashCode()));
            Assert.That(eventB, Is.EqualTo(eventA));
            Assert.That(eventB.GetHashCode(), Is.EqualTo(eventA.GetHashCode()));
            Assert.That(exDatesB, Is.EqualTo(exDatesA));
        });
    }

    [TestCase(null, false)]
    [TestCase(CalDateTime.UtcTzId, false)]
    [TestCase("America/New_York", true)]
    public void DisallowedUntilShouldThrow(string? tzId, bool shouldThrow)
    {
        var dt = new CalDateTime(2025, 11, 08, 10, 30, 00, tzId);
        var recPattern = new RecurrencePattern(FrequencyType.Daily, 1);

        if (shouldThrow)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => recPattern.Until = dt);
        }
        else
        {
            recPattern.Until = dt;
            Assert.That(recPattern.Until, Is.EqualTo(dt));
        }
    }

    [Test]
    public void InclusiveRruleUntil()
    {
        const string icalText =
            """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART;VALUE=DATE:20180101
            DTEND;VALUE=DATE:20180102
            RRULE:FREQ=WEEKLY;UNTIL=20180105;BYDAY=MO,TU,WE,TH,FR
            DTSTAMP:20170926T001103Z
            UID:5kvks79u4nurqopt7qv4fi1jo8@google.com
            CREATED:20170922T131958Z
            DESCRIPTION:
            LAST-MODIFIED:20170922T131958Z
            LOCATION:
            SEQUENCE:0
            STATUS:CONFIRMED
            SUMMARY:Holiday Break - No School
            TRANSP:TRANSPARENT
            END:VEVENT
            END:VCALENDAR
            """;
        const string timeZoneId = @"Eastern Standard Time";
        var calendar = Calendar.Load(icalText)!;
        var firstEvent = calendar.Events.First();
        var startSearch = new CalDateTime(DateTime.Parse("2017-07-01T00:00:00", CultureInfo.InvariantCulture), timeZoneId);
        var endSearch = new CalDateTime(DateTime.Parse("2018-07-01T00:00:00", CultureInfo.InvariantCulture), timeZoneId);

        var occurrences = firstEvent.GetOccurrences(startSearch).TakeBefore(endSearch).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(5));
    }

    public enum RecurrenceTestExceptionStep
    {
        None,
        Construction,
        GetOccurrenceInvocation,
        Enumeration,
    }

    public class RecurrenceTestCase
    {
        public int LineNumber { get; set; }

        public string? RRule { get; set; }

        public CalDateTime? DtStart { get; set; }

        public Duration? Duration { get; internal set; }

        public CalDateTime? StartAt { get; set; }

        public IReadOnlyList<CalDateTime>? Instances { get; set; }

        public string? Exception { get; set; }

        public RecurrenceTestExceptionStep? ExceptionStep { get; set; }

        public override string ToString()
            => $"Line {LineNumber}: {DtStart}, {RRule}";
    }

    private static IEnumerable<RecurrenceTestCase> ParseTestCaseFile(string fileContent)
    {
        RecurrenceTestCase? current = null;

        var rd = new StringReader(fileContent);
        var lineNo = 0;

        for (var line = rd.ReadLine(); line != null; line = rd.ReadLine())
        {
            lineNo++;

            if (string.IsNullOrEmpty(line))
            {
                if (current != null)
                {
                    yield return current;
                    current = null;
                }
                continue;
            }

            if (line.StartsWith("#"))
                continue;

            current = current ?? new RecurrenceTestCase();

            var m = Regex.Match(line, @"^(?<h>[A-Z-]+):(?<v>.*)$");
            if (!m.Success)
                continue;

            var hdr = m.Groups["h"].Value;
            var val = m.Groups["v"].Value;

            switch (hdr)
            {
                case "RRULE":
                    current.RRule = val;
                    current.LineNumber = lineNo;
                    break;

                case "DTSTART":
                    current.DtStart = new CalDateTime(val, "UTC");
                    break;

                case "DURATION":
                    current.Duration = Duration.Parse(val);
                    break;

                case "START-AT":
                    current.StartAt = new CalDateTime(val, "UTC");
                    break;

                case "INSTANCES":
                    current.Instances = val.Split(',').Select(dt => new CalDateTime(dt, "UTC")).ToList();
                    break;

                case "EXCEPTION":
                    current.Exception = val;
                    current.ExceptionStep ??= RecurrenceTestExceptionStep.Construction;
                    break;

                case "EXCEPTION-STEP":
                    current.ExceptionStep = (RecurrenceTestExceptionStep)Enum.Parse(typeof(RecurrenceTestExceptionStep), val);
                    break;
            }
        }

        if (current != null)
            yield return current;
    }

    private static IEnumerable<RecurrenceTestCase> TestLibicalTestCasesSource
        => ParseTestCaseFile(IcsFiles.LibicalIcalrecurTest);

    [TestCaseSource(nameof(TestLibicalTestCasesSource))]
    public void TestLibicalTestCases(RecurrenceTestCase testCase)
    {
        ExecuteRecurrenceTestCase(testCase);
    }

    private static IEnumerable<RecurrenceTestCase> TestFileBasedRecurrenceTestCaseSource
        => ParseTestCaseFile(IcsFiles.RecurrrenceTestCases);

    [TestCaseSource(nameof(TestFileBasedRecurrenceTestCaseSource))]
    public void TestFileBasedRecurrenceTestCase(RecurrenceTestCase testCase)
        => ExecuteRecurrenceTestCase(testCase);

    public void ExecuteRecurrenceTestCase(RecurrenceTestCase testCase)
    {
        var cal = new Calendar();

        var evt = cal.Create<CalendarEvent>();
        evt.Summary = "Event summary";

        // Start at midnight, UTC time
        evt.Start = testCase.DtStart!;
        evt.Duration = testCase.Duration;

        Type LoadType(string name) =>
            Type.GetType(name) ?? typeof(Calendar).Assembly.GetType(name) ?? throw new Exception();

        var exceptionType = (testCase.Exception == null) ? null : LoadType(testCase.Exception);
        IConstraint throwsConstraint = (exceptionType == null) ? Throws.InstanceOf(typeof(Exception)) : Throws.InstanceOf(exceptionType);

        RecurrencePattern GetPattern() => new RecurrencePattern(testCase.RRule!);

        if (testCase.ExceptionStep == RecurrenceTestExceptionStep.Construction)
        {
            Assert.That(() => GetPattern(), throwsConstraint);
            return;
        }

        evt.RecurrenceRules.Add(GetPattern());

        IEnumerable<Occurrence> GetOccurrences() => evt.GetOccurrences(testCase.StartAt ?? null);

        if (testCase.ExceptionStep == RecurrenceTestExceptionStep.GetOccurrenceInvocation)
        {
            Assert.That(() => GetOccurrences(), throwsConstraint);
            return;
        }

        var occurrencesEnumerator = GetOccurrences();

        List<Occurrence> EnumerateOccurrences() => occurrencesEnumerator.ToList();

        if (testCase.ExceptionStep == RecurrenceTestExceptionStep.Enumeration)
        {
            Assert.That(() => EnumerateOccurrences(), throwsConstraint);
            return;
        }

        var occurrences = EnumerateOccurrences();

        var startDates = occurrences.Select(x => x.Period.StartTime).ToList();
        Assert.That(startDates, Is.EqualTo(testCase.Instances));
    }

    [Test]
    // Reproducer from https://github.com/ical-org/ical.net/issues/629
    // Note: The original reproducer used DateTime.Parse(yyyy-MM-dd) to create a CalDateTime
    // which resolves to DateTime, with a Time part of 00:00:00.0000000.
    // It's important to always or never use the Time part in this unit test
    public void ShouldCreateARecurringYearlyEvent()
    {
        var springAdminEvent = new CalendarEvent
        {
            Start = new CalDateTime(2024, 04, 15),
            End = new CalDateTime(2024, 04, 15),
            RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Yearly, 1) },
        };

        var calendar = new Calendar();
        calendar.Events.Add(springAdminEvent);
        var searchStart = new CalDateTime(2024, 04, 15);
        var searchEnd = new CalDateTime(2050, 05, 31);
        var occurrences = calendar.GetOccurrences(searchStart).TakeBefore(searchEnd);
        Assert.That(occurrences.Count, Is.EqualTo(27));

        springAdminEvent.Start = new CalDateTime(2024, 04, 16);
        springAdminEvent.End = new CalDateTime(2024, 04, 16);
        springAdminEvent.RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Yearly, 1) };

        searchStart = new CalDateTime(2024, 04, 16);
        searchEnd = new CalDateTime(2050, 05, 31);
        occurrences = calendar.GetOccurrences(searchStart).TakeBefore(searchEnd);

        // occurrences are 26 here, omitting 4/16/2024
        Assert.That(occurrences.Count, Is.EqualTo(27));
    }

    [Test]
    public void GetOccurrenceShouldExcludeDtEndFloating()
    {
        var ical = """
                   BEGIN:VCALENDAR
                   VERSION:2.0
                   PRODID:-//github.com/ical-org/ical.net//NONSGML ical.net 5.0//EN
                   BEGIN:VEVENT
                   UID:123456
                   DTSTAMP:20240630T000000Z
                   DTSTART;VALUE=DATE:20241001
                   DTEND;VALUE=DATE:20241202
                   SUMMARY:Don't include the end date of this event
                   END:VEVENT
                   END:VCALENDAR
                   """;

        var calendar = Calendar.Load(ical)!;
        // Set start date for occurrences to search to the end date of the event
        var occurrences = calendar.GetOccurrences(new CalDateTime(2024, 12, 2)).TakeBefore(new CalDateTime(2024, 12, 3));

        Assert.That(occurrences, Is.Empty);
    }

    [Test]
    public void TestGetOccurrenceIndefinite()
    {
        var ical = """
            BEGIN:VCALENDAR
            VERSION:2.0
            BEGIN:VEVENT
            DTSTART:20241130
            RRULE:FREQ=DAILY;BYDAY=MO,TU,WE,TH,FR,SA
            EXRULE:FREQ=DAILY;INTERVAL=3
            RDATE:20241201
            EXDATE:20241202
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(ical)!;

        // Although the occurrences are unbounded, we can still call GetOccurrences without
        // specifying bounds, because the instances are only generated on enumeration.
        var occurrences = calendar.GetOccurrences();

        var instances = occurrences.Take(100).ToList();

        Assert.That(instances.Count(), Is.EqualTo(100));
    }

    [Test, Category("Recurrence")]
    [TestCase(null)]
    [TestCase("UTC")]
    [TestCase("Europe/Vienna")]
    [TestCase("America/New_York")]
    public void TestDtStartTimezone(string? tzId)
    {
        var icalText = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            UID:ignore
            DTSTAMP:20180613T154237Z
            DTSTART;TZID=Europe/Vienna:20180612T180000
            DTEND;TZID=Europe/Vienna:20180612T181000
            RRULE:FREQ=HOURLY
            END:VEVENT
            END:VCALENDAR
            """;

        var cal = Calendar.Load(icalText)!;
        var evt = cal.Events.First();
        var ev = new EventEvaluator(evt);
        var occurrences = ev.Evaluate(evt.DtStart!, evt.DtStart!.ToTimeZone(tzId), null).TakeBefore(evt.DtStart.AddMinutes(61).ToTimeZone(tzId));
        var occurrencesStartTimes = occurrences.Select(x => x.StartTime).Take(2).ToList();

        var expectedStartTimes = new[]
        {
            new CalDateTime(2018, 06, 12, 18, 0, 0, "Europe/Vienna"),
            new CalDateTime(2018, 06, 12, 19, 0, 0, "Europe/Vienna"),
        };

        Assert.That(expectedStartTimes.SequenceEqual(occurrencesStartTimes), Is.True);
    }

    // Between 00:59 and 00:00 there's a gap of 1380 minutes, which is 690 increments.
    private const string TestMaxIncrementCountWithGaps = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART:20250305T000000
            RRULE:FREQ=MINUTELY;INTERVAL=2;BYHOUR=0;COUNT=100
            END:VEVENT
            END:VCALENDAR
            """;

    private const string TestMaxIncrementCountWithoutGaps = """
            BEGIN:VCALENDAR
            BEGIN:VEVENT
            DTSTART:20250305T000000
            RRULE:FREQ=DAILY;INTERVAL=10;COUNT=100
            END:VEVENT
            END:VCALENDAR
            """;

    [Test]
    [TestCase(null, TestMaxIncrementCountWithGaps, false)]
    [TestCase(0, TestMaxIncrementCountWithGaps, true)]
    [TestCase(1, TestMaxIncrementCountWithGaps, true)]
    [TestCase(689, TestMaxIncrementCountWithGaps, true)]
    [TestCase(690, TestMaxIncrementCountWithGaps, false)]
    [TestCase(0, TestMaxIncrementCountWithoutGaps, false)]
    [TestCase(1, TestMaxIncrementCountWithoutGaps, false)]
    public void TestMaxIncrementCount(int? limit, string ical, bool expectException)
    {
        var cal = Calendar.Load(ical)!;

        var options = new EvaluationOptions
        {
            MaxUnmatchedIncrementsLimit = limit,
        };

        IResolveConstraint constraint =
            expectException
            ? Throws.Exception.TypeOf<EvaluationLimitExceededException>()
            : Throws.Nothing;

        Assert.That(() => cal.GetOccurrences(options: options).ToList(), constraint);
    }

    [TestCase("FREQ=DAILY;INTERVAL=2;UNTIL=20250430T000000Z")]
    [TestCase("UNTIL=20250430T000000Z;FREQ=DAILY;INTERVAL=2")]
    [TestCase("INTERVAL=2;FREQ=DAILY;UNTIL=20250430T000000Z")]
    [TestCase("INTERVAL=2;UNTIL=20250430T000000Z;FREQ=DAILY")]
    public void Recurrence_RRULE_Properties_ShouldBeDeserialized_In_Any_Order(string rRule)
    {
        var serializer = new RecurrencePatternSerializer();
        var recurrencePattern = serializer.Deserialize(new StringReader(rRule)) as RecurrencePattern;

        Assert.Multiple(() =>
        {
            Assert.That(recurrencePattern, Is.Not.Null);
            Assert.That(recurrencePattern?.Until, Is.Not.Null);
            Assert.That(recurrencePattern?.Until, Is.EqualTo(new CalDateTime(2025, 4, 30, 0, 0, 0, CalDateTime.UtcTzId)));
            Assert.That(recurrencePattern?.Frequency, Is.EqualTo(FrequencyType.Daily));
            Assert.That(recurrencePattern?.Interval, Is.EqualTo(2));
        });
    }

    [Test]
    public void Recurrence_RRULE_Without_Freq_Should_Throw()
    {
        var serializer = new RecurrencePatternSerializer();

        Assert.That(() => serializer.Deserialize(new StringReader("INTERVAL=2;UNTIL=20250430T000000Z")), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Recurrence_RRULE_With_Freq_Undefined_Should_Throw()
    {
        var serializer = new RecurrencePatternSerializer();

        Assert.That(() => serializer.Deserialize(new StringReader("FREQ=UNDEFINED;INTERVAL=2;UNTIL=20250430T000000Z")), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Recurrence_RRULE_With_Unsupported_Part_Should_Throw()
    {
        var serializer = new RecurrencePatternSerializer();

        Assert.That(() => serializer.Deserialize(new StringReader("FREQ=DAILY;INTERVAL=2;FAILING=0")), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Preceding_Appended_and_duplicate_Semicolons_Should_Be_Ignored()
    {
        var serializer = new RecurrencePatternSerializer();

        var recurrencePattern = serializer.Deserialize(new StringReader(";FREQ=DAILY;INTERVAL=2;UNTIL=20250430T000000Z")) as RecurrencePattern;
        Assert.Multiple(() =>
        {
            Assert.That(recurrencePattern, Is.Not.Null);
            Assert.That(recurrencePattern?.Until, Is.EqualTo(new CalDateTime(2025, 4, 30, 0, 0, 0, CalDateTime.UtcTzId)));
            Assert.That(recurrencePattern?.Frequency, Is.EqualTo(FrequencyType.Daily));
            Assert.That(recurrencePattern?.Interval, Is.EqualTo(2));
        });
    }

    [Test]
    public void Disallowed_Recurrence_RangeChecks_Should_Throw()
    {
        var serializer = new RecurrencePatternSerializer();
        Assert.Multiple(() =>
        {
            Assert.That(() => serializer.CheckMutuallyExclusive("a", "b", 1, CalDateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => serializer.CheckRange("a", 0, 1, 2, false), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => serializer.CheckRange("a", (int?) 0, 1, 2, false), Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }
}
