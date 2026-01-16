//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.CalendarComponents;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class ComponentTest
{
    [Test, Category("Components")]
    public void UniqueComponent1()
    {
        var iCal = new Calendar();
        var evt = iCal.Create<CalendarEvent>();

        Assert.Multiple(() =>
        {
            Assert.That(evt.Uid, Is.Not.Null);
            Assert.That(evt.Created, Is.Null); // We don't want this to be set automatically
            Assert.That(evt.DtStamp, Is.Not.Null);
        });
    }
}
