//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Text;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class DurationTests
{
    [Test]
    public void DurationWithMixSignsThrows()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.Throws<ArgumentException>(() => new Duration(-3, 3));
            Assert.Throws<ArgumentException>(() => new Duration(hours: 3, minutes: -3));
            Assert.Throws<ArgumentException>(() => new Duration(3, seconds: -3));
        }
    }

    [Test]
    public void DurationFromTimeSpanOnlyHasTime()
    {
        var t = new TimeSpan(10, 4, 5, 6);
        var d = Duration.FromTimeSpanExact(t);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(d.HasDate, Is.False);
            Assert.That(d.Hours, Is.EqualTo(244));
        }
    }

    [Test]
    public void DurationFromAndToTimeSpanIsEqual()
    {
        var a = new TimeSpan(10, 4, 5, 6);
        var b = Duration.FromTimeSpanExact(a).ToTimeSpanUnspecified();

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void DurationNegatesValues()
    {
        var a = new Duration(1, 2, 3, 4, 5);

        var b = -a;

        Assert.That(b.ToString(), Is.EqualTo("-P1W2DT3H4M5S"));
    }

    [Test]
    public void DurationNegatesNullValues()
    {
        var a = new Duration(1, 2, seconds: 3);

        var b = -a;

        Assert.That(b.ToString(), Is.EqualTo("-P1W2DT3S"));
    }

    [Test]
    public void DurationZeroAndNullAreEqual()
    {
        Assert.That(Duration.FromSeconds(0), Is.EqualTo(Duration.Zero));
    }
}
