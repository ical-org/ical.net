//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class ComponentTest
{
    [Test, Category("Components")]
    public void UniqueComponent1()
    {
        var iCal = new Calendar();
        var evt = iCal.Create<CalendarEvent>();

        Assert.That(evt.Uid, Is.Not.Null);
        Assert.That(evt.Created, Is.Null); // We don't want this to be set automatically
        Assert.That(evt.DtStamp, Is.Not.Null);
    }

    [Test, Category("Components")]
    public void ChangeCalDateTimeValue()
    {
        var e = new CalendarEvent
        {
            Start = new CalDateTime(2017, 11, 22, 11, 00, 01),
            End = new CalDateTime(2017, 11, 22, 11, 30, 01),
        };

        var firstStartAsUtc = e.Start.AsUtc;
        var firstEndAsUtc = e.End.AsUtc;

        e.Start.Value = new DateTime(2017, 11, 22, 11, 30, 01);
        e.End.Value = new DateTime(2017, 11, 22, 12, 00, 01);

        var secondStartAsUtc = e.Start.AsUtc;
        var secondEndAsUtc = e.End.AsUtc;

        Assert.Multiple(() =>
        {
            Assert.That(secondStartAsUtc, Is.Not.EqualTo(firstStartAsUtc));
            Assert.That(secondEndAsUtc, Is.Not.EqualTo(firstEndAsUtc));
        });
    }
}