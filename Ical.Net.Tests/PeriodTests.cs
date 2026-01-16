//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

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
        var periodWithEndTime = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), new CalDateTime(2025, 1, 1, 1, 0, 0, "America/New_York"));
        var periodWithDuration = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), Duration.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(period.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));
            Assert.That(period.EndTime, Is.Null);
            Assert.That(period.Duration, Is.Null);
            
            Assert.That(periodWithEndTime.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));

            Assert.That(periodWithDuration.StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York")));

            Assert.That(Period.Create(period.StartTime).Duration, Is.Null);
        });
    }

    [Test]
    public void CreatePeriodWithInvalidArgumentsShouldThrow()
    {
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
    public void CompareTo_ReturnsExpectedValues()
    {
        var dt = new CalDateTime(2025, 6, 1, 0, 0, 0, "Europe/Vienna")
            .ToZonedDateTime();

        Assert.Multiple(() =>
        {
            Assert.That(new EvaluationPeriod(dt).CompareTo(null),
                Is.EqualTo(1));
            Assert.That(new EvaluationPeriod(dt).CompareTo(new EvaluationPeriod(dt)),
                Is.EqualTo(0));
            Assert.That(new EvaluationPeriod(dt).CompareTo(new EvaluationPeriod(dt.PlusHours(-1))),
                Is.EqualTo(1));
            Assert.That(new EvaluationPeriod(dt).CompareTo(new EvaluationPeriod(dt.PlusHours(1))),
                Is.EqualTo(-1));
        });
    }



    [Test]
    public void HasEndOrDuration_WithoutDuration_ShouldBeFalse()
    {
        var period = new Period(new CalDateTime(2025, 7, 1, 12, 0, 0));
        Assert.That(period.HasEndOrDuration, Is.False, "HasEndOrDuration should be false when no duration is set.");
    }

    [Test]
    public void Equals_Tests()
    {
        var start = new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York");
        var duration1 = Duration.FromHours(1);
        var duration2 = Duration.FromHours(2);

        var period1 = new Period(start, duration1);
        var period2 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0, "America/New_York"), duration1);
        var period3 = new Period(start, duration2);

        Assert.Multiple(() =>
        {
            // Test equality for identical periods.
            Assert.That(period1.Equals(period2), Is.True,
                "Periods with identical start and duration should be equal.");
            Assert.That(period2.Equals(period1), Is.True,
                "Symmetric equality failed.");
            Assert.That(period1.GetHashCode(), Is.EqualTo(period2.GetHashCode()),
                "Hash codes should match for equal periods.");

            // Test non-equality with different duration.
            Assert.That(period1.Equals(period3), Is.False,
                "Periods with different durations should not be equal.");
            Assert.That(period3.Equals(period1), Is.False,
                "Symmetric non-equality failed.");

            // Test equality with self.
            Assert.That(period1.Equals(period1), Is.True,
                "A period should equal itself.");

            // Test equality with null.
            Assert.That(period1.Equals(null), Is.False,
                "A period should not equal null.");

            // Test equality with object of different type.
            Assert.That(period1!.Equals("string"), Is.False,
                "A period should not be equal to an object of different type.");
        });
    }
}
