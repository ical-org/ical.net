//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class FreeBusyTest
{
    /// <summary>
    /// Ensures that GetFreeBusyStatus() return the correct status for date/time arguments.
    /// </summary>
    [Test, Category("FreeBusy")]
    public void GetFreeBusyStatusByDateTime()
    {
        var cal = new Calendar();

        var evt = cal.Create<CalendarEvent>();
        evt.Summary = "Test event";
        evt.Start = new CalDateTime(2025, 10, 1, 8, 0, 0);
        evt.End = new CalDateTime(2025, 10, 1, 9, 0, 0);

        var freeBusy = cal.GetFreeBusy(
            DateUtil.GetZone("America/New_York"),
            new CalDateTime(2025, 10, 1, 0, 0, 0),
            new CalDateTime(2025, 10, 7, 11, 59, 59))!;

        Assert.Multiple(() =>
        {
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 7, 59, 59).ToTimeZone("America/New_York")),
                Is.EqualTo(FreeBusyStatus.Free));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 8, 0, 0).ToTimeZone("America/New_York")),
                Is.EqualTo(FreeBusyStatus.Busy));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 8, 59, 59).ToTimeZone("America/New_York")),
                Is.EqualTo(FreeBusyStatus.Busy));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 9, 0, 0).ToTimeZone("America/New_York")),
                Is.EqualTo(FreeBusyStatus.Free));
        });
    }

    [Test, Category("FreeBusy")]
    public void GetFreeBusyStatusByPeriod()
    {
        var cal = new Calendar();

        var evt = cal.Create<CalendarEvent>();
        evt.Summary = "Test event";
        evt.Start = new CalDateTime(2025, 6, 1, 8, 0, 0);
        evt.End = new CalDateTime(2025, 6, 1, 10, 0, 0);

        var freeBusy = cal.GetFreeBusy(
            DateUtil.GetZone("UTC"),
            new CalDateTime(2025, 6, 1, 0, 0, 0, "UTC"),
            new CalDateTime(2025, 6, 7, 0, 0, 0, "UTC"))!;

        Assert.Multiple(() =>
        {
            // Period completely before the event (ends when event starts)
            var periodBefore = new Period(new CalDateTime(2025, 6, 1, 7, 0, 0, "UTC"),
                                          new CalDateTime(2025, 6, 1, 8, 0, 0, "UTC"));
            Assert.That(freeBusy.GetFreeBusyStatus(periodBefore),
                Is.EqualTo(FreeBusyStatus.Free));

            // Period entirely within the event (should be busy)
            var periodDuring = new Period(new CalDateTime(2025, 6, 1, 8, 0, 0, "UTC"),
                                          new CalDateTime(2025, 6, 1, 8, 59, 59, "UTC"));
            Assert.That(freeBusy.GetFreeBusyStatus(periodDuring),
                Is.EqualTo(FreeBusyStatus.Busy));

            // Period spanning before and into the event (should be busy)
            var periodSpanningStart = new Period(new CalDateTime(2025, 6, 1, 7, 59, 59, "UTC"),
                                                 new CalDateTime(2025, 6, 1, 8, 30, 0, "UTC"));
            Assert.That(freeBusy.GetFreeBusyStatus(periodSpanningStart),
                Is.EqualTo(FreeBusyStatus.Busy));

            // Period starting at the event's end (should be free)
            var periodAfter = new Period(new CalDateTime(2025, 6, 1, 10, 0, 0, "UTC"),
                                         new CalDateTime(2025, 6, 1, 12, 0, 0, "UTC"));
            Assert.That(freeBusy.GetFreeBusyStatus(periodAfter),
                Is.EqualTo(FreeBusyStatus.Free));
        });
    }

    [Test]
    public void Contains_Tests()
    {
        var start = NodaTime.Instant.FromUtc(2025, 1, 1, 0, 0);
        var dtBefore = start.InUtc().PlusSeconds(-1).ToInstant();
        var dtAtStart = start;
        var dtMid = start.InUtc().PlusMinutes(30).ToInstant();
        var dtAtEnd = start.InUtc().PlusHours(1).ToInstant();
        var dtAfter = start.InUtc().PlusHours(1).PlusSeconds(1).ToInstant();

        // Period with duration: effective end time = start + 1 hour (exclusive)
        var periodWithDuration = new FreeBusyEntry(new(new(start), Duration.FromHours(1)), FreeBusyStatus.Free);
        Assert.Multiple(() =>
        {
            Assert.That(periodWithDuration.Contains(null), Is.False, "Contains should return false for null dt.");
            Assert.That(periodWithDuration.Contains(dtBefore), Is.False, "Contains should return false if dt is before start.");
            Assert.That(periodWithDuration.Contains(dtAtStart), Is.True, "Contains should return true for dt equal to start.");
            Assert.That(periodWithDuration.Contains(dtMid), Is.True, "Contains should return true for dt in the middle.");
            Assert.That(periodWithDuration.Contains(dtAtEnd), Is.False, "Contains should return false for dt equal to effective end (exclusive).");
            Assert.That(periodWithDuration.Contains(dtAfter), Is.False, "Contains should return false for dt after effective end.");
        });
    }


    [Test, TestCaseSource(nameof(CollidesWithPeriodTestCases))]
    public void CollidesWithPeriod(Period period1, Period? period2, bool expected)
    {
        var f1 = new FreeBusyEntry(period1, FreeBusyStatus.Free);
        var f2 = period2 is null ? null : new FreeBusyEntry(period2, FreeBusyStatus.Free);

        Assert.Multiple(() =>
        {
            Assert.That(f1.CollidesWith(f2), Is.EqualTo(expected));

            Assert.That(f2?.CollidesWith(f1) == true, Is.EqualTo(expected));
        });
    }

    private static IEnumerable<TestCaseData> CollidesWithPeriodTestCases
    {
        get
        {
            // Overlapping periods
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(2)),
                new Period(new CalDateTime(2025, 1, 1, 1, 0, 0, "UTC"), Duration.FromHours(2)),
                true
            ).SetName("Overlap: period1 and period2 overlap by 1 hour");

            // Contiguous periods (end of one is start of another, exclusive end)
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(1)),
                new Period(new CalDateTime(2025, 1, 1, 1, 0, 0, "UTC"), Duration.FromHours(1)),
                false
            ).SetName("Contiguous: period1 ends when period2 starts (no overlap)");

            // One inside another
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(4)),
                new Period(new CalDateTime(2025, 1, 1, 1, 0, 0, "UTC"), Duration.FromHours(1)),
                true
            ).SetName("Contained: period2 is inside period1");

            // Identical periods
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(2)),
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(2)),
                true
            ).SetName("Identical: periods are exactly the same");

            // Non-overlapping periods
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(1)),
                new Period(new CalDateTime(2025, 1, 1, 2, 0, 0, "UTC"), Duration.FromHours(1)),
                false
            ).SetName("NoOverlap: periods are completely separate");

            // This Duration is zero (point in time)
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.Zero),
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(1)),
                false
            ).SetName("NoOverlap: this duration is zero");

            // Other Duration is zero (point in time)
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(1)),
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.Zero),
                false
            ).SetName("NoOverlap: other duration is zero");

            // other period is null
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC"), Duration.FromHours(1)),
                null,
                false
            ).SetName("NoOverlap: other period is null");

        }
    }

    [Test]
    public void PeriodCollidesWith_WhenNoDurationShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => _ = new FreeBusyEntry(new(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC")), FreeBusyStatus.Free));
    }
}
