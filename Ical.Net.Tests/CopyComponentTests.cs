//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// Tests for deep copying of ICal components.
/// </summary>
[TestFixture]
public class CopyComponentTests
{
    private static readonly DateTime _now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    private static readonly DateTime _later = _now.AddHours(1);

    private static CalendarEvent GetSimpleEvent() => new CalendarEvent
    {
        DtStart = CalDateTime.FromDateTime(_now),
        DtEnd = CalDateTime.FromDateTime(_later),
    };

    private static string SerializeEvent(CalendarEvent e) => new CalendarSerializer().SerializeToString(new Calendar { Events = { e } })!;

    [Test]
    public void CopyCalendarEventTest()
    {
        var orig = GetSimpleEvent();
        orig.Uid = "Hello";
        orig.Summary = "Original summary";
        orig.Resources = new[] { "A", "B" };
        orig.GeographicLocation = new GeographicLocation(48.210033, 16.363449);
        orig.Transparency = TransparencyType.Opaque;
        orig.Attachments.Add(new Attachment("https://original.org/"));
        var copy = orig.Copy<CalendarEvent>()!;

        copy.Uid = "Goodbye";
        copy.Summary = "Copy summary";

        var resourcesCopyFromOrig = new List<string>(copy.Resources);
        copy.Resources = new[] { "C", "D" };
        copy.Attachments[0].Uri = new Uri("https://copy.org/");
        const string uidPattern = "UID:";
        var serializedOrig = SerializeEvent(orig);
        var serializedCopy = SerializeEvent(copy);

        using (Assert.EnterMultipleScope())
        {
            // Should be a deep copy and changes only apply to the copy instance
            Assert.That(copy.Uid, Is.Not.EqualTo(orig.Uid));
            Assert.That(copy.Summary, Is.Not.EqualTo(orig.Summary));
            Assert.That(copy.Attachments[0].Uri, Is.Not.EqualTo(orig.Attachments[0].Uri));
            Assert.That(copy.Resources[0], Is.Not.EqualTo(orig.Resources[0]));

            Assert.That(resourcesCopyFromOrig, Is.EquivalentTo(orig.Resources));
            Assert.That(copy.Transparency, Is.EqualTo(orig.Transparency));

            Assert.That(Regex.Matches(serializedOrig, uidPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(100)), Has.Count.EqualTo(1));
            Assert.That(Regex.Matches(serializedCopy, uidPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(100)), Has.Count.EqualTo(1));
        }
    }

