//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Ical.Net.DataTypes;
using NodaTime;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Ical.Net.Tests;

public class CalDateTimeTests
{
    [Test(Description = "A certain date/time value applied to different timezones should return the same UTC date/time")]
    public void SameDateTimeWithDifferentTzIdShouldReturnSameUtc()
    {
        var someTime = DateTimeOffset.Parse("2018-05-21T11:35:00-04:00", CultureInfo.InvariantCulture);

        var someDt = CalDateTime.FromDateTime(someTime.DateTime, "America/New_York");
        var firstUtc = someDt.ToZonedDateTime().ToDateTimeOffset();
        Assert.That(firstUtc, Is.EqualTo(someTime));

        someDt = CalDateTime.FromDateTime(someTime.DateTime, "Europe/Berlin");
        var berlinUtc = someDt.ToZonedDateTime().ToDateTimeOffset();
        Assert.That(berlinUtc, Is.Not.EqualTo(firstUtc));
    }

    [Test, TestCaseSource(nameof(DateTimeKindOverrideTestCases)), Description("DateTimeKind of values is always DateTimeKind.Unspecified")]
    public DateTimeKind DateTimeKindOverrideTests(DateTime dateTime, string tzId)
        => CalDateTime.FromDateTime(dateTime, tzId).ToDateTimeUnspecified().Kind;

    private static IEnumerable DateTimeKindOverrideTestCases()
    {
        const string localTz = "America/New_York";
        var localDt = DateTime.SpecifyKind(DateTime.Parse("2018-05-21T11:35:33", CultureInfo.InvariantCulture), DateTimeKind.Unspecified);

        yield return new TestCaseData(localDt, "UTC")
            .Returns(DateTimeKind.Unspecified)
            .SetName("Explicit tzid = UTC time zone returns DateTimeKind.Unspecified");

        yield return new TestCaseData(DateTime.SpecifyKind(localDt, DateTimeKind.Utc), null)
            .Returns(DateTimeKind.Unspecified)
            .SetName("DateTime with Kind = Utc and no tzid returns DateTimeKind.Unspecified");

        yield return new TestCaseData(localDt, localTz)
            .Returns(DateTimeKind.Unspecified)
            .SetName("Datetime with kind Local and local tzid returns DateTimeKind.Unspecified");

        yield return new TestCaseData(DateTime.SpecifyKind(localDt, DateTimeKind.Utc), localTz)
            .Returns(DateTimeKind.Unspecified)
            .SetName("DateTime with Kind = Utc with explicit local tzid returns DateTimeKind.Unspecified");

        yield return new TestCaseData(DateTime.SpecifyKind(localDt, DateTimeKind.Unspecified), localTz)
            .Returns(DateTimeKind.Unspecified)
            .SetName("DateTime with Kind = Unspecified with explicit local tzid returns DateTimeKind.Unspecified");

        yield return new TestCaseData(localDt, null)
            .Returns(DateTimeKind.Unspecified)
            .SetName("DateTime with Kind = Local with null tzid returns DateTimeKind.Unspecified");

        yield return new TestCaseData(DateTime.SpecifyKind(localDt, DateTimeKind.Local), null)
            .Returns(DateTimeKind.Unspecified)
            .SetName("DateTime with Kind = Local and null tzid returns DateTimeKind.Unspecified");
    }

    [Test, TestCaseSource(nameof(ToStringTestCases))]
    public string ToStringTests(CalDateTime calDateTime, string format, IFormatProvider formatProvider)
        => calDateTime.ToString(format, formatProvider);

