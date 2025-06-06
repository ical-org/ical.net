//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
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
        var dt = new CalDateTime(2025, 6, 1, 0, 0, 0, "Europe/Vienna");

        Assert.Multiple(() =>
        {
            Assert.That(new Period(dt).CompareTo(null),
                Is.EqualTo(1));
            Assert.That(new Period(dt).CompareTo(new Period(dt)),
                Is.EqualTo(0));
            Assert.That(new Period(dt).CompareTo(new Period(dt.AddHours(-1))),
                Is.EqualTo(1));
            Assert.That(new Period(dt).CompareTo(new Period(dt.AddHours(1))),
                Is.EqualTo(-1));
        });
    }

    [Test, TestCaseSource(nameof(CollidesWithPeriodTestCases))]
    public void CollidesWithPeriod(Period period1, Period? period2, bool expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(period1.CollidesWith(period2), Is.EqualTo(expected));
            Assert.That(period2?.CollidesWith(period1) == true, Is.EqualTo(expected));
        });
    }

    private static IEnumerable<TestCaseData> CollidesWithPeriodTestCases
    {
        get
        {
            // Overlapping periods
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(2)),
                new Period(new CalDateTime(2025, 1, 1, 1, 0, 0), Duration.FromHours(2)),
                true
            ).SetName("Overlap: period1 and period2 overlap by 1 hour");

            // Contiguous periods (end of one is start of another, exclusive end)
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2025, 1, 1, 1, 0, 0), Duration.FromHours(1)),
                false
            ).SetName("Contiguous: period1 ends when period2 starts (no overlap)");

            // One inside another
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(4)),
                new Period(new CalDateTime(2025, 1, 1, 1, 0, 0), Duration.FromHours(1)),
                true
            ).SetName("Contained: period2 is inside period1");

            // Identical periods
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(2)),
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(2)),
                true
            ).SetName("Identical: periods are exactly the same");

            // Non-overlapping periods
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2025, 1, 1, 2, 0, 0), Duration.FromHours(1)),
                false
            ).SetName("NoOverlap: periods are completely separate");

            // This Duration is zero (point in time)
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.Zero),
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1)),
                false
            ).SetName("NoOverlap: this duration is zero");

            // Other Duration is zero (point in time)
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1)),
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.Zero),
                false
            ).SetName("NoOverlap: other duration is zero");

            // other period is null
            yield return new TestCaseData(
                new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1)),
                null,
                false
            ).SetName("NoOverlap: other period is null");

        }
    }

    [Test]
    public void PeriodCollidesWith_WhenNoDurationShouldThrow()
    {
        var period1 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(2));
        var period2 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0));

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => _ = period1.CollidesWith(period2));
            Assert.Throws<ArgumentException>(() => _ = period2.CollidesWith(period1));
        });
    }

    [Test]
    public void EffectiveEndTime_WithoutDuration_ShouldBeNull()
    {
        var period = new Period(new CalDateTime(2025, 7, 1, 12, 0, 0));
        Assert.That(period.EffectiveEndTime, Is.Null, "EffectiveEndTime should be null when no duration is set.");
    }

    [Test]
    public void Contains_Tests()
    {
        var start = new CalDateTime(2025, 1, 1, 0, 0, 0, CalDateTime.UtcTzId);
        var dtBefore = start.AddSeconds(-1);
        var dtAtStart = start;
        var dtMid = start.AddMinutes(30);
        var dtAtEnd = start.AddHours(1);
        var dtAfter = start.AddHours(1).AddSeconds(1);

        // Period with duration: effective end time = start + 1 hour (exclusive)
        var periodWithDuration = new Period(start, Duration.FromHours(1));
        var periodWithoutDuration = new Period(start);
        Assert.Multiple(() =>
        {
            Assert.That(periodWithDuration.Contains(null), Is.False, "Contains should return false for null dt.");
            Assert.That(periodWithDuration.Contains(dtBefore), Is.False, "Contains should return false if dt is before start.");
            Assert.That(periodWithDuration.Contains(dtAtStart), Is.True, "Contains should return true for dt equal to start.");
            Assert.That(periodWithDuration.Contains(dtMid), Is.True, "Contains should return true for dt in the middle.");
            Assert.That(periodWithDuration.Contains(dtAtEnd), Is.False, "Contains should return false for dt equal to effective end (exclusive).");
            Assert.That(periodWithDuration.Contains(dtAfter), Is.False, "Contains should return false for dt after effective end.");
            Assert.That(periodWithoutDuration.Contains(dtAtStart), Is.True, "Contains should return true for self without effective end");
        });
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
            Assert.That(period1.Equals("string"), Is.False,
                "A period should not be equal to an object of different type.");
        });
    }
}
