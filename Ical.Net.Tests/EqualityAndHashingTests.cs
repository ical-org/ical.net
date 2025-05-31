//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class EqualityAndHashingTests
{
    private const string TzId = "America/Los_Angeles";
    private static readonly DateTime _nowTime = DateTime.SpecifyKind(DateTime.Parse("2016-07-16T16:47:02.9310521-04:00", CultureInfo.InvariantCulture), DateTimeKind.Unspecified);
    private static readonly DateTime _later = _nowTime.AddHours(1);

    [Test, TestCaseSource(nameof(CalDateTime_TestCases))]
    public void CalDateTime_Tests(CalDateTime incomingDt, CalDateTime expectedDt)
    {
        Assert.Multiple(() =>
        {
            Assert.That(expectedDt.Value, Is.EqualTo(incomingDt.Value));
            Assert.That(expectedDt.GetHashCode(), Is.EqualTo(incomingDt.GetHashCode()));
            Assert.That(expectedDt.TzId, Is.EqualTo(incomingDt.TzId));
            Assert.That(incomingDt.Equals(expectedDt), Is.True);
        });
    }

    public static IEnumerable CalDateTime_TestCases()
    {
        var nowCalDt = new CalDateTime(_nowTime);
        yield return new TestCaseData(nowCalDt, new CalDateTime(_nowTime)).SetName("Now, no time zone");

        var nowCalDtWithTz = new CalDateTime(_nowTime, TzId);
        yield return new TestCaseData(nowCalDtWithTz, new CalDateTime(_nowTime, TzId)).SetName("Now, with time zone");
    }

    private static RecurrencePattern GetSimpleRecurrencePattern() => new RecurrencePattern(FrequencyType.Daily, 1)
    {
        Count = 5
    };

    private static CalendarEvent GetSimpleEvent() => new CalendarEvent
    {
        DtStart = new CalDateTime(_nowTime),
        DtEnd = new CalDateTime(_later),
    };

    private static string SerializeEvent(CalendarEvent e) => new CalendarSerializer().SerializeToString(new Calendar { Events = { e } });

    [Test]
    public void Calendar_Tests()
    {
        var rruleA = new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Count = 5
        };

        var e = new CalendarEvent
        {
            DtStart = new CalDateTime(_nowTime),
            Duration = Duration.FromHours(1),
            RecurrenceRules = new List<RecurrencePattern> { rruleA },
        };

        var actualCalendar = new Calendar();
        actualCalendar.Events.Add(e);

        //Work around referential equality...
        var rruleB = new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Count = 5
        };

        var expectedCalendar = new Calendar();
        expectedCalendar.Events.Add(new CalendarEvent
        {
            DtStart = new CalDateTime(_nowTime),
            Duration = Duration.FromHours(1),
            RecurrenceRules = new List<RecurrencePattern> { rruleB },
        });

        Assert.Multiple(() =>
        {
            Assert.That(expectedCalendar.GetHashCode(), Is.EqualTo(actualCalendar.GetHashCode()));
            Assert.That(actualCalendar.Equals(expectedCalendar), Is.True);
        });
    }

    [Test, TestCaseSource(nameof(VTimeZone_TestCases))]
    public void VTimeZone_Tests(VTimeZone actual, VTimeZone expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(expected.Url, Is.EqualTo(actual.Url));
            Assert.That(expected.TzId, Is.EqualTo(actual.TzId));
            Assert.That(expected, Is.EqualTo(actual));
            Assert.That(expected.GetHashCode(), Is.EqualTo(actual.GetHashCode()));
        });
    }

    public static IEnumerable VTimeZone_TestCases()
    {
        const string nzSt = "New Zealand Standard Time";
        var first = new VTimeZone
        {
            TzId = nzSt,
        };
        var second = new VTimeZone(nzSt);
        yield return new TestCaseData(first, second);

        first.Url = new Uri("http://example.com/");
        second.Url = new Uri("http://example.com");
        yield return new TestCaseData(first, second);
    }

    [Test, TestCaseSource(nameof(CalendarCollection_TestCases))]
    public void CalendarCollection_Tests(string rawCalendar)
    {
        var a = Calendar.Load(IcsFiles.UsHolidays);
        var b = Calendar.Load(IcsFiles.UsHolidays);

        Assert.That(a, Is.Not.Null);
        Assert.That(b, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(b.GetHashCode(), Is.EqualTo(a.GetHashCode()));
            Assert.That(b, Is.EqualTo(a));
        });
    }

    public static IEnumerable CalendarCollection_TestCases()
    {
        yield return new TestCaseData(IcsFiles.Google1).SetName("Google calendar test case");
        yield return new TestCaseData(IcsFiles.Parse1).SetName("Weird file parse test case");
        yield return new TestCaseData(IcsFiles.UsHolidays).SetName("US Holidays (quite large)");
    }

    [Test]
    public void Resources_Tests()
    {
        var origContents = new[] { "Foo", "Bar" };
        var e = GetSimpleEvent();
        e.Resources = new List<string>(origContents);
        Assert.That(e.Resources.Count == 2, Is.True);

        e.Resources.Add("Baz");
        Assert.That(e.Resources.Count == 3, Is.True);
        var serialized = SerializeEvent(e);
        Assert.That(serialized, Does.Contain("Baz"));

        e.Resources.Remove("Baz");
        Assert.That(e.Resources.Count == 2, Is.True);
        serialized = SerializeEvent(e);
        Assert.That(serialized, Does.Not.Contain("Baz"));

        e.Resources.Add("Hello");
        Assert.That(e.Resources.Contains("Hello"), Is.True);
        serialized = SerializeEvent(e);
        Assert.That(serialized, Does.Contain("Hello"));

        e.Resources.Clear();
        e.Resources.AddRange(origContents);
        Assert.That(origContents, Is.EquivalentTo(e.Resources));
        serialized = SerializeEvent(e);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("Foo"));
            Assert.That(serialized, Does.Contain("Bar"));
            Assert.That(serialized, Does.Not.Contain("Baz"));
            Assert.That(serialized, Does.Not.Contain("Hello"));
        });
    }

    private static (byte[] original, byte[] copy) GetAttachments()
    {
        var payload = Encoding.UTF8.GetBytes("This is an attachment!");
        var payloadCopy = new byte[payload.Length];
        Array.Copy(payload, payloadCopy, payload.Length);
        return (payload, payloadCopy);
    }

    [Test, TestCaseSource(nameof(PeriodTestCases))]
    public void PeriodTests(Period a, Period b)
    {
        Assert.Multiple(() =>
        {
            Assert.That(b.GetHashCode(), Is.EqualTo(a.GetHashCode()));
            Assert.That(b, Is.EqualTo(a));
        });
    }

    public static IEnumerable PeriodTestCases()
    {
        yield return new TestCaseData(new Period(new CalDateTime(_nowTime)), new Period(new CalDateTime(_nowTime)))
            .SetName("Two identical CalDateTimes are equal");
    }

    [Test]
    public void PeriodListTests()
    {
        var startTimesA = new List<DateTime>
            {
                new DateTime(2017, 03, 02, 06, 00, 00),
                new DateTime(2017, 03, 03, 06, 00, 00),
                new DateTime(2017, 03, 06, 06, 00, 00),
                new DateTime(2017, 04, 28, 06, 00, 00),
                new DateTime(2017, 05, 01, 06, 00, 00)
            }
            .Select(dt => new Period(new CalDateTime(dt))).ToList();

        var a = new PeriodList();
        foreach (var period in startTimesA)
        {
            a.Add(period);
        }

        // Difference from A: first element became the second,
        // and last element became the second-to-last element
        var startTimesB = new List<DateTime>
            {
                new DateTime(2017, 03, 03, 06, 00, 00),
                new DateTime(2017, 03, 02, 06, 00, 00),
                new DateTime(2017, 03, 06, 06, 00, 00),
                new DateTime(2017, 05, 01, 06, 00, 00),
                new DateTime(2017, 04, 28, 06, 00, 00)
            }
            .Select(dt => new Period(new CalDateTime(dt))).ToList();

        var b = new PeriodList();

        foreach (var period in startTimesB)
        {
            b.Add(period);
        }

        var collectionEqual = CollectionHelpers.Equals(a, b);

        var aThenB = new List<PeriodList> { a, b };
        var bThenA = new List<PeriodList> { b, a };

        Assert.Multiple(() =>
        {
            Assert.That(collectionEqual, Is.EqualTo(true));
            Assert.That(CollectionHelpers.Equals(aThenB, bThenA), Is.True);
        });
    }

    [Test]
    public void CalDateTimeTests()
    {
        var nowLocal = DateTime.Now;
        var nowUtc = nowLocal.ToUniversalTime();

        var asLocal = new CalDateTime(nowLocal, "America/New_York");
        var asUtc = new CalDateTime(nowUtc, "UTC");

        Assert.That(asUtc, Is.Not.EqualTo(asLocal));
    }

    private void TestComparison(Func<CalDateTime, CalDateTime, bool> calOp, Func<int?, int?, bool> intOp)
    {
        int? intSome = 1;
        int? intGreater = 2;

        var dtSome = new CalDateTime(2018, 1, 1);
        var dtGreater = new CalDateTime(2019, 1, 1);

        Assert.Multiple(() =>
        {
            Assert.That(calOp(null, null), Is.EqualTo(intOp(null, null)));
            Assert.That(calOp(null, dtSome), Is.EqualTo(intOp(null, intSome)));
            Assert.That(calOp(dtSome, null), Is.EqualTo(intOp(intSome, null)));
            Assert.That(calOp(dtSome, dtSome), Is.EqualTo(intOp(intSome, intSome)));
            Assert.That(calOp(dtSome, dtGreater), Is.EqualTo(intOp(intSome, intGreater)));
            Assert.That(calOp(dtGreater, dtSome), Is.EqualTo(intOp(intGreater, intSome)));
        });
    }

    [Test]
    public void CalDateTimeComparisonOperatorTests()
    {
        TestComparison((dt1, dt2) => dt1 == dt2, (i1, i2) => i1 == i2);
        TestComparison((dt1, dt2) => dt1 != dt2, (i1, i2) => i1 != i2);
        TestComparison((dt1, dt2) => dt1 > dt2, (i1, i2) => i1 > i2);
        TestComparison((dt1, dt2) => dt1 >= dt2, (i1, i2) => i1 >= i2);
        TestComparison((dt1, dt2) => dt1 < dt2, (i1, i2) => i1 < i2);
        TestComparison((dt1, dt2) => dt1 <= dt2, (i1, i2) => i1 <= i2);
    }
}