    public static IEnumerable ToStringTestCases()
    {
        yield return new TestCaseData(new CalDateTime(2024, 8, 30, 10, 30, 0, tzId: "Pacific/Auckland"), "O", null)
            .Returns("2024-08-30T10:30:00.0000000 Pacific/Auckland")
            .SetName("Date and time with 'O' format arg, default culture");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30), "O", null)
            .Returns("2024-08-30") // Date only cannot have timezone
            .SetName("Date only with 'O' format arg, default culture");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30, 10, 30, 0, tzId: "Pacific/Auckland"), "O",
                CultureInfo.GetCultureInfo("fr-FR"))
            .Returns("2024-08-30T10:30:00.0000000 Pacific/Auckland")
            .SetName("Date and time with 'O' format arg, French culture");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30, 10, 30, 0, tzId: "Pacific/Auckland"),
                "yyyy-MM-dd", CultureInfo.InvariantCulture)
            .Returns("2024-08-30 Pacific/Auckland")
            .SetName("Date and time with custom format, default culture");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30, 10, 30, 0, tzId: "Pacific/Auckland"),
                "MM/dd/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("FR"))
            .Returns("08/30/2024 10:30:00 Pacific/Auckland")
            .SetName("Date and time with format and 'FR' CultureInfo");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30), null,
                CultureInfo.GetCultureInfo("IT")) // Date only cannot have timezone
            .Returns("30/08/2024")
            .SetName("Date only with 'IT' CultureInfo and default format arg");
    }

    [Test, TestCaseSource(nameof(EqualityTestCases))]
    public bool EqualityTests(Func<CalDateTime, bool> operation)
    {
        return operation(new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: CalDateTime.UtcTzId));
    }

    public static IEnumerable EqualityTestCases()
    {
        yield return new TestCaseData(new Func<CalDateTime, bool>(dt => (CalDateTime)dt == new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: CalDateTime.UtcTzId)))
            .Returns(true)
            .SetName("== operator 2 UTC timezones");

        yield return new TestCaseData(new Func<CalDateTime, bool>(dt => (CalDateTime)dt != new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: "Europe/Berlin")))
            .Returns(true)
            .SetName("!= operator 2 timezones");

        yield return new TestCaseData(new Func<CalDateTime, bool>(dt => (CalDateTime)dt == new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: null)))
            .Returns(false)
            .SetName("== operator UTC vs. floating");
    }

    [Test]
    public void EqualityShouldBeTransitive()
    {
        var seq1 =
            new CalDateTime[]
            {
                new("20241204T000000", tzId: "Europe/Vienna"),
                new("20241204T000000", tzId: null),
                new("20241204T000000", tzId: "America/New_York")
            }.Distinct();

        var seq2 =
            new CalDateTime[]
            {
                new("20241204T000000", tzId: "America/New_York"),
                new("20241204T000000", tzId: "Europe/Vienna"),
                new("20241204T000000", tzId: null)
            }.Distinct();

        // Equality using GetHashCode()
        Assert.That(seq1.Count(), Is.EqualTo(seq2.Count()));
    }

    [Test]
    public void Simple_PropertyAndMethod_HasTime_Tests()
    {
        // A collection of tests that are not covered elsewhere

        var dt = new DateTime(2025, 1, 2, 10, 20, 30, DateTimeKind.Utc);
        var c = CalDateTime.FromDateTime(dt, tzId: "Europe/Berlin");

        var c2 = new CalDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, c.TzId);
        var c3 = new CalDateTime(new NodaTime.LocalDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second), c.TzId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(c2.ToDateTimeUnspecified(), Is.EqualTo(c3.ToDateTimeUnspecified()));
            Assert.That(c2.TzId, Is.EqualTo(c3.TzId));
            Assert.That(CalDateTime.UtcNow.ToDateTimeUnspecified().Kind, Is.EqualTo(DateTimeKind.Unspecified));
            Assert.That(CalDateTime.Today.ToDateTimeUnspecified().Kind, Is.EqualTo(DateTimeKind.Unspecified));
            Assert.That(c.DayOfYear, Is.EqualTo(dt.DayOfYear));
            Assert.That(c.Time, Is.EqualTo(NodaTime.LocalDateTime.FromDateTime(dt).TimeOfDay));
            Assert.That(c.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), Is.EqualTo("02.01.2025 Europe/Berlin"));
        }
    }

    private static TestCaseData[] CalDateTime_FromDateTime_HandlesKindCorrectlyTestCases =>
        [
            new TestCaseData(DateTimeKind.Unspecified, Is.EqualTo(new CalDateTime(2024, 12, 30, 10, 44, 50, null))),
            new TestCaseData(DateTimeKind.Utc, Is.EqualTo(new CalDateTime(2024, 12, 30, 10, 44, 50, "UTC"))),
            new TestCaseData(DateTimeKind.Local, Is.EqualTo(new CalDateTime(2024, 12, 30, 10, 44, 50))),
        ];

    [Test, TestCaseSource(nameof(CalDateTime_FromDateTime_HandlesKindCorrectlyTestCases))]


    public void CalDateTime_FromDateTime_HandlesKindCorrectly(DateTimeKind kind, IResolveConstraint constraint)
    {
        var dt = new DateTime(2024, 12, 30, 10, 44, 50, kind);

        Assert.That(() => CalDateTime.FromDateTime(dt), constraint);
    }

    [TestCase("20250703T060000Z", null)]
    [TestCase("20250703T060000Z", CalDateTime.UtcTzId)]
    public void ConstructorWithIso8601UtcString_ShouldResultInUtc(string value, string? tzId)
    {
        var dt = new CalDateTime(value, tzId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dt.ToDateTimeUnspecified(), Is.EqualTo(new DateTime(2025, 7, 3, 6, 0, 0, DateTimeKind.Utc)));
#pragma warning disable CA1305
            Assert.That(dt.ToString("yyyy-MM-dd HH:mm:ss"), Is.EqualTo("2025-07-03 06:00:00 UTC"));
#pragma warning restore CA1305
            Assert.That(dt.IsUtc, Is.True);
        }
    }

    [Test]
    public void ConstructorWithIso8601UtcString_ButDifferentTzId_ShouldThrow()
        => Assert.That(() => _ = new CalDateTime("20250703T060000Z", "CEST"), Throws.ArgumentException);

    [Test]
    public void ThrowsWhenDateYearIsInvalid()
    {
        var d = new LocalDate(0, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new CalDateTime(d);
        });
    }

    [Test]
    public void ThrowsWhenStringValueIsInvalid()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new CalDateTime("invalid");
        });
    }

