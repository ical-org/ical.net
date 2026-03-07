//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using Ical.Net.DataTypes;
using NodaTime;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class ProgramTest
{
    [Test]
    public void LoadAndDisplayCalendar()
    {
        // The following code loads and displays an iCalendar
        // with US Holidays for 2006.
        var iCal = Calendar.Load(IcsFiles.UsHolidays);
        Assert.That(iCal, Is.Not.Null, "iCalendar did not load.");
    }

    private const string _tzid = "US-Eastern";

    public static void TestCal(Calendar cal)
    {
        Assert.That(cal, Is.Not.Null, "The iCalendar was not loaded");
        if (cal.Events.Count > 0)
            Assert.That(cal.Events.Count == 1, Is.True, "Calendar should contain 1 event; however, the iCalendar loaded " + cal.Events.Count + " events");
        else if (cal.Todos.Count > 0)
            Assert.That(cal.Todos.Count == 1, Is.True, "Calendar should contain 1 todo; however, the iCalendar loaded " + cal.Todos.Count + " todos");
    }

    /// <summary>
    /// The following test is an aggregate of MonthlyCountByMonthDay3() and MonthlyByDay1() in the
    /// </summary>
    [Test]
    public void Merge1()
    {
        var iCal1 = Calendar.Load(IcsFiles.MonthlyCountByMonthDay3)!;
        var iCal2 = Calendar.Load(IcsFiles.MonthlyByDay1)!;

        // Change the UID of the 2nd event to make sure it's different
        iCal2.Events[iCal1.Events[0]!.Uid!].Uid = "1234567890";
        iCal1.MergeWith(iCal2);

        var evt1 = iCal1.Events.First();
        var evt2 = iCal1.Events.Skip(1).First();

        var tz = iCal1.TimeZoneProvider[_tzid];

        // Get occurrences for the first event
        var occurrences = evt1.GetOccurrences(
            new LocalDate(1996, 1, 1).AtStartOfDayInZone(tz))
            .TakeWhileBefore(new LocalDate(2000, 1, 1).AtStartOfDayInZone(tz).ToInstant()).ToList();

        var dateTimes = new LocalDate[]
        {
            new(1997, 9, 10),
            new(1997, 9, 11),
            new(1997, 9, 12),
            new(1997, 9, 13),
            new(1997, 9, 14),
            new(1997, 9, 15),
            new(1999, 3, 10),
            new(1999, 3, 11),
            new(1999, 3, 12),
            new(1999, 3, 13),
        }.Select(x => x.At(new LocalTime(9, 0)).InZoneStrictly(tz)).ToArray();

        for (var i = 0; i < dateTimes.Length; i++)
        {
            var dt = dateTimes[i];
            var start = occurrences[i].Start;
            Assert.That(start, Is.EqualTo(dt));
        }

        Assert.That(occurrences.Count == dateTimes.Length, Is.True, "There should be exactly " + dateTimes.Length + " occurrences; there were " + occurrences.Count);

        // Get occurrences for the 2nd event
        occurrences = evt2.GetOccurrences(
            new LocalDate(1996, 1, 1).AtStartOfDayInZone(tz))
            .TakeWhileBefore(new LocalDate(1998, 4, 1).AtStartOfDayInZone(tz).ToInstant()).ToList();

        var dateTimes1 = new LocalDate[]
        {
            new (1997, 9, 2),
            new (1997, 9, 9),
            new (1997, 9, 16),
            new (1997, 9, 23),
            new (1997, 9, 30),
            new (1997, 11, 4),
            new (1997, 11, 11),
            new (1997, 11, 18),
            new (1997, 11, 25),
            new (1998, 1, 6),
            new (1998, 1, 13),
            new (1998, 1, 20),
            new (1998, 1, 27),
            new (1998, 3, 3),
            new (1998, 3, 10),
            new (1998, 3, 17),
            new (1998, 3, 24),
            new (1998, 3, 31)
        }.Select(x => x.At(new LocalTime(9, 0)).InZoneStrictly(tz)).ToArray();

        using (Assert.EnterMultipleScope())
        {
            for (var i = 0; i < dateTimes1.Length; i++)
            {
                var dt = dateTimes1[i];
                var start = occurrences[i].Start;
                {
                    Assert.That(start, Is.EqualTo(dt));
                }
            }
        }

        Assert.That(occurrences, Has.Count.EqualTo(dateTimes1.Length), "There should be exactly " + dateTimes1.Length + " occurrences; there were " + occurrences.Count);
    }

    [Test]
    public void SystemTimeZone3()
    {
        // Per Jon Udell's test, we should be able to get all
        // system time zones on the machine and ensure they
        // are properly translated.
        var zones = TimeZoneInfo.GetSystemTimeZones();
        foreach (var zone in zones)
        {
            Assert.That(() =>
            {
                TimeZoneInfo.FindSystemTimeZoneById(zone.Id);
            }, Throws.Nothing, "Time zone should be found.");
        }
    }
}
