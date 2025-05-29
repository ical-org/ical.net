//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Globalization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class CalDateTimeZonedTests
{
#pragma warning disable NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure

    [Test]
    public void Offset_ReturnsZero_WhenNoZone()
    {
        var zoned = new CalDateTime(2024, 1, 1, 12, 0, 0).AsZoned();

        Assert.That(zoned.Offset, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void Offset_ReturnsCorrectOffset_WhenZonePresent()
    {
        var zoned = new CalDateTime(2024, 1, 1, 12, 0, 0, "Europe/Vienna").AsZoned();

        Assert.That(zoned.Offset, Is.EqualTo(TimeSpan.FromHours(1)));
    }

    [Test]
    public void HasZone_IsFalse_WhenZonedDateTimeIsNull()
    {
        var zoned = new CalDateTime(2024, 1, 1).AsZoned();

        Assert.That(zoned.HasZone, Is.False);
    }

    [Test]
    public void HasZone_IsTrue_WhenZonedDateTimeIsNotNull()
    {
        var zonedDt = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        Assert.That(zonedDt.HasZone, Is.True);
    }

    [Test]
    public void Equals_Object_ReturnsTrueForSameValues()
    {
        var zonedDt = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var a = zonedDt;
        var b = zonedDt;

        // Using the Equals method directly
        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void Equals_Object_ReturnsFalseForDifferentValues()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 2, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        // Using the Equals method directly
        // Typed Equals:
        Assert.That(zonedDt1.Equals(zonedDt2), Is.False);
        Assert.That(zonedDt1.Equals(zonedDt1), Is.True);
        // Equals object;
        Assert.That(zonedDt1.Equals((object) zonedDt2), Is.False); 
        Assert.That(zonedDt1.Equals((object) zonedDt1), Is.True);
        Assert.That(zonedDt2.Equals(null), Is.False);
        Assert.That(zonedDt2.Equals(new object()), Is.False);
    }

    [Test]
    public void GetHashCode_SameForEqualInstances()
    {
        var zonedDt = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        var a = zonedDt;
        var b = zonedDt;

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_CalDateTimeZoned_ReturnsTrueForSameValues()
    {
        var zonedDt = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var a = zonedDt;
        var b = zonedDt;

        // Using the Equals method directly
        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void Equals_CalDateTimeZoned_ReturnsFalseForDifferentValues()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 2, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        // Using the Equals method directly
        Assert.That(zonedDt1.Equals(zonedDt2), Is.False);
    }

    [Test]
    public void Operator_Equality_ReturnsTrueForEqual()
    {
        var zonedDt = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var a = zonedDt;
        var b = zonedDt;

        Assert.Multiple(() =>
        {
            Assert.That(a == b, Is.True);
            Assert.That(null == a, Is.False);
            Assert.That(a == null, Is.False);
        });
        
    }

    [Test]
    public void Operator_Inequality_ReturnsTrueForDifferent()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 1, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        Assert.That(zonedDt1 != zonedDt2, Is.True);
    }

    [Test]
    public void Operator_LessThan_Works()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 1, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        Assert.Multiple(() =>
        {
            Assert.That(zonedDt1 < zonedDt2, Is.True);
            Assert.That(null < zonedDt2, Is.False);
            Assert.That(zonedDt1 < null, Is.False);
        });
        
    }

    [Test]
    public void Operator_LessThanOrEqual_Works()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 1, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

#pragma warning disable CS1718 // Comparison made to same variable
        Assert.Multiple(() =>
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.That(zonedDt1 <= zonedDt2, Is.True);
            Assert.That(null <= zonedDt2, Is.False);
            Assert.That(zonedDt1 <= null, Is.False);
        });
#pragma warning restore CS1718 // Comparison made to same variable
    }

    [Test]
    public void Operator_GreaterThan_Works()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        Assert.Multiple(() =>
        {
            Assert.That(zonedDt1 > zonedDt2, Is.True);
            Assert.That(null > zonedDt2, Is.False);
            Assert.That(zonedDt1 > null, Is.False);
        });
    }

    [Test]
    public void Operator_GreaterThanOrEqual_Works()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

