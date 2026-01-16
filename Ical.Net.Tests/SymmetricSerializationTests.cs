//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.Tests;

public class SymmetricSerializationTests
{
    private const string _ldapUri = "ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)";

    private static readonly DateTime _nowTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    private static readonly DateTime _later = _nowTime.AddHours(1);
    private static CalendarSerializer GetNewSerializer() => new CalendarSerializer();
    private static string SerializeToString(Calendar c) => GetNewSerializer().SerializeToString(c)!;
    private static CalendarEvent GetSimpleEvent(bool useDtEnd = true)
    {
        var evt = new CalendarEvent { DtStart = new CalDateTime(_nowTime) };
        if (useDtEnd)
            evt.DtEnd = new CalDateTime(_later);
        else
            evt.Duration = (_later - _nowTime).ToDurationExact();

        return evt;
    }

    private static Calendar UnserializeCalendar(string s) => Calendar.Load(s)!;
    
    [Test]
    public void VTimeZoneSerialization_Test()
    {
        var originalCalendar = new Calendar();
        var tz = new VTimeZone
        {
            TzId = "New Zealand Standard Time"
        };
        originalCalendar.AddTimeZone(tz);
        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(originalCalendar)!;
        var unserializedCalendar = Calendar.Load(serializedCalendar)!;

        Assert.Multiple(() =>
        {
            Assert.That(unserializedCalendar.TimeZones, Is.EqualTo(originalCalendar.TimeZones).AsCollection);
            Assert.That(unserializedCalendar, Is.EqualTo(originalCalendar));
        });
        Assert.That(unserializedCalendar.GetHashCode(), Is.EqualTo(originalCalendar.GetHashCode()));
    }

    [Test, TestCaseSource(nameof(AttendeeSerialization_TestCases))]
    public void AttendeeSerialization_Test(Attendee attendee)
    {
        var calendar = new Calendar();
        calendar.AddTimeZone(new VTimeZone("America/Los_Angeles"));
        var someEvent = GetSimpleEvent();
        someEvent.Attendees = new List<Attendee> { attendee };
        calendar.Events.Add(someEvent);

        var serialized = SerializeToString(calendar);
        var unserialized = UnserializeCalendar(serialized);

        Assert.Multiple(() =>
        {
            Assert.That(unserialized.GetHashCode(), Is.EqualTo(calendar.GetHashCode()));
            Assert.That(calendar.Events.SequenceEqual(unserialized.Events), Is.True);
            Assert.That(unserialized, Is.EqualTo(calendar));
        });
    }

    public static IEnumerable<ITestCaseData> AttendeeSerialization_TestCases()
    {
        var complex1 = new Attendee("MAILTO:mary@example.com")
        {
            CommonName = "Mary Accepted",
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Accepted,
            SentBy = new Uri("mailto:someone@example.com"),
            DirectoryEntry = new Uri(_ldapUri),
            Type = "CuType",
            Members = new List<string> { "Group A", "Group B" },
            Role = ParticipationRole.Chair,
            DelegatedTo = new List<string> { "Peon A", "Peon B" },
            DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B" }
        };
        yield return new TestCaseData(complex1).SetName("Complex attendee");

        var simple = new Attendee("MAILTO:james@example.com")
        {
            CommonName = "James James",
            Role = ParticipationRole.RequiredParticipant,
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Tentative
        };
        yield return new TestCaseData(simple).SetName("Simple attendee");
    }

    [Test, TestCaseSource(nameof(BinaryAttachment_TestCases))]
    public void BinaryAttachment_Tests(string theString, string expectedAttachment)
    {
        var asBytes = Encoding.UTF8.GetBytes(theString);
        var binaryAttachment = new Attachment(asBytes);

        var calendar = new Calendar();
        var vEvent = GetSimpleEvent();
        vEvent.Attachments = new List<Attachment> { binaryAttachment };
        calendar.Events.Add(vEvent);

        var serialized = SerializeToString(calendar);
        var unserialized = UnserializeCalendar(serialized);
        var unserializedAttachment = unserialized
            .Events
            .First()
            .Attachments
            .Select(a => Encoding.UTF8.GetString(a.Data!))
            .First();

        Assert.Multiple(() =>
        {
            Assert.That(unserializedAttachment, Is.EqualTo(expectedAttachment));
        });
    }

