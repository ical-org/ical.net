﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class FreeBusyTest
{
    /// <summary>
    /// Ensures that GetFreeBusyStatus() return the correct status.
    /// </summary>
    [Test, Category("FreeBusy")]
    public void GetFreeBusyStatus1()
    {
        Calendar cal = new Calendar();

        CalendarEvent evt = cal.Create<CalendarEvent>();
        evt.Summary = "Test event";
        evt.Start = new CalDateTime(2010, 10, 1, 8, 0, 0);
        evt.End = new CalDateTime(2010, 10, 1, 9, 0, 0);

        var freeBusy = cal.GetFreeBusy(new CalDateTime(2010, 10, 1, 0, 0, 0), new CalDateTime(2010, 10, 7, 11, 59, 59));
        Assert.Multiple(() =>
        {
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 7, 59, 59)), Is.EqualTo(FreeBusyStatus.Free));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 8, 0, 0)), Is.EqualTo(FreeBusyStatus.Busy));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 8, 59, 59)), Is.EqualTo(FreeBusyStatus.Busy));
            Assert.That(freeBusy.GetFreeBusyStatus(new CalDateTime(2010, 10, 1, 9, 0, 0)), Is.EqualTo(FreeBusyStatus.Free));
        });
    }
}