#pragma warning disable CS1718 // Comparison made to same variable
        Assert.Multiple(() =>
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.That(zonedDt1 >= zonedDt2, Is.True);
            Assert.That(null >= zonedDt2, Is.False);
            Assert.That(zonedDt1 >= null, Is.False);
        });
#pragma warning restore CS1718 // Comparison made to same variable
    }

    [Test]
    public void CompareTo_ReturnsZeroForEqual()
    {
        var zonedDt = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var a = zonedDt;
        var b = zonedDt;

        Assert.That(a.CompareTo(b), Is.EqualTo(0));
    }

    [Test]
    public void CompareTo_ReturnsNegativeForLess()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 2, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        Assert.That(zonedDt1.CompareTo(zonedDt2), Is.LessThan(0));
    }

    [Test]
    public void CompareTo_ReturnsPositiveForGreater()
    {
        var zonedDt1 = new CalDateTime(2024, 2, 2, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 1, 1, 12, 0, 0, CalDateTime.UtcTzId).AsZoned();

        Assert.That(zonedDt1.CompareTo(zonedDt2), Is.GreaterThan(0));
    }

    [Test]
    public void CompareTo_Throws_WhenNoTimeZoneExists()
    {
        var zonedDt1 = new CalDateTime(2024, 1, 1).AsZoned();
        var zonedDt2 = new CalDateTime(2024, 1, 2).AsZoned();
        var zonedDt3 = new CalDateTime(2024, 1, 2, 10, 11, 12).AsZoned();

        Assert.Throws<InvalidOperationException>(() => _ = zonedDt1.CompareTo(zonedDt2)); // Both have no zone
        Assert.Throws<InvalidOperationException>(() => _ = zonedDt1.CompareTo(zonedDt3)); // Self has no zone
        Assert.Throws<InvalidOperationException>(() => _ = zonedDt3.CompareTo(zonedDt1)); // Other has no zone
    }

    [Test]
    public void SubtractWithMissingTimePart_ShouldThrow()
    {
        var zonedNoTime = new CalDateTime(2025, 8, 1).AsZoned();
        var zonedWithTime = new CalDateTime(2025, 6, 1, 12, 0, 0).AsZoned();

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => _ = zonedWithTime.Subtract(zonedNoTime), "Cannot subtract a date with no time from one with time.");
            Assert.Throws<InvalidOperationException>(() => _ = zonedWithTime.Subtract(zonedNoTime), "Cannot subtract a date with time from one with no time.");
        });
    }
