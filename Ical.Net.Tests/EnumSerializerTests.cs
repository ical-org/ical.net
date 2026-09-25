//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// <see cref="EnumSerializer.Deserialize"/> falls back to the raw string only when the value is not
/// a member of the enum. Any other failure, a decoding error included, propagates rather than
/// yielding a silently mistyped property value.
/// </summary>
[TestFixture]
public class EnumSerializerTests
{
    private static object? Deserialize(Type enumType, string value)
        => new EnumSerializer(enumType).Deserialize(new StringReader(value));

    [TestCase("Weekly", FrequencyType.Weekly)]
    [TestCase("WEEKLY", FrequencyType.Weekly)]
    [TestCase("weekly", FrequencyType.Weekly)]
    [TestCase("Monthly", FrequencyType.Monthly)]
    public void ParsesKnownValuesCaseInsensitively(string value, FrequencyType expected)
        => Assert.That(Deserialize(typeof(FrequencyType), value), Is.EqualTo(expected));

    /// <summary>Hyphens are stripped before parsing, so SECOND-LY resolves to Secondly.</summary>
    [Test]
    public void StripsHyphensBeforeParsing()
        => Assert.That(Deserialize(typeof(FrequencyType), "SECOND-LY"), Is.EqualTo(FrequencyType.Secondly));

    /// <summary>
    /// Unknown enum values are common in real-world .ics files and must fall back to the raw string
    /// rather than throwing. The long numeric case overflows the underlying type, which fails
    /// differently from an unrecognised name.
    /// </summary>
    [TestCase("Fortnightly")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("123456789012345678901234567890")]
    public void UnknownValueFallsBackToTheRawString(string value)
        => Assert.That(Deserialize(typeof(FrequencyType), value), Is.EqualTo(value));

    /// <summary>
    /// Hyphens are stripped before matching, and <see cref="Enum.Parse(Type, string, bool)"/>
    /// accepts a numeric string, so "-1" resolves by value rather than falling back.
    /// </summary>
    [Test]
    public void NumericValueResolvesByValue()
        => Assert.That(Deserialize(typeof(FrequencyType), "-1"), Is.EqualTo(FrequencyType.Minutely));

    [Test]
    public void SerializesEnumToItsName()
        => Assert.That(new EnumSerializer(typeof(FrequencyType)).SerializeToString(FrequencyType.Yearly),
            Is.EqualTo("Yearly"));
}
