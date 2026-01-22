//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Runtime.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class SimpleDeserializerExceptionTests
{
    [Test, Category("Deserialization")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        PRODID:Test
        END:VCALENDAR
        BEGIN:VEVENT
        """, 
        TestName = "Unclosed_VEVENT")]
    [TestCase("""
        BEGIN:VCALENDAR
        VERSION:2.0
        PRODID:Test
        BEGIN:VEVENT
        UID:test
        END:VEVENT
        """,
        TestName = "UnclosedCalendar")]
    public void UnclosedComponent_Tests(string ics)
    {
        Assert.That(() => Calendar.Load(ics), 
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("Unclosed component"));
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
        Assert.That(() => Calendar.Load(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("Expected 'BEGIN'"));
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
        BEGIN:
        END:VCALENDAR
        """,
        TestName = "BeginWithEmptyValue")]
    [TestCase("""
        BEGIN:VCALENDAR
        BEGIN:
        END:VCALENDAR
        """,
        TestName = "NestedBeginWithEmptyValue")]
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
        Assert.That(() => Calendar.Load(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.StartsWith("Expected 'END:"));
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
        Assert.That(() => Calendar.Load(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("missing name"));
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
        Assert.That(() => Calendar.Load(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("missing colon"));
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
        Assert.That(() => Calendar.Load(ics), Throws.Nothing);
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
        Assert.That(() => Calendar.Load(ics),
            Throws.Exception.TypeOf<SerializationException>()
                .With.Message.Contains("Unclosed component"));
    }

    [Test, Category("Deserialization")]
    public void BareCr_ShouldParseSuccessfully()
    {
        // Create a calendar with bare CR (\r) without LF (\n)
        // This uses string concatenation to ensure the actual \r character is in the string
        const string icsWithBareCr = "BEGIN:VCALENDAR\rVERSION:2.0\rEND:VCALENDAR";

        Assert.That(() => Calendar.Load(icsWithBareCr), Throws.Nothing);
    }

    [Test, Category("Deserialization")]
    public void BareLf_ShouldParseSuccessfully()
    {
        // Unix-style LF line endings (accepted for compatibility)
        const string icsWithLf = "BEGIN:VCALENDAR\nVERSION:2.0\nPRODID:Test\nEND:VCALENDAR";

        Assert.That(() => Calendar.Load(icsWithLf), Throws.Nothing);
    }

    [Test, Category("Deserialization")]
    public void CrLf_ShouldParseSuccessfully()
    {
        // Standard CRLF line endings (RFC 5545 compliant)
        const string icsWithCrLf = "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:Test\r\nEND:VCALENDAR";
        
        Assert.That(() => Calendar.Load(icsWithCrLf), Throws.Nothing);
    }
}

