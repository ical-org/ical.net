//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class PeriodTests
{
    [Test]
    public void CreatePeriodWithArguments()
    {
        var period = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"));
        var periodWithEndTime = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"),
            new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York"));
        var periodWithDuration =
            new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), Duration.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(period.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(period.EndTime, Is.Null);
            Assert.That(period.Duration, Is.Null);
            Assert.That(period.EffectiveEndTime, Is.Null);
            Assert.That(period.EffectiveDuration, Is.Null);

            Assert.That(periodWithEndTime.StartTime,
                Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(periodWithEndTime.EffectiveEndTime,
                Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York")));
            Assert.That(periodWithEndTime.EffectiveDuration, Is.EqualTo(Duration.FromHours(1)));

            Assert.That(periodWithDuration.StartTime,
                Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(periodWithDuration.EffectiveEndTime,
                Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York")));
            Assert.That(periodWithDuration.EffectiveDuration, Is.EqualTo(Duration.FromHours(1)));

            Assert.That(Period.Create(period.StartTime).Duration, Is.Null);
            Assert.That(Period.Create(period.StartTime).EffectiveDuration, Is.Null);
        });
    }

    [Test]
    public void CreatePeriodWithInvalidArgumentsShouldThrow()
    {
        // Test with CalDateTime
        Assert.Multiple(() =>
        {
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

        // Test with CalDateTimeZoned
        Assert.Multiple(() =>
        {
            // EndTime is before StartTime
            Assert.Throws<ArgumentException>(() => _ = new Period(
                new CalDateTime(2025, 1, 2, 0, 0, 0, "America/New_York").AsZoned(),
                new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York").AsZoned()));

            // Duration is negative
            Assert.Throws<ArgumentException>(() =>
                _ = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York").AsZoned(), Duration.FromHours(-1)));

            // Timezones are different
            Assert.Throws<ArgumentException>(() => _ = new Period(
                new CalDateTime(2025, 1, 2, 0, 0, 0, "America/New_York").AsZoned(),
                new CalDateTime(2025, 1, 1, 0, 0, 0, "Europe/Vienna").AsZoned()));

            // StartTime is date-only while EndTime has time
            Assert.Throws<ArgumentException>(() => _ = new Period(new CalDateTime(2025, 1, 2, 0, 0, 0).AsZoned(),
                new CalDateTime(2025, 1, 1).AsZoned()));
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
                Assert.Throws<ArgumentException>(() => _ = new Period(p.Item1.AsZoned(), p.Item2.AsZoned()));
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

    [Test]
    public void CreatePeriodWith_StartTime_and_Duration()
    {
        var dt = new CalDateTime(2025, 7, 1, 10, 0, 0, "Europe/London");
        var periodCal = new Period(dt, Duration.FromHours(1));
        var periodZoned = new Period(dt.AsZoned(), Duration.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(periodCal.StartTime, Is.EqualTo(dt));
            Assert.That(periodCal.EffectiveEndTime, Is.EqualTo(new CalDateTime(2025, 7, 1, 11, 0, 0, "Europe/London")));
            Assert.That(periodZoned.StartTime, Is.EqualTo(dt));
            Assert.That(periodZoned.EffectiveEndTime,
                Is.EqualTo(new CalDateTime(2025, 7, 1, 11, 0, 0, "Europe/London")));
        });
    }

    [Test]
    public void CreatePeriodWith_StartTime_and_EndTime()
    {
        var dt1 = new CalDateTime(2025, 7, 1, 10, 0, 0, "Europe/London");
        var dt2 = new CalDateTime(2025, 7, 1, 11, 0, 0, "Europe/London");
        var periodCal = new Period(dt1, dt2);
        var periodZoned = new Period(dt1.AsZoned(), dt2.AsZoned());

        Assert.Multiple(() =>
        {
            Assert.That(periodCal.StartTime, Is.EqualTo(dt1));
            Assert.That(periodCal.EndTime, Is.EqualTo(dt2));
            Assert.That(periodCal.EffectiveEndTime,
                Is.EqualTo(new CalDateTime(2025, 7, 1, 11, 0, 0, "Europe/London")));

            Assert.That(periodZoned.StartTime, Is.EqualTo(dt1));
            Assert.That(periodZoned.EndTime, Is.EqualTo(dt2));
            Assert.That(periodZoned.EffectiveEndTime,
                Is.EqualTo(new CalDateTime(2025, 7, 1, 11, 0, 0, "Europe/London")));
        });
    }

    [Test]
    public void CreatePeriodWith_StartTime_Only()
    {
        var dt = new CalDateTime(2025, 7, 1, 10, 0, 0, "Europe/London");
        var periodCal = new Period(dt);
        var periodZoned = new Period(dt.AsZoned());

        Assert.Multiple(() =>
        {
            Assert.That(periodCal.StartTime, Is.EqualTo(dt));
            Assert.That(periodCal.EndTime, Is.Null);
            Assert.That(periodCal.Duration, Is.Null);
            Assert.That(periodCal.EffectiveEndTime, Is.Null);

            Assert.That(periodZoned.StartTime, Is.EqualTo(dt));
            Assert.That(periodZoned.EndTime, Is.Null);
            Assert.That(periodZoned.Duration, Is.Null);
            Assert.That(periodZoned.EffectiveEndTime, Is.Null);
        });
    }

    [Test]
    public void PeriodCompareToNull_ShouldReturnOne()
    {
        var dt = new CalDateTime(2025, 7, 1, 10, 0, 0, "Europe/London");
        var period = new Period(dt, Duration.FromHours(1));

        Assert.That(period.CompareTo(null), Is.EqualTo(1));
    }

    [Test]
    public void PeriodCopyFrom_ShouldBeEquivalentToOriginal()
    {
        var original = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), Duration.FromHours(10));
        var copy = new Period();
        copy.CopyFrom(original);

        var copyEmpty = new Period(); // internal CTOR
        // another ICopyable instance which is not a Period
        copyEmpty.CopyFrom(new PeriodList());

        Assert.Multiple(() =>
        {
            Assert.That(copy.StartTime, Is.EqualTo(original.StartTime));
            Assert.That(copy.Duration, Is.EqualTo(original.Duration));

            Assert.That(copyEmpty.StartTime, Is.Null);
            Assert.That(copyEmpty.EndTime, Is.Null);
            Assert.That(copyEmpty.Duration, Is.Null);
        });
    }
}
