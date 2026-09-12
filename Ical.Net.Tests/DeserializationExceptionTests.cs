//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// These tests should be compared with <see cref="SimpleDeserializerExceptionTests"/>
/// for differences between serializers.
/// </summary>
public class DeserializationExceptionTests
{
    [Test, Category("Deserialization")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        PRODID:Test
        END:VCALENDAR
        BEGIN:VCALENDAR
        """,
        TestName = "UnclosedSecondVCALENDAR")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        PRODID:Test
        BEGIN:VEVENT
        UID:test
        END:VEVENT
        """,
        TestName = "UnclosedCalendar")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        PRODID:Test
        END:VCALENDAR
        BEGIN:VEVENT
        """,
        TestName = "UnexpectedType_VEVENT")]
    public void UnclosedComponent_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.DeserializeCollection<CalendarComponent>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.StartsWith("Missing end of component"));
    }

    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        PRODID:Test
        END:VCALENDAR
        BEGIN:VEVENT
        """,
        TestName = "UnexpectedType_VEVENT")]
    public void UnexpectedComponent_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.DeserializeCollection<Calendar>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.StartsWith("Unexpected component type"));
    }

    [Test, Category("Deserialization")]
    [TestCase("""
        SUMMARY:Test Event
        UID:test@example.com
        END:VEVENT
        """,
        TestName = "EndEvent_BeforeBegin")]
    [TestCase("""
        UID:test@example.com
        DTSTART:20230101T100000Z
        """,
        TestName = "PropertyWithoutBegin")]
    [TestCase("""
        END:VEVENT
        """,
        TestName = "EndBeforeBegin")]
    public void PropertyBeforeBegin_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("Expected start of component"));
    }

    [Test, Category("Deserialization")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        BEGIN:VEVENT
        UID:test
        END:VCALENDAR
        END:VEVENT
        """,
        TestName = "MismatchedEndTag")]
    [TestCase("""
        BEGIN:VCALENDAR
        BEGIN:VEVENT
        UID:test
        END:VTODO
        END:VCALENDAR
        """,
        TestName = "WrongComponentEndTag")]


    [TestCase("""
        BEGIN:VCALENDAR
        BEGIN:VEVENT
        END:
        END:VCALENDAR
        """,
        TestName = "EndWithEmptyValue")]
    [TestCase("""
        BEGIN:VCALENDAR
        END:
        """,
        TestName = "CalendarEndWithEmptyValue")]
    public void MismatchedEndTag_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.StartsWith("Unmatched END"));
    }

    [TestCase("""
        BEGIN:VCALENDAR
        BEGIN:
        END:VCALENDAR
        """,
        TestName = "NestedBeginWithEmptyValue")]
    [TestCase("""
        BEGIN:
        END:VCALENDAR
        """,
        TestName = "BeginWithEmptyValue")]
    public void MissingComponentName_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.StartsWith("Missing component name"));
    }

    [Test, Category("Deserialization")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION 2.0
        END:VCALENDAR
        """,
        TestName = "MissingColon")]
    [TestCase("""
        BEGIN:VCALENDAR
        PRODID
        END:VCALENDAR
        """,
        TestName = "BlankProperty")]
    [TestCase("""
        BEGIN
        END:VCALENDAR
        """,
        TestName = "BeginWithoutColon")]
    [TestCase("""
        BEGIN:VCALENDAR
        BEGIN
        END:VCALENDAR
        """,
        TestName = "NestedBeginWithoutColon")]
    [TestCase("""
        BEGIN:VCALENDAR
        BEGIN:VEVENT
        END
        END:VCALENDAR
        """,
        TestName = "EndWithoutColon")]
    [TestCase("""
        BEGIN:VCALENDAR
        END
        """,
        TestName = "CalendarEndWithoutColon")]
    public void MalformedLine_MissingName_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("name missing"));
    }

    [TestCase("""
              BEGIN:VCALENDAR
              VERSION:2.0
              BEGIN:VEVENT
              UID:test
              DTSTART;20230101T100000Z
              END:VEVENT
              """,
        TestName = "NoColonBeforePropertyValue")]
    public void MalformedLine_MissingColon_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("Failed to parse"));
    }

    [Test, Category("Deserialization")]
    [TestCase("""
        BEGIN:VCALENDAR
        BEGIN:VEVENT
        UID:test
        DTSTART;VALUE=:20230101
        END:VEVENT
        END:VCALENDAR
        """,
        TestName = "ParameterFollowedByColon")]
    public void ValidParameterSyntax_Tests(string ics)
    {
        // This should parse successfully - parameters can have empty values
        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics), Throws.Nothing);
    }

    [Test, Category("Deserialization")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        BEGIN:VEVENT
        UID:test
        DTSTART:20230101T100000Z
        END:VEVENT
        """,
        TestName = "ValidCalendarButUnclosedOuter")]
    public void ComplexUnclosedScenarios_Tests(string ics)
    {
        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.StartsWith("Missing end of component"));
    }

    [Test, Category("Deserialization")]
    public void BareCr_ShouldParseSuccessfully()
    {
        // Create a calendar with bare CR (\r) without LF (\n)
        // This uses string concatenation to ensure the actual \r character is in the string
        const string icsWithBareCr = "BEGIN:VCALENDAR\rVERSION:2.0\rEND:VCALENDAR";

        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(icsWithBareCr), Throws.Nothing);
    }

    [Test, Category("Deserialization")]
    public void BareLf_ShouldParseSuccessfully()
    {
        // Unix-style LF line endings (accepted for compatibility)
        const string icsWithLf = "BEGIN:VCALENDAR\nVERSION:2.0\nPRODID:Test\nEND:VCALENDAR";

        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(icsWithLf), Throws.Nothing);
    }

    [Test, Category("Deserialization")]
    public void CrLf_ShouldParseSuccessfully()
    {
        // Standard CRLF line endings (RFC 5545 compliant)
        const string icsWithCrLf = "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:Test\r\nEND:VCALENDAR";

        Assert.That(() => CalendarSerializer.Deserialize<Calendar>(icsWithCrLf), Throws.Nothing);
    }

    [Test, Category("Deserialization")]
    [TestCase("""
              BEGIN:VCALENDAR
              VERSION:2.0
              BEGIN:VEVENT
              UID:event123@example.com
              DTSTART:20260101T100000Z
              DTEND:20260101T110000Z
              SUMMARY:Team Meeting
              ATTENDEE;CN="John Doe;RSVP=TRUE:mailto:john@example.com
              END:VEVENT
              END:VCALENDAR
              """,
        TestName = "UnbalancedQuotes")]
    public void Unbalanced_Quotes_ShouldThrow(string ics)
        => Assert.That(() => CalendarSerializer.Deserialize<Calendar>(ics), Throws.Exception.TypeOf<SerializationException>()
            .With.Message.StartsWith("Unbalanced quotes"));
}
