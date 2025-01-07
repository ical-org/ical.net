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
        var periodWithDuration = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), Duration.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(period.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(period.EndTime, Is.Null);
            Assert.That(period.Duration, Is.Null);
            
            Assert.That(periodWithEndTime.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(periodWithEndTime.EffectiveEndTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York")));
            Assert.That(periodWithEndTime.EffectiveDuration, Is.EqualTo(Duration.FromHours(1)));

            Assert.That(periodWithDuration.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(periodWithDuration.EffectiveEndTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York")));
            Assert.That(periodWithDuration.EffectiveDuration, Is.EqualTo(Duration.FromHours(1)));

            Assert.That(Period.Create(period.StartTime).Duration, Is.Null);
            Assert.That(Period.Create(period.StartTime).EffectiveDuration, Is.Null);
        });
    }

    [Test]
    public void CreatePeriodWithInvalidArgumentsShouldThrow()
    {
        Assert.Multiple(() =>
        {
            // StartTime is null
            Assert.Throws<ArgumentNullException>(() => _ = new Period(null!));

            // EndTime is before StartTime
            Assert.Throws<ArgumentException>(() => _ = new Period(
                new CalDateTime(2025, 1, 2, 0, 0, 0, "America/New_York"),
                new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));

            // Duration is negative
            Assert.Throws<ArgumentException>(() =>
                _ = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), Duration.FromHours(-1)));

            // Timezones are different
            Assert.Throws<ArgumentException>(() => _ = new Period(
                new CalDateTime(2025, 1, 2, 0, 0, 0, "America/New_York"),
                new CalDateTime(2025, 1, 1, 0, 0, 0, "Europe/Vienna")));

            // StartTime is date-only while EndTime has time
            Assert.Throws<ArgumentException>(() => _ = new Period(new CalDateTime(2025, 1, 2, 0, 0, 0),
                new CalDateTime(2025, 1, 1)));
        });
    }

    [Test]
    public void Timezones_StartTime_EndTime_MustBeEqual()
    {
        var periods = new[]
        {
            (new CalDateTime(2025, 1, 1, 0, 0, 0, "Europe/Vienna"),
                new CalDateTime(2025, 1, 1, 0, 0, 0, CalDateTime.UtcTzId)),
            (new CalDateTime(2025, 1, 1, 0, 0, 0, null),
                new CalDateTime(2025, 1, 1, 0, 0, 0, CalDateTime.UtcTzId)),
            (new CalDateTime(2025, 1, 1, 0, 0, 0, CalDateTime.UtcTzId),
                new CalDateTime(2025, 1, 1, 0, 0, 0, null))
        };

        Assert.Multiple(() =>
        {
            foreach (var p in periods)
            {
                Assert.Throws<ArgumentException>(() => _ = new Period(p.Item1, p.Item2));
            }
        });
    }

    [Test]
    public void CollidesWithPeriod()
    {
        var period1 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1));
        var period2 = new Period(new CalDateTime(2025, 1, 1, 0, 30, 0), Duration.FromHours(1));
        var period3 = new Period(new CalDateTime(2025, 1, 1, 1, 30, 0), Duration.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(period1.CollidesWith(period2), Is.True);
            Assert.That(period1.CollidesWith(period3), Is.False);
            Assert.That(period2.CollidesWith(period3), Is.True);
        });
    }
}
