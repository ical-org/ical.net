//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class DataTypeSerializerTests
{
    private static IEnumerable<TestCaseData> DurationTestCases()
    {
        yield return new TestCaseData(new Duration()).Returns("P0D");
        yield return new TestCaseData(new Duration(weeks: 2)).Returns("P2W");
        yield return new TestCaseData(new Duration(days: 1, hours: 2, minutes: 3, seconds: 4)).Returns("P1DT2H3M4S");
        yield return new TestCaseData(new Duration(days: -10, minutes: -30)).Returns("-P10DT30M");
        yield return new TestCaseData(new Duration(seconds: int.MaxValue)).Returns("PT2147483647S");
    }

    [Test, TestCaseSource(nameof(DurationTestCases))]
    public string? SerializeDuration(Duration value)
        => new DurationSerializer().SerializeToString(value);

    [TestCase(5, 30, 0, ExpectedResult = "+0530")]
    [TestCase(-5, -30, 0, ExpectedResult = "-0530")]
    [TestCase(0, 0, 0, ExpectedResult = "+0000")]
    [TestCase(-1, -2, -3, ExpectedResult = "-010203")]
    public string? SerializeUtcOffset(int hours, int minutes, int seconds)
        => new UtcOffsetSerializer().SerializeToString(new UtcOffset(new TimeSpan(hours, minutes, seconds)));

    [Test]
    public void SerializePeriod()
    {
        var start = new CalDateTime(2025, 1, 2, 3, 4, 5, CalDateTime.UtcTzId);
        var serializer = new PeriodSerializer();

        Assert.Multiple(() =>
        {
            Assert.That(serializer.SerializeToString(new Period(start, Duration.FromHours(1))), Is.EqualTo("20250102T030405Z/PT1H"));
            Assert.That(serializer.SerializeToString(new Period(start, new CalDateTime(2025, 1, 2, 4, 4, 5, CalDateTime.UtcTzId))), Is.EqualTo("20250102T030405Z/20250102T040405Z"));
        });
    }

    [TestCase("Europe/Vienna", ExpectedResult = "TZID=Europe/Vienna")]
    [TestCase("a;b", ExpectedResult = "TZID=\"a;b\"")]
    [TestCase("a\"b", ExpectedResult = "TZID=ab")]
    public string? SerializeParameter(string value)
        => new ParameterSerializer().SerializeToString(new CalendarParameter("TZID", value));
}
