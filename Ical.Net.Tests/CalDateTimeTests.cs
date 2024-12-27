//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ical.Net.Tests;

public class CalDateTimeTests
{
    private static readonly DateTime _now = DateTime.Now;
    private static readonly DateTime _later = _now.AddHours(1);

    private static CalendarEvent GetEventWithRecurrenceRules(string tzId)
    {
        var dailyForFiveDays = new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Count = 5,
        };

        var calendarEvent = new CalendarEvent
        {
            Start = new CalDateTime(_now, tzId),
            End = new CalDateTime(_later, tzId),
            RecurrenceRules = new List<RecurrencePattern> { dailyForFiveDays },
            Resources = new List<string>(new[] { "Foo", "Bar", "Baz" }),
        };
        return calendarEvent;
    }

    [Test, TestCaseSource(nameof(ToTimeZoneTestCases))]
    public void ToTimeZoneTests(CalendarEvent calendarEvent, string targetTimeZone)
    {
        var startAsUtc = calendarEvent.Start.AsUtc;

        var convertedStart = calendarEvent.Start.ToTimeZone(targetTimeZone);
        var convertedAsUtc = convertedStart.AsUtc;

        Assert.That(convertedAsUtc, Is.EqualTo(startAsUtc));
    }

    public static IEnumerable ToTimeZoneTestCases()
    {
        const string bclCst = "Central Standard Time";
        const string bclEastern = "Eastern Standard Time";
        var bclEvent = GetEventWithRecurrenceRules(bclCst);
        yield return new TestCaseData(bclEvent, bclEastern)
            .SetName($"BCL to BCL: {bclCst} to {bclEastern}");

        const string ianaNy = "America/New_York";
        const string ianaRome = "Europe/Rome";
        var ianaEvent = GetEventWithRecurrenceRules(ianaNy);

        yield return new TestCaseData(ianaEvent, ianaRome)
            .SetName($"IANA to IANA: {ianaNy} to {ianaRome}");

        const string utc = "UTC";
        var utcEvent = GetEventWithRecurrenceRules(utc);
        yield return new TestCaseData(utcEvent, utc)
            .SetName("UTC to UTC");

        yield return new TestCaseData(bclEvent, ianaRome)
            .SetName($"BCL to IANA: {bclCst} to {ianaRome}");

        yield return new TestCaseData(ianaEvent, bclCst)
            .SetName($"IANA to BCL: {ianaNy} to {bclCst}");
    }

    [Test(Description = "A certain date/time value applied to different timezones should return the same UTC date/time")]
    public void SameDateTimeWithDifferentTzIdShouldReturnSameUtc()
    {
        var someTime = DateTimeOffset.Parse("2018-05-21T11:35:00-04:00");

        var someDt = new CalDateTime(someTime.DateTime, "America/New_York");
        var firstUtc = someDt.AsUtc;
        Assert.That(firstUtc, Is.EqualTo(someTime.UtcDateTime));

        someDt = new CalDateTime(someTime.DateTime, "Europe/Berlin");
        var berlinUtc = someDt.AsUtc;
        Assert.That(berlinUtc, Is.Not.EqualTo(firstUtc));
    }

    [Test, TestCaseSource(nameof(DateTimeKindOverrideTestCases)), Description("DateTimeKind of values is always DateTimeKind.Unspecified")]
    public DateTimeKind DateTimeKindOverrideTests(DateTime dateTime, string tzId)
        => new CalDateTime(dateTime, tzId).Value.Kind;

    public static IEnumerable DateTimeKindOverrideTestCases()
    {
        const string localTz = "America/New_York";
        var localDt = DateTime.SpecifyKind(DateTime.Parse("2018-05-21T11:35:33"), DateTimeKind.Unspecified);

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
            .Returns("2024-08-30T10:30:00.0000000+12:00 Pacific/Auckland")
            .SetName("Date and time with 'O' format arg, default culture");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30), "O", null)
            .Returns("2024-08-30") // Date only cannot have timezone
            .SetName("Date only with 'O' format arg, default culture");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30, 10, 30, 0, tzId: "Pacific/Auckland"), "O",
                CultureInfo.GetCultureInfo("fr-FR"))
            .Returns("2024-08-30T10:30:00.0000000+12:00 Pacific/Auckland")
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

    [Test, TestCaseSource(nameof(DateTimeArithmeticTestCases))]
    public DateTime DateTimeArithmeticTests(Func<IDateTime, IDateTime> operation)
    {
        var result = operation(new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: CalDateTime.UtcTzId));
        return result.Value;
    }

    public static IEnumerable DateTimeArithmeticTestCases()
    {
        var dateTime = new DateTime(2025, 1, 15, 10, 20, 30);

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.AddHours(1)))
            .Returns(dateTime.AddHours(1))
            .SetName($"{nameof(IDateTime.AddHours)} 1 hour");

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.Add(Duration.FromSeconds(30))))
            .Returns(dateTime.Add(TimeSpan.FromSeconds(30)))
            .SetName($"{nameof(IDateTime.Add)} 30 seconds");

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.AddMinutes(70)))
            .Returns(dateTime.AddMinutes(70))
            .SetName($"{nameof(IDateTime.AddMinutes)} 70 minutes");
    }

    [Test, TestCaseSource(nameof(EqualityTestCases))]
    public bool EqualityTests(Func<IDateTime, bool> operation)
    {
        return operation(new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: CalDateTime.UtcTzId));
    }

    public static IEnumerable EqualityTestCases()
    {
        yield return new TestCaseData(new Func<IDateTime, bool>(dt => (CalDateTime)dt == new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: CalDateTime.UtcTzId)))
            .Returns(true)
            .SetName("== operator 2 UTC timezones");

        yield return new TestCaseData(new Func<IDateTime, bool>(dt => (CalDateTime)dt != new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: "Europe/Berlin")))
            .Returns(true)
            .SetName("!= operator 2 timezones");

        yield return new TestCaseData(new Func<IDateTime, bool>(dt => (CalDateTime)dt == new CalDateTime(2025, 1, 15, 10, 20, 30, tzId: null)))
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


    [Test, TestCaseSource(nameof(DateOnlyValidArithmeticTestCases))]
    public (DateTime Value, bool HasTime) DateOnlyValidArithmeticTests(Func<IDateTime, IDateTime> operation)
    {
        var result = operation(new CalDateTime(2025, 1, 15));
        return (result.Value, result.HasTime);
    }

    public static IEnumerable DateOnlyValidArithmeticTestCases()
    {
        var dateTime = new DateTime(2025, 1, 15);

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.Add(-Duration.FromDays(1))))
            .Returns((dateTime.AddDays(-1), false))
            .SetName($"{nameof(IDateTime.Add)} -1 day TimeSpan");

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.AddYears(1)))
            .Returns((dateTime.AddYears(1), false))
            .SetName($"{nameof(IDateTime.AddYears)} 1 year");

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.AddMonths(2)))
            .Returns((dateTime.AddMonths(2), false))
            .SetName($"{nameof(IDateTime.AddMonths)} 2 months");

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.AddDays(7)))
            .Returns((dateTime.AddDays(7), false))
            .SetName($"{nameof(IDateTime.AddDays)} 7 days");

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.Add(Duration.FromDays(1))))
            .Returns((dateTime.Add(TimeSpan.FromDays(1)), false))
            .SetName($"{nameof(IDateTime.Add)} 1 day TimeSpan");

        yield return new TestCaseData(new Func<IDateTime, IDateTime>(dt => dt.Add(Duration.Zero)))
            .Returns((dateTime.Add(TimeSpan.Zero), false))
            .SetName($"{nameof(IDateTime.Add)} TimeSpan.Zero");
    }

    [Test]
    public void DateOnlyInvalidArithmeticTests()
    {
        var dt = new CalDateTime(2025, 1, 15);

        Assert.Multiple(() =>
        {
            Assert.That(() => dt.Add(Duration.FromHours(1)), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => dt.AddHours(2), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => dt.AddMinutes(3), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => dt.AddSeconds(4), Throws.TypeOf<InvalidOperationException>());
        });
    }

    [Test]
    public void Simple_PropertyAndMethod_HasTime_Tests()
    {
        // A collection of tests that are not covered elsewhere

        var dt = new DateTime(2025, 1, 2, 10, 20, 30, DateTimeKind.Utc);
        var c = new CalDateTime(dt, tzId: "Europe/Berlin");

        var c2 = new CalDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, c.TzId);
        var c3 = new CalDateTime(new DateOnly(dt.Year, dt.Month, dt.Day),
            new TimeOnly(dt.Hour, dt.Minute, dt.Second), c.TzId);

        Assert.Multiple(() =>
        {
            Assert.That(c2.Value, Is.EqualTo(c3.Value));
            Assert.That(c2.TzId, Is.EqualTo(c3.TzId));
            Assert.That(CalDateTime.UtcNow.Value.Kind, Is.EqualTo(DateTimeKind.Unspecified));
            Assert.That(CalDateTime.Today.Value.Kind, Is.EqualTo(DateTimeKind.Unspecified));
            Assert.That(c.DayOfYear, Is.EqualTo(dt.DayOfYear));
            Assert.That(c.Time?.ToTimeSpan(), Is.EqualTo(dt.TimeOfDay));
            Assert.That(c.Add(-Duration.FromSeconds(dt.Second)).Value.Second, Is.EqualTo(0));
            Assert.That(c.ToString("dd.MM.yyyy"), Is.EqualTo("02.01.2025 Europe/Berlin"));
            // Create a date-only CalDateTime from a CalDateTime
            Assert.That(new CalDateTime(new CalDateTime(2025, 1, 1)), Is.EqualTo(new CalDateTime(2025, 1, 1)));
        });
    }

    public static IEnumerable<TestCaseData> AddAndSubtractTestCases()
    {
        yield return new TestCaseData(new CalDateTime(2024, 10, 27, 0, 0, 0, tzId: null), Duration.FromHours(4))
           .SetName("Floating");

        yield return new TestCaseData(new CalDateTime(2024, 10, 27, 0, 0, 0, tzId: CalDateTime.UtcTzId), Duration.FromHours(4))
            .SetName("UTC");

        yield return new TestCaseData(new CalDateTime(2024, 10, 27, 0, 0, 0, tzId: "Europe/Paris"), Duration.FromHours(4))
            .SetName("Zoned Date/Time with DST change");
    }

    [Test, TestCaseSource(nameof(AddAndSubtractTestCases))]
    public void AddAndSubtract_ShouldBeReversible(CalDateTime t, Duration d)
    {
        Assert.Multiple(() =>
        {
            Assert.That(t.Add(d).Add(-d), Is.EqualTo(t));
            Assert.That(t.Add(d).SubtractExact(t), Is.EqualTo(d.ToTimeSpan()));
        });
    }
}
