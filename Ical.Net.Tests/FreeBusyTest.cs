//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
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
            new CalDateTime(2025, 10, 1, 0, 0, 0),
            new CalDateTime(2025, 10, 7, 11, 59, 59))!;

        Assert.Multiple(() =>
        {
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 7, 59, 59)),
                Is.EqualTo(FreeBusyStatus.Free));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 8, 0, 0)),
                Is.EqualTo(FreeBusyStatus.Busy));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 8, 59, 59)),
                Is.EqualTo(FreeBusyStatus.Busy));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2025, 10, 1, 9, 0, 0)),
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
            new CalDateTime(2025, 6, 1, 0, 0, 0),
            new CalDateTime(2025, 6, 7, 0, 0, 0))!;

        Assert.Multiple(() =>
        {
            // Period completely before the event (ends when event starts)
            var periodBefore = new Period(new CalDateTime(2025, 6, 1, 7, 0, 0),
                                          new CalDateTime(2025, 6, 1, 8, 0, 0));
            Assert.That(freeBusy.GetFreeBusyStatus(periodBefore),
                Is.EqualTo(FreeBusyStatus.Free));

            // Period entirely within the event (should be busy)
            var periodDuring = new Period(new CalDateTime(2025, 6, 1, 8, 0, 0),
                                          new CalDateTime(2025, 6, 1, 8, 59, 59));
            Assert.That(freeBusy.GetFreeBusyStatus(periodDuring),
                Is.EqualTo(FreeBusyStatus.Busy));

            // Period spanning before and into the event (should be busy)
            var periodSpanningStart = new Period(new CalDateTime(2025, 6, 1, 7, 59, 59),
                                                 new CalDateTime(2025, 6, 1, 8, 30, 0));
            Assert.That(freeBusy.GetFreeBusyStatus(periodSpanningStart),
                Is.EqualTo(FreeBusyStatus.Busy));

            // Period starting at the event's end (should be free)
            var periodAfter = new Period(new CalDateTime(2025, 6, 1, 10, 0, 0),
                                         new CalDateTime(2025, 6, 1, 12, 0, 0));
            Assert.That(freeBusy.GetFreeBusyStatus(periodAfter),
                Is.EqualTo(FreeBusyStatus.Free));
        });
    }
}
