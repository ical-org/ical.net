//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
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
    [Test, TestCaseSource(nameof(CopyCalendarTest_TestCases)), Category("Copy tests")]
    public void CopyCalendarTest(string calendarString)
    {
        var iCal1 = Calendar.Load(calendarString);
        var iCal2 = iCal1.Copy<Calendar>();
        SerializationTests.CompareCalendars(iCal1, iCal2);
    }

    public static IEnumerable CopyCalendarTest_TestCases()
    {
        yield return new TestCaseData(IcsFiles.Attachment3).SetName("Attachment3");
        yield return new TestCaseData(IcsFiles.Bug2148092).SetName("Bug2148092");
        yield return new TestCaseData(IcsFiles.CaseInsensitive1).SetName("CaseInsensitive1");
        yield return new TestCaseData(IcsFiles.CaseInsensitive2).SetName("CaseInsensitive2");
        yield return new TestCaseData(IcsFiles.CaseInsensitive3).SetName("CaseInsensitive3");
        yield return new TestCaseData(IcsFiles.Categories1).SetName("Categories1");
        yield return new TestCaseData(IcsFiles.Duration1).SetName("Duration1");
        yield return new TestCaseData(IcsFiles.Encoding1).SetName("Encoding1");
        yield return new TestCaseData(IcsFiles.Event1).SetName("Event1");
        yield return new TestCaseData(IcsFiles.Event2).SetName("Event2");
        yield return new TestCaseData(IcsFiles.Event3).SetName("Event3");
        yield return new TestCaseData(IcsFiles.Event4).SetName("Event4");
        yield return new TestCaseData(IcsFiles.GeographicLocation1).SetName("GeographicLocation1");
        yield return new TestCaseData(IcsFiles.Language1).SetName("Language1");
        yield return new TestCaseData(IcsFiles.Language2).SetName("Language2");
        yield return new TestCaseData(IcsFiles.Language3).SetName("Language3");
        yield return new TestCaseData(IcsFiles.TimeZone1).SetName("TimeZone1");
        yield return new TestCaseData(IcsFiles.TimeZone2).SetName("TimeZone2");
        yield return new TestCaseData(IcsFiles.TimeZone3).SetName("TimeZone3");
        yield return new TestCaseData(IcsFiles.XProperty1).SetName("XProperty1");
        yield return new TestCaseData(IcsFiles.XProperty2).SetName("XProperty2");
    }

    private static readonly DateTime _now = DateTime.Now;
    private static readonly DateTime _later = _now.AddHours(1);

    private static CalendarEvent GetSimpleEvent() => new CalendarEvent
    {
        DtStart = new CalDateTime(_now),
        DtEnd = new CalDateTime(_later),
    };

    private static string SerializeEvent(CalendarEvent e) => new CalendarSerializer().SerializeToString(new Calendar { Events = { e } });

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
        var copy = orig.Copy<CalendarEvent>();

        copy.Uid = "Goodbye";
        copy.Summary = "Copy summary";

        var resourcesCopyFromOrig = new List<string>(copy.Resources);
        copy.Resources = new[] { "C", "D" };
        copy.Attachments[0].Uri = new Uri("https://copy.org/");
        const string uidPattern = "UID:";
        var serializedOrig = SerializeEvent(orig);
        var serializedCopy = SerializeEvent(copy);

        Assert.Multiple(() =>
        {
            // Should be a deep copy and changes only apply to the copy instance
            Assert.That(copy.Uid, Is.Not.EqualTo(orig.Uid));
            Assert.That(copy.Summary, Is.Not.EqualTo(orig.Summary));
            Assert.That(copy.Attachments[0].Uri, Is.Not.EqualTo(orig.Attachments[0].Uri));
            Assert.That(copy.Resources[0], Is.Not.EqualTo(orig.Resources[0]));

            Assert.That(resourcesCopyFromOrig, Is.EquivalentTo(orig.Resources));
            Assert.That(copy.GeographicLocation, Is.EqualTo(orig.GeographicLocation));
            Assert.That(copy.Transparency, Is.EqualTo(orig.Transparency));

            Assert.That(Regex.Matches(serializedOrig, uidPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(100)), Has.Count.EqualTo(1));
            Assert.That(Regex.Matches(serializedCopy, uidPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(100)), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void CopyFreeBusyTest()
    {
        var orig = new FreeBusy
        {
            Start = new CalDateTime(_now),
            End = new CalDateTime(_later),
            Entries = { new FreeBusyEntry { Language = "English", StartTime = new CalDateTime(2024, 10, 1), Duration = Duration.FromDays(1), Status = FreeBusyStatus.Busy } }
        };

        var copy = orig.Copy<FreeBusy>();

        Assert.Multiple(() =>
        {
            // Start/DtStart and End/DtEnd are the same
            Assert.That(copy.Start, Is.EqualTo(orig.DtStart));
            Assert.That(copy.End, Is.EqualTo(orig.DtEnd));
            Assert.That(copy.Entries[0].Language, Is.EqualTo(orig.Entries[0].Language));
            Assert.That(copy.Entries[0].StartTime, Is.EqualTo(orig.Entries[0].StartTime));
            Assert.That(copy.Entries[0].Duration, Is.EqualTo(orig.Entries[0].Duration));
            Assert.That(copy.Entries[0].Status, Is.EqualTo(orig.Entries[0].Status));
        });
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

        var copy = orig.Copy<Alarm>();

        Assert.Multiple(() =>
        {
            Assert.That(copy.Action, Is.EqualTo(orig.Action));
            Assert.That(copy.Trigger, Is.EqualTo(orig.Trigger));
            Assert.That(copy.Description, Is.EqualTo(orig.Description));
        });
    }

    [Test]
    public void CopyTodoTest()
    {
        var orig = new Todo
        {
            Summary = "Test Todo",
            Description = "This is a test todo",
            Due = new CalDateTime(DateTime.Now.AddDays(10)),
            Priority = 1,
            Contacts = new[] { "John", "Paul" },
            Status = "NeedsAction"
        };

        var copy = orig.Copy<Todo>();

        Assert.Multiple(() =>
        {
            Assert.That(copy.Summary, Is.EqualTo(orig.Summary));
            Assert.That(copy.Description, Is.EqualTo(orig.Description));
            Assert.That(copy.Due, Is.EqualTo(orig.Due));
            Assert.That(copy.Priority, Is.EqualTo(orig.Priority));
            Assert.That(copy.Contacts, Is.EquivalentTo(orig.Contacts));
            Assert.That(copy.Status, Is.EqualTo(orig.Status));
        });
    }

    [Test]
    public void CopyJournalTest()
    {
        var orig = new Journal
        {
            Summary = "Test Journal",
            Description = "This is a test journal",
            DtStart = new CalDateTime(DateTime.Now),
            Categories = new List<string> { "Category1", "Category2" },
            Priority = 1,
            Status = "Draft"
        };

        var copy = orig.Copy<Journal>();

        Assert.Multiple(() =>
        {
            Assert.That(copy.Summary, Is.EqualTo(orig.Summary));
            Assert.That(copy.Description, Is.EqualTo(orig.Description));
            Assert.That(copy.DtStart, Is.EqualTo(orig.DtStart));
            Assert.That(copy.Categories, Is.EquivalentTo(orig.Categories));
            Assert.That(copy.Priority, Is.EqualTo(orig.Priority));
            Assert.That(copy.Status, Is.EqualTo(orig.Status));
        });
    }
}
