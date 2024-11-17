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

    [TestCaseSource(nameof(AsDateTimeOffsetTestCases))]
    public DateTimeOffset AsDateTimeOffsetTests(CalDateTime incoming)
        => incoming.AsDateTimeOffset;

    public static IEnumerable AsDateTimeOffsetTestCases()
    {
        const string nyTzId = "America/New_York";
        var summerDate = DateTime.Parse("2018-05-15T11:00");

        var nySummerOffset = TimeSpan.FromHours(-4);
        var nySummer = new CalDateTime(summerDate, nyTzId);
        yield return new TestCaseData(nySummer)
            .SetName("NY Summer DateTime returns DateTimeOffset with UTC-4")
            .Returns(new DateTimeOffset(summerDate, nySummerOffset));

        var utc = new CalDateTime(summerDate, "UTC");
        yield return new TestCaseData(utc)
            .SetName("UTC summer DateTime returns a DateTimeOffset with UTC-0")
            .Returns(new DateTimeOffset(summerDate, TimeSpan.Zero));

        var convertedToNySummer = new CalDateTime(summerDate, "UTC");
        convertedToNySummer.TzId = nyTzId;
        yield return new TestCaseData(convertedToNySummer)
            .SetName(
                "Summer UTC DateTime converted to NY time zone by setting TzId returns a DateTimeOffset with UTC-4")
            .Returns(new DateTimeOffset(summerDate, nySummerOffset));

        var noTz = new CalDateTime(summerDate);
        var currentSystemOffset = TimeZoneInfo.Local.GetUtcOffset(summerDate);
        yield return new TestCaseData(noTz)
            .SetName(
                $"Summer DateTime with no time zone information returns the system-local's UTC offset ({currentSystemOffset})")
            .Returns(new DateTimeOffset(summerDate, currentSystemOffset));
    }

    [Test(Description = "Calling AsUtc should always return the proper UTC time, even if the TzId has changed")]
    public void TestTzidChanges()
    {
        var someTime = DateTimeOffset.Parse("2018-05-21T11:35:00-04:00");

        var someDt = new CalDateTime(someTime.DateTime) { TzId = "America/New_York" };
        var firstUtc = someDt.AsUtc;
        Assert.That(firstUtc, Is.EqualTo(someTime.UtcDateTime));

        someDt.TzId = "Europe/Berlin";
        var berlinUtc = someDt.AsUtc;
        Assert.That(berlinUtc, Is.Not.EqualTo(firstUtc));
    }

    [Test, TestCaseSource(nameof(DateTimeKindOverrideTestCases))]
    public DateTimeKind DateTimeKindOverrideTests(DateTime dateTime, string tzId)
        => new CalDateTime(dateTime, tzId).Value.Kind;

    public static IEnumerable DateTimeKindOverrideTestCases()
    {
        const string localTz = "America/New_York";
        var localDt = DateTime.SpecifyKind(DateTime.Parse("2018-05-21T11:35:33"), DateTimeKind.Local);

        yield return new TestCaseData(localDt, "UTC")
            .Returns(DateTimeKind.Utc)
            .SetName("Explicit tzid = UTC time zone returns DateTimeKind.Utc");

        yield return new TestCaseData(DateTime.SpecifyKind(localDt, DateTimeKind.Utc), null)
            .Returns(DateTimeKind.Utc)
            .SetName("DateTime with Kind = Utc and no tzid returns DateTimeKind.Utc");

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

        yield return new TestCaseData(DateTime.SpecifyKind(localDt, DateTimeKind.Unspecified), null)
            .Returns(DateTimeKind.Unspecified)
            .SetName("DateTime with Kind = Unspecified and null tzid returns DateTimeKind.Unspecified");
    }

    [Test, TestCaseSource(nameof(ToStringTestCases))]
    public string ToStringTests(CalDateTime calDateTime, string format, IFormatProvider formatProvider)
        => calDateTime.ToString(format, formatProvider);

    public static IEnumerable ToStringTestCases()
    {
        yield return new TestCaseData(new CalDateTime(2024, 8, 30, 10, 30, 0, tzId: "Pacific/Auckland"), "O", null)
            .Returns("2024-08-30T10:30:00.0000000+12:00 Pacific/Auckland")
            .SetName("Date and time with 'O' format arg, default culture");

        yield return new TestCaseData(new CalDateTime(2024, 8, 30, tzId: "Pacific/Auckland"), "O", null)
            .Returns("08/30/2024 Pacific/Auckland")
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

        yield return new TestCaseData(new CalDateTime(2024, 8, 30, tzId: "Pacific/Auckland"), null,
                CultureInfo.GetCultureInfo("IT"))
            .Returns("30/08/2024 Pacific/Auckland")
            .SetName("Date only with 'IT' CultureInfo and default format arg");
    }

    [Test]
    public void SetValue_AppliesSameRulesAsWith_CTOR()
    {
        var dateTime = new DateTime(2024, 8, 30, 10, 30, 0, DateTimeKind.Unspecified);
        var tzId = "Europe/Berlin";

        var dt1 = new CalDateTime(dateTime, tzId);
        var dt2 = new CalDateTime(DateTime.Now, tzId);
        dt2.Value = dateTime;

        Assert.Multiple(() =>
        {
            // TzId changes the DateTimeKind to Unspecified
            Assert.That(dt1.Value.Kind, Is.EqualTo(dateTime.Kind));
            Assert.That(dt1.Value.Kind, Is.EqualTo(dt2.Value.Kind));
            Assert.That(dt1.TzId, Is.EqualTo(dt2.TzId));
        });
    }

    [Test]
    public void SetValue_LeavesExistingPropertiesUnchanged()
    {
        var cal = new Calendar();
        var dateTime = new DateTime(2024, 8, 30, 10, 30, 0, DateTimeKind.Unspecified);
        var tzId = "Europe/Berlin";

        var dt = new CalDateTime(dateTime, tzId, false)
        {
            AssociatedObject = cal
        };
        var hasTimeInitial = dt.HasTime;

        dt.Value = DateTime.MinValue;

        // Properties should remain unchanged
        Assert.Multiple(() =>
        {
            Assert.That(dt.HasTime, Is.EqualTo(hasTimeInitial));
            Assert.That(dt.TzId, Is.EqualTo(tzId));
            Assert.That(dt.Calendar, Is.SameAs(cal));
        });
    }

    [Test]
    public void Simple_PropertyAndMethod_HasTime_Tests()
    {
        var dt = new DateTime(2025, 1, 2, 10, 20, 30, DateTimeKind.Utc);
        var c = new CalDateTime(dt, tzId: "Europe/Berlin");

        var c2 = new CalDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, c.TzId, null);
        var c3 = new CalDateTime(new DateOnly(dt.Year, dt.Month, dt.Day),
            new TimeOnly(dt.Hour, dt.Minute, dt.Second), dt.Kind, c.TzId);

        Assert.Multiple(() =>
        {
            Assert.That(c2.Ticks, Is.EqualTo(c3.Ticks));
            Assert.That(c2.TzId, Is.EqualTo(c3.TzId));
            Assert.That(CalDateTime.UtcNow.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(c.Millisecond, Is.EqualTo(0));
            Assert.That(c.Ticks, Is.EqualTo(dt.Ticks));
            Assert.That(c.DayOfYear, Is.EqualTo(dt.DayOfYear));
            Assert.That(c.TimeOfDay, Is.EqualTo(dt.TimeOfDay));
            Assert.That(c.Subtract(TimeSpan.FromSeconds(dt.Second)).Value.Second, Is.EqualTo(0));
            Assert.That(c.AddYears(1).Value, Is.EqualTo(dt.AddYears(1)));
            Assert.That(c.AddMonths(1).Value, Is.EqualTo(dt.AddMonths(1)));
            Assert.That(c.AddDays(1).Value, Is.EqualTo(dt.AddDays(1)));
            Assert.That(c.AddHours(1).Value, Is.EqualTo(dt.AddHours(1)));
            Assert.That(c.AddMinutes(1).Value, Is.EqualTo(dt.AddMinutes(1)));
            Assert.That(c.AddSeconds(15).Value, Is.EqualTo(dt.AddSeconds(15)));
            Assert.That(c.AddMilliseconds(100).Value, Is.EqualTo(dt.AddMilliseconds(0))); // truncated
            Assert.That(c.AddMilliseconds(1000).Value, Is.EqualTo(dt.AddMilliseconds(1000)));
            Assert.That(c.AddTicks(1).Value, Is.EqualTo(dt.AddTicks(0))); // truncated
            Assert.That(c.AddTicks(TimeSpan.FromMinutes(1).Ticks).Value, Is.EqualTo(dt.AddTicks(TimeSpan.FromMinutes(1).Ticks)));
            Assert.That(c.DateOnlyValue, Is.EqualTo(new DateOnly(dt.Year, dt.Month, dt.Day)));
            Assert.That(c.TimeOnlyValue, Is.EqualTo(new TimeOnly(dt.Hour, dt.Minute, dt.Second)));
            Assert.That(c.ToString("dd.MM.yyyy"), Is.EqualTo("02.01.2025 Europe/Berlin"));
        });
    }

    [Test]
    public void Simple_PropertyAndMethod_NotHasTime_Tests()
    {
        var dt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var c = new CalDateTime(dt, tzId: "Europe/Berlin", hasTime: false);

        // Adding time to a date-only value should not change the HasTime property
        Assert.Multiple(() =>
        {
            var result = c.AddHours(1);
            Assert.That(result.HasTime, Is.EqualTo(true));

            result = c.AddMinutes(1);
            Assert.That(result.HasTime, Is.EqualTo(true));

            result = c.AddSeconds(1);
            Assert.That(result.HasTime, Is.EqualTo(true));

            result = c.AddMilliseconds(1000);
            Assert.That(result.HasTime, Is.EqualTo(true));

            result = c.AddTicks(TimeSpan.FromMinutes(1).Ticks);
            Assert.That(result.HasTime, Is.EqualTo(true));
        });
    }

    [Test]
    public void Toggling_HasDate_ShouldSucceed()
    {
        var dateTime = new DateTime(2025, 1, 2, 10, 20, 30, DateTimeKind.Utc);
        var dt = new CalDateTime(dateTime);
        Assert.Multiple(() =>
        {
            Assert.That(dt.HasTime, Is.True);
            Assert.That(dt.HasDate, Is.True);

            dt.HasDate = false;
            Assert.That(dt.HasDate, Is.False);
            Assert.That(dt.DateOnlyValue.HasValue, Is.False);
            Assert.That(() => dt.Value, Throws.InstanceOf<InvalidOperationException>());

            dt.HasDate = true;
            Assert.That(dt.HasDate, Is.True);
        });
    }
}