#pragma warning restore NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure

    [Test]
    public void ToString_ReturnsRequestedFormat()
    {
        var dt = new CalDateTime(2025, 5, 1, 12, 0, 0, "America/New_York");
        var zonedDt1 = dt.AsZoned();
        var zonedDt2 = zonedDt1.ToTimeZone("Asia/Istanbul");
        var result1 = zonedDt1.ToString("yyyy-MM-dd HH:mm:ss zzz", null);
        var result2 = zonedDt2.ToString(null, CultureInfo.GetCultureInfo("en-US"));
        var result3 = zonedDt2.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo("2025-05-01 12:00:00 -04:00 America/New_York"));
            Assert.That(result2, Is.EqualTo("5/1/2025 7:00:00 PM +03:00 Asia/Istanbul"));
            Assert.That(result3, Is.EqualTo("05/01/2025 19:00:00 +03:00 Asia/Istanbul"));
        });
    }

    [Test, TestCaseSource(nameof(LessThanTestCases))]
    public void LessThanComparisonTestCases(CalDateTimeZoned? self, CalDateTimeZoned? other, bool expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(self?.LessThan(other) == true, Is.EqualTo(expected));
            Assert.That(self?.LessThanOrEqual(other) == true, Is.EqualTo(expected));
        });
    }

    public static System.Collections.IEnumerable LessThanTestCases
    {
        get
        {
            // Floating: both
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 1).AsZoned(),
                new CalDateTime(2025, 1, 2).AsZoned(),
                true
            ).SetName("LessThan_BothHaveIsFloating");

            // Floating self.IsFloating = true, other.IsFloating = false
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 1).AsZoned(),
                new CalDateTime(2025, 1, 2, 12, 0, 0, "UTC").AsZoned(),
                true
            ).SetName("LessThan_LeftIsFloating_OtherNotFloating");

            // Floating self.IsFloating = false, other.IsFloating = true
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                new CalDateTime(2025, 1, 2).AsZoned(),
                true
            ).SetName("LessThan_LeftIsNotFloating_OtherIsFloating");

            // Timezone: same
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                new CalDateTime(2025, 1, 2, 12, 0, 0, "UTC").AsZoned(),
                true
            ).SetName("LessThan_SameTimeZone");

            // Timezone: different
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                new CalDateTime(2025, 1, 2, 12, 0, 0, "Europe/Vienna").AsZoned(),
                true
            ).SetName("LessThan_DifferentTimeZone");

            // self is null, other is not
            yield return new TestCaseData(
                null,
                new CalDateTime(2025, 1, 2, 12, 0, 0, "Europe/Vienna").AsZoned(),
                false
            ).SetName("LessThan_LeftIsNull");

            // self is null, other is not
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                null,
                false
            ).SetName("LessThan_OtherIsNull");
        }
    }

    [Test]
    public void LessThanForEquality()
    {
        var zoned = new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned();

        Assert.Multiple(() =>
        {
            Assert.That(zoned.LessThanOrEqual(zoned), Is.True);
            Assert.That(zoned.LessThan(zoned), Is.False);
        });
    }

    [Test, TestCaseSource(nameof(GreaterThanTestCases))]
    public void GreaterThanComparisonTestCases(CalDateTimeZoned? self, CalDateTimeZoned? other, bool expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(self?.GreaterThan(other) == true, Is.EqualTo(expected));
            Assert.That(self?.GreaterThanOrEqual(other) == true, Is.EqualTo(expected));
        });
    }

    public static System.Collections.IEnumerable GreaterThanTestCases
    {
        get
        {
            // Floating: both
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 2).AsZoned(),
                new CalDateTime(2025, 1, 1).AsZoned(),
                true
            ).SetName("GreaterThan_BothHaveIsFloating");

            // Floating self.IsFloating = true, other.IsFloating = false
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 2, 12, 0, 0, "UTC").AsZoned(),
                new CalDateTime(2025, 1, 1).AsZoned(),
                true
            ).SetName("GreaterThan_LeftIsFloating_OtherNotFloating");

            // Floating self.IsFloating = false, other.IsFloating = true
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 2).AsZoned(),
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                true
            ).SetName("GreaterThan_LeftIsNotFloating_OtherIsFloating");

            // Timezone: same
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 2, 12, 0, 0, "UTC").AsZoned(),
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                true
            ).SetName("GreaterThan_SameTimeZone");

            // Timezone: different
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 2, 12, 0, 0, "Europe/Vienna").AsZoned(),
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                true
            ).SetName("GreaterThan_DifferentTimeZone");

            yield return new TestCaseData(
                null,
                new CalDateTime(2025, 1, 2, 12, 0, 0, "Europe/Vienna").AsZoned(),
                false
            ).SetName("GreaterThan_LeftIsNull");

            // self is null, other is not
            yield return new TestCaseData(
                new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned(),
                null,
                false
            ).SetName("GreaterThan_OtherIsNull");
        }
    }

    [Test]
    public void GreaterThanForEquality()
    {
        var zoned = new CalDateTime(2025, 1, 1, 12, 0, 0, "UTC").AsZoned();
        Assert.Multiple(() =>
        {
            Assert.That(zoned.GreaterThanOrEqual(zoned), Is.True);
            Assert.That(zoned.GreaterThan(zoned), Is.False);
        });
    }
}
