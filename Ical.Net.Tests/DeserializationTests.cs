//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture, Category("Deserialization")]
public class DeserializationTests
{
    [Test]
    public void Attendee1()
    {
        var iCal = Calendar.Load(IcsFiles.Attendee1);
        Assert.That(iCal.Events, Has.Count.EqualTo(1));

        var evt = iCal.Events.First();
        // Ensure there are 2 attendees
        Assert.That(evt.Attendees, Has.Count.EqualTo(2));

        var attendee1 = evt.Attendees[0];
        var attendee2 = evt.Attendees[1];

        Assert.Multiple(() =>
        {
            // Values
            Assert.That(attendee1.Value, Is.EqualTo(new Uri("mailto:joecool@example.com")));
            Assert.That(attendee2.Value, Is.EqualTo(new Uri("mailto:ildoit@example.com")));

            // MEMBERS
            Assert.That(attendee1.Members, Has.Count.EqualTo(1));
            Assert.That(attendee2.Members.Count, Is.EqualTo(0));
        });
        Assert.Multiple(() =>
        {
            Assert.That(attendee1.Members[0], Is.EqualTo(new Uri("mailto:DEV-GROUP@example.com").ToString()));

            // DELEGATED-FROM
            Assert.That(attendee1.DelegatedFrom.Count, Is.EqualTo(0));
            Assert.That(attendee2.DelegatedFrom, Has.Count.EqualTo(1));
            Assert.That(attendee2.DelegatedFrom[0], Is.EqualTo(new Uri("mailto:immud@example.com").ToString()));
        });
        Assert.Multiple(() =>
        {
            // DELEGATED-TO
            Assert.That(attendee1.DelegatedTo.Count, Is.EqualTo(0));
            Assert.That(attendee2.DelegatedTo.Count, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Tests that multiple parameters of the
    /// same name are correctly aggregated into
    /// a single list.
    /// </summary>
    [Test]
    public void Attendee2()
    {
        var iCal = Calendar.Load(IcsFiles.Attendee2);
        Assert.That(iCal.Events, Has.Count.EqualTo(1));

        var evt = iCal.Events.First();
        // Ensure there is 1 attendee
        Assert.That(evt.Attendees, Has.Count.EqualTo(1));

        var attendee1 = evt.Attendees;

        // Values
        Assert.That(attendee1[0].Value, Is.EqualTo(new Uri("mailto:joecool@example.com")));

        Assert.Multiple(() =>
        {
            // MEMBERS
            Assert.That(attendee1[0].Members, Has.Count.EqualTo(3));
            Assert.That(attendee1[0].Members[0], Is.EqualTo(new Uri("mailto:DEV-GROUP@example.com").ToString()));
            Assert.That(attendee1[0].Members[1], Is.EqualTo(new Uri("mailto:ANOTHER-GROUP@example.com").ToString()));
            Assert.That(attendee1[0].Members[2], Is.EqualTo(new Uri("mailto:THIRD-GROUP@example.com").ToString()));
        });
    }

    /// <summary>
    /// Tests that Lotus Notes-style properties are properly handled.
    /// https://sourceforge.net/tracker/?func=detail&aid=2033495&group_id=187422&atid=921236
    /// Sourceforge bug #2033495
    /// </summary>
    [Test]
    public void Bug2033495()
    {
        var iCal = Calendar.Load(IcsFiles.Bug2033495);
        Assert.Multiple(() =>
        {
            Assert.That(iCal.Events, Has.Count.EqualTo(1));
            Assert.That(iCal.Properties["X-LOTUS-CHILD_UID"].Value, Is.EqualTo("XXX"));
        });
    }

    /// <summary>
    /// Tests bug #2938007 - involving the HasTime property in IDateTime values.
    /// See https://sourceforge.net/tracker/?func=detail&aid=2938007&group_id=187422&atid=921236
    /// </summary>
    [Test]
    public void Bug2938007()
    {
        var iCal = Calendar.Load(IcsFiles.Bug2938007);
        Assert.That(iCal.Events, Has.Count.EqualTo(1));

        var evt = iCal.Events.First();
        Assert.Multiple(() =>
        {
            Assert.That(evt.Start.HasTime, Is.EqualTo(true));
            Assert.That(evt.End.HasTime, Is.EqualTo(true));
        });

        foreach (var o in evt.GetOccurrences(new CalDateTime(2010, 1, 17, 0, 0, 0), new CalDateTime(2010, 2, 1, 0, 0, 0)))
        {
            Assert.Multiple(() =>
            {
                Assert.That(o.Period.StartTime.HasTime, Is.EqualTo(true));
                Assert.That(o.Period.EndTime.HasTime, Is.EqualTo(true));
            });
        }
    }

    /// <summary>
    /// Tests bug #3177278 - Serialize closes stream
    /// See https://sourceforge.net/tracker/?func=detail&aid=3177278&group_id=187422&atid=921236
    /// </summary>
    [Test]
    public void Bug3177278()
    {
        var calendar = new Calendar();
        var serializer = new CalendarSerializer();

        var ms = new MemoryStream();
        serializer.Serialize(calendar, ms, Encoding.UTF8);

        Assert.That(ms.CanWrite, Is.True);
    }

    /// <summary>
    /// Tests that a mixed-case VERSION property is loaded properly
    /// </summary>
    [Test]
    public void CaseInsensitive4()
    {
        var iCal = Calendar.Load(IcsFiles.CaseInsensitive4);
        Assert.That(iCal.Version, Is.EqualTo("2.5"));
    }

    [Test]
    public void Categories1_2()
    {
        var iCal = Calendar.Load(IcsFiles.Categories1);
        ProgramTest.TestCal(iCal);
        var evt = iCal.Events.First();

        var items = new List<string>();
        items.AddRange(new[]
        {
            "One", "Two", "Three",
            "Four", "Five", "Six",
            "Seven", "A string of text with nothing less than a comma, semicolon; and a newline\n."
        });

        var found = new Dictionary<string, bool>();
        foreach (var s in evt.Categories.Where(s => items.Contains(s)))
        {
            found[s] = true;
        }

        foreach (string item in items)
        {
            Assert.That(found.ContainsKey(item), Is.True, "Event should contain CATEGORY '" + item + "', but it was not found.");
        }
    }

    [Test]
    public void EmptyLines1()
    {
        var iCal = Calendar.Load(IcsFiles.EmptyLines1);
        Assert.That(iCal.Events, Has.Count.EqualTo(2), "iCalendar should have 2 events");
    }

    [Test]
    public void EmptyLines2()
    {
        var calendars = CalendarCollection.Load(IcsFiles.EmptyLines2);
        Assert.That(calendars, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(calendars[0].Events, Has.Count.EqualTo(2), "iCalendar should have 2 events");
            Assert.That(calendars[1].Events, Has.Count.EqualTo(2), "iCalendar should have 2 events");
        });
    }

    /// <summary>
    /// Verifies that blank lines between components are allowed
    /// (as occurs with some applications/parsers - i.e. KOrganizer)
    /// </summary>
    [Test]
    public void EmptyLines3()
    {
        var iCal = Calendar.Load(IcsFiles.EmptyLines3);
        Assert.That(iCal.Todos, Has.Count.EqualTo(1), "iCalendar should have 1 todo");
    }

    /// <summary>
    /// Similar to PARSE4 and PARSE5 tests.
    /// </summary>
    [Test]
    public void EmptyLines4()
    {
        var iCal = Calendar.Load(IcsFiles.EmptyLines4);
        Assert.That(iCal.Events, Has.Count.EqualTo(28));
    }

    [Test]
    public void Encoding2()
    {
        var iCal = Calendar.Load(IcsFiles.Encoding2);
        ProgramTest.TestCal(iCal);
        var evt = iCal.Events.First();

        Assert.That(
    evt.Attachments[0].ToString(),
   Is.EqualTo("This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large.\r\n" +
            "This is a test to try out base64 encoding without being too large."),
    "Attached value does not match.");
    }

    [Test]
    public void Encoding3()
    {
        var iCal = Calendar.Load(IcsFiles.Encoding3);
        ProgramTest.TestCal(iCal);
        var evt = iCal.Events.First();

        Assert.Multiple(() =>
        {
            Assert.That(evt.Uid, Is.EqualTo("uuid1153170430406"), "UID should be 'uuid1153170430406'; it is " + evt.Uid);
            Assert.That(evt.Sequence, Is.EqualTo(1), "SEQUENCE should be 1; it is " + evt.Sequence);
        });
    }

    [Test]
    public void Event8()
    {
        var sr = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Apple Computer\, Inc//iCal 1.0//EN
CALSCALE:GREGORIAN
BEGIN:VEVENT
CREATED:20070404T211714Z
DTEND:20070407T010000Z
DTSTAMP:20070404T211714Z
DTSTART:20070406T230000Z
DURATION:PT2H
RRULE:FREQ=WEEKLY;UNTIL=20070801T070000Z;BYDAY=FR
SUMMARY:Friday Meetings
DTSTAMP:20040103T033800Z
SEQUENCE:1
UID:fd940618-45e2-4d19-b118-37fd7a8e3906
END:VEVENT
BEGIN:VEVENT
CREATED:20070404T204310Z
DTEND:20070416T030000Z
DTSTAMP:20070404T204310Z
DTSTART:20070414T200000Z
DURATION:P1DT7H
RRULE:FREQ=DAILY;COUNT=12;BYDAY=SA,SU
SUMMARY:Weekend Yea!
DTSTAMP:20040103T033800Z
SEQUENCE:1
UID:ebfbd3e3-cc1e-4a64-98eb-ced2598b3908
END:VEVENT
END:VCALENDAR
";
        var iCal = Calendar.Load(sr);
        Assert.That(iCal.Events.Count == 2, Is.True, "There should be 2 events in the parsed calendar");
        Assert.That(iCal.Events["fd940618-45e2-4d19-b118-37fd7a8e3906"], Is.Not.Null, "Event fd940618-45e2-4d19-b118-37fd7a8e3906 should exist in the calendar");
        Assert.That(iCal.Events["ebfbd3e3-cc1e-4a64-98eb-ced2598b3908"], Is.Not.Null, "Event ebfbd3e3-cc1e-4a64-98eb-ced2598b3908 should exist in the calendar");
    }

    [Test]
    public void GeographicLocation1_2()
    {
        var iCal = Calendar.Load(IcsFiles.GeographicLocation1);
        ProgramTest.TestCal(iCal);
        var evt = iCal.Events.First();

        Assert.Multiple(() =>
        {
            Assert.That(evt.GeographicLocation.Latitude, Is.EqualTo(37.386013), "Latitude should be 37.386013; it is not.");
            Assert.That(evt.GeographicLocation.Longitude, Is.EqualTo(-122.082932), "Longitude should be -122.082932; it is not.");
        });
    }

    [Test]
    public void Google1()
    {
        var tzId = "Europe/Berlin";
        var iCal = Calendar.Load(IcsFiles.Google1);
        var evt = iCal.Events["594oeajmftl3r9qlkb476rpr3c@google.com"];
        Assert.That(evt, Is.Not.Null);

        IDateTime dtStart = new CalDateTime(2006, 12, 18, tzId);
        IDateTime dtEnd = new CalDateTime(2006, 12, 23, tzId);
        var occurrences = iCal.GetOccurrences(dtStart, dtEnd).OrderBy(o => o.Period.StartTime).ToList();

        var dateTimes = new[]
        {
            new CalDateTime(2006, 12, 18, 7, 0, 0, tzId),
            new CalDateTime(2006, 12, 19, 7, 0, 0, tzId),
            new CalDateTime(2006, 12, 20, 7, 0, 0, tzId),
            new CalDateTime(2006, 12, 21, 7, 0, 0, tzId),
            new CalDateTime(2006, 12, 22, 7, 0, 0, tzId)
        };

        for (var i = 0; i < dateTimes.Length; i++)
            Assert.That(occurrences[i].Period.StartTime, Is.EqualTo(dateTimes[i]), "Event should occur at " + dateTimes[i]);

        Assert.That(occurrences, Has.Count.EqualTo(dateTimes.Length), "There should be exactly " + dateTimes.Length + " occurrences; there were " + occurrences.Count);
    }

    /// <summary>
    /// Tests that valid RDATE properties are parsed correctly.
    /// </summary>
    [Test]
    public void RecurrenceDates1()
    {
        var iCal = Calendar.Load(IcsFiles.RecurrenceDates1);
        Assert.That(iCal.Events, Has.Count.EqualTo(1));
        Assert.That(iCal.Events.First().RecurrenceDates, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(iCal.Events.First().RecurrenceDates[0][0].StartTime, Is.EqualTo((CalDateTime)new DateTime(1997, 7, 14, 12, 30, 0, DateTimeKind.Utc)));
            Assert.That(iCal.Events.First().RecurrenceDates[1][0].StartTime, Is.EqualTo((CalDateTime)new DateTime(1996, 4, 3, 2, 0, 0, DateTimeKind.Utc)));
            Assert.That(iCal.Events.First().RecurrenceDates[1][0].EndTime, Is.EqualTo((CalDateTime)new DateTime(1996, 4, 3, 4, 0, 0, DateTimeKind.Utc)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][0].StartTime, Is.EqualTo(new CalDateTime(1997, 1, 1)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][1].StartTime, Is.EqualTo(new CalDateTime(1997, 1, 20)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][2].StartTime, Is.EqualTo(new CalDateTime(1997, 2, 17)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][3].StartTime, Is.EqualTo(new CalDateTime(1997, 4, 21)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][4].StartTime, Is.EqualTo(new CalDateTime(1997, 5, 26)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][5].StartTime, Is.EqualTo(new CalDateTime(1997, 7, 4)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][6].StartTime, Is.EqualTo(new CalDateTime(1997, 9, 1)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][7].StartTime, Is.EqualTo(new CalDateTime(1997, 10, 14)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][8].StartTime, Is.EqualTo(new CalDateTime(1997, 11, 28)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][9].StartTime, Is.EqualTo(new CalDateTime(1997, 11, 29)));
            Assert.That(iCal.Events.First().RecurrenceDates[2][10].StartTime, Is.EqualTo(new CalDateTime(1997, 12, 25)));
        });
    }

    /// <summary>
    /// Tests that valid REQUEST-STATUS properties are parsed correctly.
    /// </summary>
    [Test]
    public void RequestStatus1()
    {
        var iCal = Calendar.Load(IcsFiles.RequestStatus1);
        Assert.That(iCal.Events, Has.Count.EqualTo(1));
        Assert.That(iCal.Events.First().RequestStatuses, Has.Count.EqualTo(4));

        var rs = iCal.Events.First().RequestStatuses[0];
        Assert.Multiple(() =>
        {
            Assert.That(rs.StatusCode.Primary, Is.EqualTo(2));
            Assert.That(rs.StatusCode.Secondary, Is.EqualTo(0));
            Assert.That(rs.Description, Is.EqualTo("Success"));
        });
        Assert.That(rs.ExtraData, Is.Null);

        rs = iCal.Events.First().RequestStatuses[1];
        Assert.Multiple(() =>
        {
            Assert.That(rs.StatusCode.Primary, Is.EqualTo(3));
            Assert.That(rs.StatusCode.Secondary, Is.EqualTo(1));
            Assert.That(rs.Description, Is.EqualTo("Invalid property value"));
            Assert.That(rs.ExtraData, Is.EqualTo("DTSTART:96-Apr-01"));
        });

        rs = iCal.Events.First().RequestStatuses[2];
        Assert.Multiple(() =>
        {
            Assert.That(rs.StatusCode.Primary, Is.EqualTo(2));
            Assert.That(rs.StatusCode.Secondary, Is.EqualTo(8));
            Assert.That(rs.Description, Is.EqualTo(" Success, repeating event ignored. Scheduled as a single event."));
            Assert.That(rs.ExtraData, Is.EqualTo("RRULE:FREQ=WEEKLY;INTERVAL=2"));
        });

        rs = iCal.Events.First().RequestStatuses[3];
        Assert.Multiple(() =>
        {
            Assert.That(rs.StatusCode.Primary, Is.EqualTo(4));
            Assert.That(rs.StatusCode.Secondary, Is.EqualTo(1));
            Assert.That(rs.Description, Is.EqualTo("Event conflict. Date/time is busy."));
        });
        Assert.That(rs.ExtraData, Is.Null);
    }

    /// <summary>
    /// Tests that string escaping works with Text elements.
    /// </summary>
    [Test]
    public void String2()
    {
        var serializer = new StringSerializer();
        var value = @"test\with\;characters";
        var unescaped = (string)serializer.Deserialize(new StringReader(value));

        Assert.That(unescaped, Is.EqualTo(@"test\with;characters"), "String unescaping was incorrect.");

        value = @"C:\Path\To\My\New\Information";
        unescaped = (string)serializer.Deserialize(new StringReader(value));
        Assert.That(unescaped, Is.EqualTo("C:\\Path\\To\\My\new\\Information"), "String unescaping was incorrect.");

        value = @"\""This\r\nis\Na\, test\""\;\\;,";
        unescaped = (string)serializer.Deserialize(new StringReader(value));

        Assert.That(unescaped, Is.EqualTo("\"This\\r\nis\na, test\";\\;,"), "String unescaping was incorrect.");
    }

    [Test]
    public void Transparency2()
    {
        var iCal = Calendar.Load(IcsFiles.Transparency2);

        Assert.That(iCal.Events, Has.Count.EqualTo(1));
        var evt = iCal.Events.First();

        Assert.That(evt.Transparency, Is.EqualTo(TransparencyType.Transparent));
    }

    [Test]
    public void DateTime1_Unrepresentable_DateTimeArgs_ShouldThrow()
    {
        Assert.That(() =>
        {
            _ = Calendar.Load(IcsFiles.DateTime1);
        }, Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Language4()
    {
        var iCal = Calendar.Load(IcsFiles.Language4);
        Assert.That(iCal, Is.Not.Null);
    }

    [Test]
    public void Outlook2007_LineFolds1()
    {
        var iCal = Calendar.Load(IcsFiles.Outlook2007LineFolds);
        var events = iCal.GetOccurrences(new CalDateTime(2009, 06, 20), new CalDateTime(2009, 06, 22));
        Assert.That(events, Has.Count.EqualTo(1));
    }

    [Test]
    public void Outlook2007_LineFolds2()
    {
        var longName = "The Exceptionally Long Named Meeting Room Whose Name Wraps Over Several Lines When Exported From Leading Calendar and Office Software Application Microsoft Office 2007";
        var iCal = Calendar.Load(IcsFiles.Outlook2007LineFolds);
        var events = iCal.GetOccurrences<CalendarEvent>(new CalDateTime(2009, 06, 20), new CalDateTime(2009, 06, 22)).OrderBy(o => o.Period.StartTime).ToList();
        Assert.That(((CalendarEvent)events[0].Source).Location, Is.EqualTo(longName));
    }

    /// <summary>
    /// Tests that multiple parameters are allowed in iCalObjects
    /// </summary>
    [Test]
    public void Parameter1()
    {
        var iCal = Calendar.Load(IcsFiles.Parameter1);

        var evt = iCal.Events.First();
        IList<CalendarParameter> parms = evt.Properties["DTSTART"].Parameters.AllOf("VALUE").ToList();
        Assert.Multiple(() =>
        {
            Assert.That(parms, Has.Count.EqualTo(2));
            Assert.That(parms[0].Values.First(), Is.EqualTo("DATE"));
            Assert.That(parms[1].Values.First(), Is.EqualTo("OTHER"));
        });
    }

    /// <summary>
    /// Tests that empty parameters are allowed in iCalObjects
    /// </summary>
    [Test]
    public void Parameter2()
    {
        var iCal = Calendar.Load(IcsFiles.Parameter2);
        Assert.That(iCal.Events, Has.Count.EqualTo(2));
    }

    /// <summary>
    /// Tests a calendar that should fail to properly parse.
    /// </summary>
    [Test]
    public void Parse1()
    {
        Assert.That(() =>
        {
            var content = IcsFiles.Parse1;
            var iCal = Calendar.Load(content);
        }, Throws.Exception.TypeOf<SerializationException>());
    }

    /// <summary>
    /// Tests that multiple properties are allowed in iCalObjects
    /// </summary>
    [Test]
    public void Property1()
    {
        var iCal = Calendar.Load(IcsFiles.Property1);

        IList<ICalendarProperty> props = iCal.Properties.AllOf("VERSION").ToList();
        Assert.That(props, Has.Count.EqualTo(2));

        for (var i = 0; i < props.Count; i++)
        {
            Assert.That(props[i].Value, Is.EqualTo("2." + i));
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void KeepApartDtEndAndDuration_Tests(bool useDtEnd)
    {
        var calStr = $@"BEGIN:VCALENDAR
BEGIN:VEVENT
DTSTART:20070406T230000Z
{(useDtEnd ? "DTEND:20070407T010000Z" : "DURATION:PT1H")}
END:VEVENT
END:VCALENDAR
";

        var calendar = Calendar.Load(calStr);

        Assert.Multiple(() =>
        {
            Assert.That(calendar.Events.Single().DtEnd != null, Is.EqualTo(useDtEnd));
            Assert.That(calendar.Events.Single().Duration != default, Is.EqualTo(!useDtEnd));
        });
    }
}