    public static IEnumerable BinaryAttachment_TestCases()
    {
        const string shortString = "This is a string.";
        yield return new TestCaseData(shortString, shortString)
            .SetName("Short string");

        const string mediumString = "This is a stringThisThis is a";
        yield return new TestCaseData(mediumString, mediumString)
            .SetName("Moderate string");

        const string longishString = "This is a song that never ends. It just goes on and on my friends. Some people started singing it not...";
        yield return new TestCaseData(longishString, longishString)
            .SetName("Much longer string");

        const string jsonString =
            "{\"TheList\":[\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\"],\"TheNumber\":42,\"TheSet\":[\"Foo\",\"Bar\",\"Baz\"]}";
        yield return new TestCaseData(jsonString, jsonString).SetName("JSON-serialized text");
    }

    [Test, TestCaseSource(nameof(UriAttachment_TestCases))]
    public void UriAttachment_Tests(string uri, Uri expectedUri)
    {
        var attachment = new Attachment(uri);

        var calendar = new Calendar();
        var vEvent = GetSimpleEvent();
        vEvent.Attachments = new List<Attachment> { attachment };
        calendar.Events.Add(vEvent);

        var serialized = SerializeToString(calendar);
        var unserialized = UnserializeCalendar(serialized);
        var unserializedUri = unserialized
            .Events
            .First()
            .Attachments
            .Select(a => a.Uri)
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(unserializedUri, Is.EqualTo(expectedUri));
            Assert.That(unserialized.GetHashCode(), Is.EqualTo(calendar.GetHashCode()));
            Assert.That(unserialized, Is.EqualTo(calendar));
        });
    }

    public static IEnumerable UriAttachment_TestCases()
    {
        yield return new TestCaseData("http://www.google.com", new Uri("http://www.google.com")).SetName("HTTP URL");
        yield return new TestCaseData("mailto:rstockbower@gmail.com", new Uri("mailto:rstockbower@gmail.com")).SetName("mailto: URL");
        yield return new TestCaseData(_ldapUri, new Uri(_ldapUri)).SetName("ldap URL");
        yield return new TestCaseData("C:\\path\\to\\file.txt", new Uri("C:\\path\\to\\file.txt")).SetName("Local file path URL");
        yield return new TestCaseData("\\\\uncPath\\to\\resource.txt", new Uri("\\\\uncPath\\to\\resource.txt")).SetName("UNC path URL");
    }

    [Test, TestCaseSource(nameof(CategoryTest_TestCases))]
    public void CategoriesTest(string[] categories)
    {
        var vEvent = GetSimpleEvent();
        vEvent.Categories = categories;
        var c = new Calendar();
        c.Events.Add(vEvent);

        var serialized = SerializeToString(c);
        var categoriesCount = Regex.Matches(serialized, "CATEGORIES").Count;
        Assert.That(categoriesCount, Is.EqualTo(1));
        var catString = $"CATEGORIES:{string.Join(",", categories.OfType<string>())}";
        Assert.That(serialized, Does.Contain(catString));

        var deserialized = UnserializeCalendar(serialized);
        Assert.That(deserialized, Is.EqualTo(c));
    }

    private static IEnumerable CategoryTest_TestCases()
    {
        yield return new[] { "Foo", "Bar", "Baz" };
        yield return new[] { "Hello", "world" };
    }

    [Test, TestCaseSource(nameof(ResourceTest_TestCases))]
    public void ResourceTest(string[] resources)
    {
        var vEvent = GetSimpleEvent();
        vEvent.Resources = resources;
        var c = new Calendar();
        c.Events.Add(vEvent);

        var serialized = SerializeToString(c);
        var categoriesCount = Regex.Matches(serialized, "RESOURCES").Count;
        Assert.That(categoriesCount, Is.EqualTo(1));
        var resString = $"RESOURCES:{string.Join(",", resources.OfType<string>())}";
        Assert.That(serialized, Does.Contain(resString));

        var deserialized = UnserializeCalendar(serialized);
        Assert.That(deserialized, Is.EqualTo(c));
    }

    private static IEnumerable ResourceTest_TestCases()
    {
        yield return new[] { "Foo", "Bar", "Baz" };
        yield return new[] { "Hello", "world" };
    }
}
