//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
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
        iCal2.Events[iCal1.Events[0].Uid!].Uid = "1234567890";
        iCal1.MergeWith(iCal2);

        var evt1 = iCal1.Events.First();
        var evt2 = iCal1.Events.Skip(1).First();

        // Get occurrences for the first event
        var occurrences = evt1.GetOccurrences(
            new CalDateTime(1996, 1, 1))
            .TakeBefore(new CalDateTime(2000, 1, 1)).ToList();

        var dateTimes = new[]
        {
            new CalDateTime(1997, 9, 10, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 11, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 13, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 14, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid),
            new CalDateTime(1999, 3, 10, 9, 0, 0, _tzid),
            new CalDateTime(1999, 3, 11, 9, 0, 0, _tzid),
            new CalDateTime(1999, 3, 12, 9, 0, 0, _tzid),
            new CalDateTime(1999, 3, 13, 9, 0, 0, _tzid)
        };

        var timeZones = new[]
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
        };

        for (var i = 0; i < dateTimes.Length; i++)
        {
            CalDateTime dt = dateTimes[i];
            var start = occurrences[i].Period.StartTime;
            Assert.That(start, Is.EqualTo(dt));

            var expectedZone = DateUtil.GetZone(dt.TimeZoneName);
            var actualZone = DateUtil.GetZone(timeZones[i]);

            //Assert.AreEqual();

            //Normalize the time zones and then compare equality
            Assert.That(actualZone, Is.EqualTo(expectedZone));

            //Assert.IsTrue(dt.TimeZoneName == TimeZones[i], "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
        }

        Assert.That(occurrences.Count == dateTimes.Length, Is.True, "There should be exactly " + dateTimes.Length + " occurrences; there were " + occurrences.Count);

        // Get occurrences for the 2nd event
        occurrences = evt2.GetOccurrences(
            new CalDateTime(1996, 1, 1))
            .TakeBefore(new CalDateTime(1998, 4, 1)).ToList();

        var dateTimes1 = new[]
        {
            new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid),
            new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
            new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid),
            new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid),
            new CalDateTime(1997, 11, 18, 9, 0, 0, _tzid),
            new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid),
            new CalDateTime(1998, 1, 6, 9, 0, 0, _tzid),
            new CalDateTime(1998, 1, 13, 9, 0, 0, _tzid),
            new CalDateTime(1998, 1, 20, 9, 0, 0, _tzid),
            new CalDateTime(1998, 1, 27, 9, 0, 0, _tzid),
            new CalDateTime(1998, 3, 3, 9, 0, 0, _tzid),
            new CalDateTime(1998, 3, 10, 9, 0, 0, _tzid),
            new CalDateTime(1998, 3, 17, 9, 0, 0, _tzid),
            new CalDateTime(1998, 3, 24, 9, 0, 0, _tzid),
            new CalDateTime(1998, 3, 31, 9, 0, 0, _tzid)
        };

        var timeZones1 = new[]
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
        };

        for (var i = 0; i < dateTimes1.Length; i++)
        {
            CalDateTime dt = dateTimes1[i];
            var start = occurrences[i].Period.StartTime;
            Assert.Multiple(() =>
            {
                Assert.That(start, Is.EqualTo(dt));
                Assert.That(dt.TimeZoneName == timeZones1[i], Is.True, "Event " + dt + " should occur in the " + timeZones1[i] + " timezone");
            });
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
