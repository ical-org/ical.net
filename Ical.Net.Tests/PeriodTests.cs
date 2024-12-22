//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class PeriodTests
{
    [Test]
    public void CreatePeriodWithArguments()
    {
        var period = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"));
        var periodWithEndTime = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York"));
        var periodWithDuration = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), new TimeSpan(1, 0, 0));

        Assert.Multiple(() =>
        {
            Assert.That(period.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(period.EndTime, Is.Null);
            Assert.That(period.Duration, Is.Null);
            
            Assert.That(periodWithEndTime.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(periodWithEndTime.EndTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York")));
            Assert.That(periodWithEndTime.Duration, Is.EqualTo(new TimeSpan(1, 0, 0)));

            Assert.That(periodWithDuration.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(periodWithDuration.EndTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York")));
            Assert.That(periodWithDuration.Duration, Is.EqualTo(new TimeSpan(1, 0, 0)));
        });
    }

    [Test]
    public void CreatePeriodWithInvalidArgumentsShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => _ = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"),
            new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
        Assert.Throws<ArgumentException>(() =>
            _ = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), new TimeSpan(-1, 0, 0)));
    }
    [Test]
    public void SetAndGetStartTime()
    {
        var period = new Period(new CalDateTime(DateTime.UtcNow));
        var startTime = new CalDateTime(2025, 1, 1, 0, 0, 0);
        period.StartTime = startTime;

        Assert.That(period.StartTime, Is.EqualTo(startTime));
    }

    [Test]
    public void SetEndTime_GetDuration()
    {
        var period = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0));
        var endTime = new CalDateTime(2025, 1, 31, 0, 0, 0);
        period.EndTime = endTime;

        Assert.That(period.EndTime, Is.EqualTo(endTime));
        Assert.That(period.GetOriginalValues().EndTime, Is.EqualTo(endTime));
        Assert.That(period.GetOriginalValues().Duration, Is.Null);
        Assert.That(period.Duration, Is.EqualTo(new TimeSpan(30, 0, 0, 0)));
    }

    [Test]
    public void SetDuration_GetEndTime()
    {
        var period = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0));
        var duration = new TimeSpan(1, 0, 0);
        period.Duration = duration;

        Assert.That(period.Duration, Is.EqualTo(duration));
        Assert.That(period.GetOriginalValues().Duration, Is.EqualTo(duration));
        Assert.That(period.GetOriginalValues().EndTime, Is.Null);
        Assert.That(period.EndTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0)));
    }

    [Test]
    public void CollidesWithPeriod()
    {
        var period1 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), new TimeSpan(1, 0, 0));
        var period2 = new Period(new CalDateTime(2025, 1, 1, 0, 30, 0), new TimeSpan(1, 0, 0));
        var period3 = new Period(new CalDateTime(2025, 1, 1, 1, 30, 0), new TimeSpan(1, 0, 0));

        Assert.Multiple(() =>
        {
            Assert.That(period1.CollidesWith(period2), Is.True);
            Assert.That(period1.CollidesWith(period3), Is.False);
            Assert.That(period2.CollidesWith(period3), Is.True);
        });
    }
}