    [TestCase("ATTACH;FMTTYPE=text/plain:https://example.com/file.txt")]
    [TestCase("ATTACH:https://example.com/file.txt")]
    [TestCase("ATTENDEE:mailto:guest@example.com")]
    [TestCase("ATTENDEE;RSVP=FALSE;CN=Guest:mailto:guest@example.com")]
    [TestCase("ATTENDEE;SENT-BY=\"mailto:assistant@example.com\":mailto:guest@example.com")]
    [TestCase("URL;VALUE=URI:https://example.com/")]
    [TestCase("X-TEST;X-VALUES=first,second:value")]
    public void CopyCalendarPreservesParameters(string property)
    {
        var original = Calendar.Load(string.Join("\r\n", new[]
        {
            "BEGIN:VCALENDAR", "VERSION:2.0", "PRODID:-//Copy test//EN",
            "BEGIN:VEVENT", "UID:copy-test", "DTSTAMP:20260101T000000Z",
            property, "END:VEVENT", "END:VCALENDAR", ""
        }))!;
        var serializer = new CalendarSerializer();
        var beforeCopy = serializer.SerializeToString(original);

        var copy = original.Copy<Calendar>()!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serializer.SerializeToString(copy), Is.EqualTo(beforeCopy));
            Assert.That(serializer.SerializeToString(original), Is.EqualTo(beforeCopy));
        }
    }

    [Test]
    public void CopyCalendarIsolatesAttachmentParameters()
    {
        var original = Calendar.Load(string.Join("\r\n", new[]
        {
            "BEGIN:VCALENDAR", "VERSION:2.0", "PRODID:-//Copy test//EN",
            "BEGIN:VEVENT", "UID:copy-test", "DTSTAMP:20260101T000000Z",
            "ATTACH;FMTTYPE=text/plain:https://example.com/file.txt",
            "END:VEVENT", "END:VCALENDAR", ""
        }))!;
        var copy = original.Copy<Calendar>()!;
        var originalAttachment = original.Events.First().Attachments[0];
        var copiedAttachment = copy.Events.First().Attachments[0];

        copiedAttachment.FormatType = "application/pdf";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(originalAttachment.FormatType, Is.EqualTo("text/plain"));
            Assert.That(copiedAttachment.FormatType, Is.EqualTo("application/pdf"));
            Assert.That(copy.Events.First().Properties["ATTACH"]!.Parameters.Get("FMTTYPE"), Is.EqualTo("application/pdf"));
            Assert.That(copiedAttachment.Calendar, Is.SameAs(copy));
        }
    }

    [Test]
    public void CopyPropertyIsolatesParameterValues()
    {
        var original = new CalendarProperty("X-TEST", "value");
        original.Parameters.Set("X-VALUES", new[] { "first", "second" });
        var copy = original.Copy<CalendarProperty>()!;

        Assert.That(copy.Parameters.GetMany("X-VALUES"), Is.EquivalentTo(new[] { "first", "second" }));
        copy.Parameters.GetMany("X-VALUES").Add("third");
        original.Parameters.GetMany("X-VALUES").Remove("first");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(original.Parameters.GetMany("X-VALUES"), Is.EquivalentTo(new[] { "second" }));
            Assert.That(copy.Parameters.GetMany("X-VALUES"), Is.EquivalentTo(new[] { "first", "second", "third" }));
        }
    }

    [Test]
    public void CopyDataTypeIsolatesParameters()
    {
        var original = new Organizer { CommonName = "Original", Value = new Uri("mailto:host@example.com") };
        original.Parameters.Set("X-VALUES", new[] { "first", "second" });
        var copy = original.Copy<Organizer>()!;

        copy.CommonName = "Copy";
        copy.Parameters.GetMany("X-VALUES").Add("third");
        original.Parameters.GetMany("X-VALUES").Remove("first");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(original.CommonName, Is.EqualTo("Original"));
            Assert.That(original.Parameters.GetMany("X-VALUES"), Is.EquivalentTo(new[] { "second" }));
            Assert.That(copy.Parameters.GetMany("X-VALUES"), Is.EquivalentTo(new[] { "first", "second", "third" }));
        }
    }

    [Test]
    public void CopyAttendeeReplacesCachedParameters()
    {
        var original = new Attendee("mailto:guest@example.com");
        original.Parameters.Set("CN", "Original");
        original.Parameters.Set("MEMBER", new[] { "mailto:group@example.com" });
        var copy = new Attendee("mailto:other@example.com")
        {
            CommonName = "Other", Rsvp = true, Members = new[] { "mailto:other-group@example.com" }
        };

        copy.CopyFrom(original);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(copy.CommonName, Is.EqualTo("Original"));
            Assert.That(copy.Rsvp, Is.False);
            Assert.That(copy.Parameters.ContainsKey("RSVP"), Is.False);
            Assert.That(copy.Members, Is.EquivalentTo(original.Members));
            Assert.That(copy.Value, Is.EqualTo(original.Value));
        }
    }

    [Test]
    public void CopyFreeBusyTest()
    {
        var orig = new FreeBusy
        {
            Start = CalDateTime.FromDateTime(_now),
            End = CalDateTime.FromDateTime(_later),
            Entries = { new FreeBusyEntry(new Period(new CalDateTime(2024, 10, 1), Duration.FromDays(1)), FreeBusyStatus.Busy) { Language = "English" }}
        };

        var copy = orig.Copy<FreeBusy>()!;

        using (Assert.EnterMultipleScope())
        {
            // Start/DtStart and End/DtEnd are the same
            Assert.That(copy.Start, Is.EqualTo(orig.DtStart));
            Assert.That(copy.End, Is.EqualTo(orig.DtEnd));
            Assert.That(copy.Entries[0].Language, Is.EqualTo(orig.Entries[0].Language));
            Assert.That(copy.Entries[0].StartTime, Is.EqualTo(orig.Entries[0].StartTime));
            Assert.That(copy.Entries[0].Duration, Is.EqualTo(orig.Entries[0].Duration));
            Assert.That(copy.Entries[0].Status, Is.EqualTo(orig.Entries[0].Status));
        }
    }

    [Test]
    public void CopyAlarmTest()
    {
        var orig = new Alarm
        {
            Action = AlarmAction.Display,
            Trigger = new Trigger(Duration.FromMinutes(15)),
            Description = "Test Alarm"
        };

        var copy = orig.Copy<Alarm>()!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(copy.Action, Is.EqualTo(orig.Action));
            Assert.That(copy.Trigger?.DateTime, Is.EqualTo(orig.Trigger.DateTime));
            Assert.That(copy.Description, Is.EqualTo(orig.Description));
        }
    }

    [Test]
    public void CopyTodoTest()
    {
        var orig = new Todo
        {
            Summary = "Test Todo",
            Description = "This is a test todo",
            Due = CalDateTime.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).AddDays(10)),
            Priority = 1,
            Contacts = new[] { "John", "Paul" },
            Status = "NeedsAction"
        };

        var copy = orig.Copy<Todo>()!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(copy.Summary, Is.EqualTo(orig.Summary));
            Assert.That(copy.Description, Is.EqualTo(orig.Description));
            Assert.That(copy.Due, Is.EqualTo(orig.Due));
            Assert.That(copy.Priority, Is.EqualTo(orig.Priority));
            Assert.That(copy.Contacts, Is.EquivalentTo(orig.Contacts));
            Assert.That(copy.Status, Is.EqualTo(orig.Status));
        }
    }

    [Test]
    public void CopyJournalTest()
    {
        var orig = new Journal
        {
            Summary = "Test Journal",
            Description = "This is a test journal",
            DtStart = CalDateTime.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)),
            Categories = new List<string> { "Category1", "Category2" },
            Priority = 1,
            Status = "Draft"
        };

        var copy = orig.Copy<Journal>()!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(copy.Summary, Is.EqualTo(orig.Summary));
            Assert.That(copy.Description, Is.EqualTo(orig.Description));
            Assert.That(copy.DtStart, Is.EqualTo(orig.DtStart));
            Assert.That(copy.Categories, Is.EquivalentTo(orig.Categories));
            Assert.That(copy.Priority, Is.EqualTo(orig.Priority));
            Assert.That(copy.Status, Is.EqualTo(orig.Status));
        }
    }
}
