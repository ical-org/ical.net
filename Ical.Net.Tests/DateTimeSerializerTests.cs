//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class DateTimeSerializerTests
{
    [Test, Category("Deserialization")]
    public void TZIDPropertyShouldBeAppliedForLocalTimezones()
    {
        // see http://www.ietf.org/rfc/rfc2445.txt p.36
        var result = CalDateTime
            .FromDateTime(new DateTime(1997, 7, 14, 13, 30, 0, DateTimeKind.Local), "US-Eastern")
            .ToBasicIso();

        // TZID is applied elsewhere - just make sure this doesn't have 'Z' appended. 
        Assert.That(result, Is.EqualTo("19970714T133000"));
    }

    private static IEnumerable<TestCaseData> SerializeToStringTestCases()
    {
        yield return new TestCaseData(new CalDateTime(2025, 1, 2)).Returns("20250102");
        yield return new TestCaseData(new CalDateTime(2025, 1, 2, 3, 4, 5)).Returns("20250102T030405");
        yield return new TestCaseData(new CalDateTime(2025, 1, 2, 3, 4, 5, CalDateTime.UtcTzId)).Returns("20250102T030405Z");
        yield return new TestCaseData(new CalDateTime(2025, 1, 2, 3, 4, 5, "Europe/Vienna")).Returns("20250102T030405");
        yield return new TestCaseData(new CalDateTime(1, 1, 1)).Returns("00010101");
        yield return new TestCaseData(new CalDateTime(9999, 12, 31, 23, 59, 59, CalDateTime.UtcTzId)).Returns("99991231T235959Z");
    }

    [Test, TestCaseSource(nameof(SerializeToStringTestCases))]
    public string? SerializeToString_WritesExactValue(CalDateTime value)
        => new DateTimeSerializer().SerializeToString(value);

    [Test]
    public void SerializeToString_ReturnsNullForOtherTypes()
        => Assert.That(new DateTimeSerializer().SerializeToString("20250102"), Is.Null);
}