#if NET6_0_OR_GREATER
    [Test]
    public void ConvertsToAndFromDateOnly()
    {
        var d = new DateOnly(2026, 4, 19);
        var c = CalDateTime.FromDateOnly(d);

        Assert.That(c.ToDateOnly(), Is.EqualTo(d));
    }

    [Test]
    public void ConvertsToAndFromTimeOnly()
    {
        var t = new TimeOnly(6, 30, 2);
        var d = new DateOnly(2026, 4, 19).ToDateTime(t);

        var c = CalDateTime.FromDateTime(d);

        Assert.That(c.ToTimeOnly(), Is.EqualTo(t));
    }

    [Test]
    public void TimeOnlyIsNullForDateValues()
    {
        var d = new DateOnly(2026, 4, 19);
        var c = CalDateTime.FromDateOnly(d);

        Assert.That(c.ToTimeOnly(), Is.Null);
    }
#endif

    [Test]
    public void ThrowsWhenConvertingToZonedDateTimeWithoutTimeZone()
    {
        var c = new CalDateTime(2026, 4, 19, 5, 0, 0);

        Assert.Throws<InvalidOperationException>(() =>
        {
            c.ToZonedDateTime();
        });
    }

    [Test]
    public void DayOfWeekIsCorrect()
    {
        var c = new CalDateTime(2026, 4, 19, 5, 0, 0);
        Assert.That(c.DayOfWeek, Is.EqualTo(DayOfWeek.Sunday));
    }

    [Test]
    public void TimeValuesAreZeroForDateValues()
    {
        var c = new CalDateTime(2026, 4, 19);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(c.Hour, Is.Zero);
            Assert.That(c.Minute, Is.Zero);
            Assert.That(c.Second, Is.Zero);
        }
    }

    [Test]
    public void OtherTypeIsNotEqual()
    {
        var d = new CalDateTime(2026, 4, 19);

        var isEqual = Equals(d, new DateTime(2026, 4, 19));

        Assert.That(isEqual, Is.False);
    }

    [Test]
    public void DateValueDoesNotEqualDateTimeAtMidnight()
    {
        var a = new CalDateTime(2026, 4, 19, 0, 0, 0);
        var b = new CalDateTime(2026, 4, 19);

        Assert.That(a, Is.Not.EqualTo(b));
    }
}
