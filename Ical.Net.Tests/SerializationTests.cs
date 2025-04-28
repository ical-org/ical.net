//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class SerializationTests
{
    private static readonly CalDateTime _nowTime = CalDateTime.Now;
    private static readonly CalDateTime _later = _nowTime.AddHours(1);
    private static CalendarSerializer GetNewSerializer() => new CalendarSerializer();
    private static string SerializeToString(Calendar c) => GetNewSerializer().SerializeToString(c);
    private static string SerializeToString(CalendarEvent e) => SerializeToString(new Calendar { Events = { e } });
    private static CalendarEvent GetSimpleEvent() => new CalendarEvent { DtStart = new CalDateTime(_nowTime), Duration = (_later.Value - _nowTime.Value).ToDurationExact() };
    private static Calendar DeserializeCalendar(string s) => Calendar.Load(s);

    internal static void CompareCalendars(Calendar cal1, Calendar cal2)
    {
        CompareComponents(cal1, cal2);

        Assert.That(cal2.Children, Has.Count.EqualTo(cal1.Children.Count), "Children count is different between calendars.");

        for (var i = 0; i < cal1.Children.Count; i++)
        {
            var component1 = cal1.Children[i] as ICalendarComponent;
            var component2 = cal2.Children[i] as ICalendarComponent;
            if (component1 != null && component2 != null)
            {
                CompareComponents(component1, component2);
            }
        }
    }

    internal static void CompareComponents(ICalendarComponent cb1, ICalendarComponent cb2)
    {
        foreach (var p1 in cb1.Properties)
        {
            var isMatch = false;
            foreach (var p2 in cb2.Properties.AllOf(p1.Name))
            {
                Assert.That(p2, Is.EqualTo(p1), "The properties '" + p1.Name + "' are not equal.");
                if (p1.Value is IComparable)
                {
                    if (((IComparable)p1.Value).CompareTo(p2.Value) != 0)
                        continue;
                }
                else if (p1.Value is IEnumerable)
                {
                    CompareEnumerables((IEnumerable)p1.Value, (IEnumerable)p2.Value, p1.Name);
                }
                else
                {
                    Assert.That(p2.Value, Is.EqualTo(p1.Value),
                        "The '" + p1.Name + "' property values are not equal.");
                }

                isMatch = true;
                break;
            }

            Assert.That(isMatch, Is.True, "Could not find a matching property - " + p1.Name + ":" + (p1.Value?.ToString() ?? string.Empty));
        }

        Assert.That(cb2.Children, Has.Count.EqualTo(cb1.Children.Count), "The number of children are not equal.");
        for (var i = 0; i < cb1.Children.Count; i++)
        {
            var child1 = cb1.Children[i] as ICalendarComponent;
            var child2 = cb2.Children[i] as ICalendarComponent;
            if (child1 != null && child2 != null)
            {
                CompareComponents(child1, child2);
            }
            else
            {
                Assert.That(child2, Is.EqualTo(child1), "The child objects are not equal.");
            }
        }
    }

    public static void CompareEnumerables(IEnumerable a1, IEnumerable a2, string value)
    {
        if (a1 == null && a2 == null)
        {
            return;
        }

        Assert.That((a1 == null && a2 != null) || (a1 != null && a2 == null), Is.False, value + " do not match - one item is null");

        var enum1 = a1.GetEnumerator();
        var enum2 = a2.GetEnumerator();

        while (enum1.MoveNext() && enum2.MoveNext())
        {
            Assert.That(enum2.Current, Is.EqualTo(enum1.Current), value + " do not match");
        }
    }

    public static string InspectSerializedSection(string serialized, string sectionName, IEnumerable<string> elements)
    {
        const string notFound = "expected '{0}' not found";
        var searchFor = "BEGIN:" + sectionName;
        var begin = serialized.IndexOf(searchFor, StringComparison.Ordinal);
        Assert.That(begin, Is.Not.EqualTo(-1), () => string.Format(notFound, searchFor));

        searchFor = "END:" + sectionName;
        var end = serialized.IndexOf(searchFor, begin, StringComparison.Ordinal);
        Assert.That(end, Is.Not.EqualTo(-1), () => string.Format(notFound, searchFor));

        var searchRegion = serialized.Substring(begin, end - begin + searchFor.Length);

        foreach (var e in elements)
        {
            Assert.That(searchRegion, Does.Contain(SerializationConstants.LineBreak + e + SerializationConstants.LineBreak), () => string.Format(notFound, e));
        }

        return searchRegion;
    }

    //3 formats - UTC, local time as defined in vTimeZone, and floating,
    //at some point it would be great to independently unit test string serialization of an CalDateTime object, into its 3 forms
    //http://www.kanzaki.com/docs/ical/dateTime.html
    private static string CalDateString(CalDateTime cdt)
    {
        var returnVar = $"{cdt.Year}{cdt.Month:D2}{cdt.Day:D2}T{cdt.Hour:D2}{cdt.Minute:D2}{cdt.Second:D2}";
        if (cdt.IsUtc)
        {
            return returnVar + 'Z';
        }

        return string.IsNullOrEmpty(cdt.TzId)
            ? returnVar
            : $"TZID={cdt.TzId}:{returnVar}";
    }

    //This method needs renaming
    private static Dictionary<string, string> GetValues(string serialized, string name, string value)
    {
        var lengthened = serialized.Replace(SerializationConstants.LineBreak + ' ', string.Empty);
        //using a regex for now - for the sake of speed, it may be worth creating a C# text search later
        var match = Regex.Match(lengthened, '^' + Regex.Escape(name) + "(;.+)?:" + Regex.Escape(value) + SerializationConstants.LineBreak, RegexOptions.Multiline);
        Assert.That(match.Success, Is.True, $"could not find a(n) '{name}' with value '{value}'");
        return match.Groups[1].Value.Length == 0
            ? new Dictionary<string, string>()
            : match.Groups[1].Value.Substring(1).Split(';').Select(v => v.Split('=')).ToDictionary(v => v[0], v => v.Length > 1 ? v[1] : null);
    }

    [Test, Category("Serialization"), Ignore("TODO: standard time, for NZ standard time (current example)")]
    public void TimeZoneSerialize()
    {
        var cal = new Calendar
        {
            Method = "PUBLISH",
            Version = "2.0"
        };

        const string exampleTz = "New Zealand Standard Time";
        var tzi = TimeZoneInfo.FindSystemTimeZoneById(exampleTz);
        var tz = new VTimeZone(exampleTz);
        cal.AddTimeZone(tz);
        var evt = new CalendarEvent
        {
            Summary = "Testing",
            Start = new CalDateTime(2016, 7, 14),
            End = new CalDateTime(2016, 7, 15)
        };
        cal.Events.Add(evt);

        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(cal);

        var vTimezone = InspectSerializedSection(serializedCalendar, "VTIMEZONE", new[] { "TZID:" + tz.TzId });
        var o = tzi.BaseUtcOffset.ToString("hhmm", CultureInfo.InvariantCulture);

        InspectSerializedSection(vTimezone, "STANDARD", new[] {"TZNAME:" + tzi.StandardName, "TZOFFSETTO:" + o
            //"DTSTART:20150402T030000",
            //"RRULE:FREQ=YEARLY;BYDAY=1SU;BYHOUR=3;BYMINUTE=0;BYMONTH=4",
            //"TZOFFSETFROM:+1300"
        });


        InspectSerializedSection(vTimezone, "DAYLIGHT", new[] { "TZNAME:" + tzi.DaylightName, "TZOFFSETFROM:" + o });
    }

    [Test, Category("Serialization")]
    public void SerializeDeserialize()
    {
        var cal1 = new Calendar
        {
            Method = "PUBLISH",
            Version = "2.0"
        };

        var evt = new CalendarEvent
        {
            Class = "PRIVATE",
            Created = new CalDateTime(2010, 3, 25, 12, 53, 35),
            DtStamp = new CalDateTime(2010, 3, 25, 12, 53, 35),
            LastModified = new CalDateTime(2010, 3, 27, 13, 53, 35),
            Sequence = 0,
            Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
            Priority = 5,
            Location = "here",
            Summary = "test",
            DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
            DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00)
        };
        cal1.Events.Add(evt);

        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(cal1);
        var cal2 = Calendar.Load(serializedCalendar);
        CompareCalendars(cal1, cal2);
    }

    [Test, Category("Serialization")]
    public void EventPropertiesSerialized()
    {
        var cal = new Calendar
        {
            Method = "PUBLISH",
            Version = "2.0"
        };

        var evt = new CalendarEvent
        {
            Class = "PRIVATE",
            Created = new CalDateTime(2010, 3, 25, 12, 53, 35),
            DtStamp = new CalDateTime(2010, 3, 25, 12, 53, 35),
            LastModified = new CalDateTime(2010, 3, 27, 13, 53, 35),
            Sequence = 0,
            Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
            Priority = 5,
            Location = "here",
            Summary = "test",
            DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
            DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00)
            //not yet testing property below as serialized output currently does not comply with RTFC 2445
            //Transparency = TransparencyType.Opaque,
            //Status = EventStatus.Confirmed
        };
        cal.Events.Add(evt);

        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(cal);

        Assert.Multiple(() =>
        {
            Assert.That(serializedCalendar.StartsWith("BEGIN:VCALENDAR"), Is.True);
            Assert.That(serializedCalendar.EndsWith("END:VCALENDAR" + SerializationConstants.LineBreak), Is.True);
        });

        var expectProperties = new[] { "METHOD:PUBLISH", "VERSION:2.0" };

        foreach (var p in expectProperties)
        {
            Assert.That(serializedCalendar, Does.Contain(SerializationConstants.LineBreak + p + SerializationConstants.LineBreak), "expected '" + p + "' not found");
        }

        InspectSerializedSection(serializedCalendar, "VEVENT",
            new[]
            {
                "CLASS:" + evt.Class, "CREATED:" + CalDateString(evt.Created), "DTSTAMP:" + CalDateString(evt.DtStamp),
                "LAST-MODIFIED:" + CalDateString(evt.LastModified), "SEQUENCE:" + evt.Sequence, "UID:" + evt.Uid, "PRIORITY:" + evt.Priority,
                "LOCATION:" + evt.Location, "SUMMARY:" + evt.Summary, "DTSTART:" + CalDateString(evt.DtStart), "DTEND:" + CalDateString(evt.DtEnd)
                //"TRANSPARENCY:" + TransparencyType.Opaque.ToString().ToUpperInvariant(),
                //"STATUS:" + EventStatus.Confirmed.ToString().ToUpperInvariant()
            });
    }

    private static readonly IList<Attendee> _attendees = new List<Attendee>
    {
        new Attendee("MAILTO:james@example.com")
        {
            CommonName = "James",
            Role = ParticipationRole.RequiredParticipant,
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Tentative
        },
        new Attendee("MAILTO:mary@example.com")
        {
            CommonName = "Mary",
            Role = ParticipationRole.RequiredParticipant,
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Accepted
        }
    }.AsReadOnly();

    [Test, Category("Serialization")]
    public void AttendeesSerialized()
    {
        var cal = new Calendar
        {
            Method = "REQUEST",
            Version = "2.0"
        };

        var evt = AttendeeTest.VEventFactory();
        cal.Events.Add(evt);
        // The casing of `MAILTO` is not in line with RFC 2368, but we should
        // be able to deal with it nevertheless and preserve it the way it is.
        const string org = "MAILTO:james@example.com";
        evt.Organizer = new Organizer(org);

        evt.Attendees.AddRange(_attendees);

        // Changing the ParticipationStatus just keeps the last status
        evt.Attendees[0].ParticipationStatus = EventParticipationStatus.Declined;

        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(cal);

        var vEvt = InspectSerializedSection(serializedCalendar, "VEVENT", new[] { "ORGANIZER:" + org });

        foreach (var a in evt.Attendees)
        {
            var vals = GetValues(vEvt, "ATTENDEE", a.Value.OriginalString.ToString());
            foreach (var v in new Dictionary<string, string>
            {
                ["CN"] = a.CommonName,
                ["ROLE"] = a.Role,
                ["RSVP"] = a.Rsvp.ToString()
                    .ToUpperInvariant(),
                ["PARTSTAT"] = a.ParticipationStatus
            })
            {
                Assert.Multiple(() =>
                {
                    Assert.That(vals.ContainsKey(v.Key), Is.True, $"could not find key '{v.Key}'");
                    Assert.That(vals[v.Key], Is.EqualTo(v.Value), $"ATTENDEE prop '{v.Key}' differ");
                });
            }
        }
    }

    [Test]
    public void ZeroDuration_Test()
    {
        var result = new DurationSerializer().SerializeToString(Duration.Zero);
        Assert.That("P0D".Equals(result, StringComparison.Ordinal), Is.True);
    }

    [Test]
    public void Duration_FromWeeks()
    {
        var weeks = Duration.FromWeeks(4).Weeks;
        Assert.That(weeks, Is.EqualTo(4));
    }

    [Test]
    public void DurationIsStable_Tests()
    {
        var e = GetSimpleEvent(); // DTSTART and DURATION are set
        var originalDuration = e.Duration;
        var c = new Calendar();
        c.Events.Add(e);
        var serialized = SerializeToString(c);
        Assert.Multiple(() =>
        {
            Assert.That(e.Duration, Is.EqualTo(originalDuration));
            Assert.That(serialized, Does.Contain("DURATION"));
        });
    }

    [Test]
    public void EventStatusAllCaps()
    {
        var e = GetSimpleEvent();
        e.Status = EventStatus.Confirmed;
        var serialized = SerializeToString(e);
        Assert.That(serialized.Contains(EventStatus.Confirmed, EventStatus.Comparison), Is.True);

        var calendar = DeserializeCalendar(serialized);
        var eventStatus = calendar.Events.First().Status;
        Assert.That(string.Equals(EventStatus.Confirmed, eventStatus, EventStatus.Comparison), Is.True);
    }

    [Test]
    public void ToDoStatusAllCaps()
    {
        var component = new Todo
        {
            Status = TodoStatus.NeedsAction
        };

        var c = new Calendar { Todos = { component } };
        var serialized = SerializeToString(c);
        Assert.That(serialized.Contains(TodoStatus.NeedsAction, TodoStatus.Comparison), Is.True);

        var calendar = DeserializeCalendar(serialized);
        var status = calendar.Todos.First().Status;
        Assert.That(string.Equals(TodoStatus.NeedsAction, status, TodoStatus.Comparison), Is.True);
    }

    [Test]
    public void JournalStatusAllCaps()
    {
        var component = new Journal
        {
            Status = JournalStatus.Final,
        };

        var c = new Calendar { Journals = { component } };
        var serialized = SerializeToString(c);
        Assert.That(serialized.Contains(JournalStatus.Final, JournalStatus.Comparison), Is.True);

        var calendar = DeserializeCalendar(serialized);
        var status = calendar.Journals.First().Status;
        Assert.That(string.Equals(JournalStatus.Final, status, JournalStatus.Comparison), Is.True);
    }

    [Test]
    public void UnicodeDescription()
    {
        const string ics =
           """
           BEGIN:VEVENT
           DTSTAMP:20171120T124856Z
           DTSTART;TZID=Europe/Helsinki:20160707T110000
           DTEND;TZID=Europe/Helsinki:20160707T140000
           SUMMARY:Some summary
           UID:20160627T123608Z-182847102@atlassian.net
           DESCRIPTION:Key points:\n�	Some text (text,
            , text\, text\, TP) some text\;\n�	some tex
            t Some text (Text\, Text)\;\n�	Some tex
            t some text\, some text\, text.\;\n\nsome te
            xt some tex�t some text. 
           ORGANIZER;X-CONFLUENCE-USER-KEY=ff801df01547101c6720006;CN=Some
            user;CUTYPE=INDIVIDUAL:mailto:some.mail@domain.com
           CREATED:20160627T123608Z
           LAST-MODIFIED:20160627T123608Z
           ATTENDEE;X-CONFLUENCE-USER-KEY=ff8080ef1df01547101c6720006;CN=Some
            text;CUTYPE=INDIVIDUAL:mailto:some.mail@domain.com
           SEQUENCE:1
           X-CONFLUENCE-SUBCALENDAR-TYPE:other
           TRANSP:TRANSPARENT
           STATUS:CONFIRMED
           END:VEVENT
           """;
        var deserializedEvent = Calendar.Load<CalendarEvent>(ics).Single();

        Assert.Multiple(() =>
        {
            Assert.That(deserializedEvent.Description, Does.Contain("\t"));
            Assert.That(deserializedEvent.Description, Does.Contain("�"));
            Assert.That(deserializedEvent.Description, Does.Contain("�"));
        });
    }

    [Test]
    public void TestStandardDaylightTimeZoneInfoDeserialization()
    {

        const string ics =
          """
           BEGIN:VTIMEZONE
           TZID:
           BEGIN:STANDARD
           DTSTART:16010101T030000
           TZOFFSETFROM:+0200
           TZOFFSETTO:+0100
           RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=10
           END:STANDARD
           BEGIN:DAYLIGHT
           DTSTART:16010101T020000
           TZOFFSETFROM:+0100
           TZOFFSETTO:+0200
           RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=3
           END:DAYLIGHT
           END:VTIMEZONE
           """;
        var timeZone = Calendar.Load<VTimeZone>(ics).Single();
        Assert.That(timeZone, Is.Not.Null, "Expected the TimeZone to be successfully deserialized");
        var timeZoneInfos = timeZone.TimeZoneInfos;
        Assert.Multiple(() =>
        {
            Assert.That(timeZoneInfos, Is.Not.Null, "Expected TimeZoneInfos to be deserialized");
            Assert.That(timeZoneInfos, Has.Count.EqualTo(2), "Expected 2 TimeZoneInfos");
            Assert.That(timeZoneInfos[0].Name, Is.EqualTo("STANDARD"));
            Assert.That(timeZoneInfos[0].OffsetFrom, Is.EqualTo(new UtcOffset("+0200")));
            Assert.That(timeZoneInfos[0].OffsetTo, Is.EqualTo(new UtcOffset("+0100")));
            Assert.That(timeZoneInfos[1].Name, Is.EqualTo("DAYLIGHT"));
            Assert.That(timeZoneInfos[1].OffsetFrom, Is.EqualTo(new UtcOffset("+0100")));
            Assert.That(timeZoneInfos[1].OffsetTo, Is.EqualTo(new UtcOffset("+0200")));
        });
    }

    [Test]
    public void TestRRuleUntilSerialization()
    {
        var rrule = new RecurrencePattern(FrequencyType.Daily)
        {
            Until = new CalDateTime(_nowTime.AddDays(7)),
        };
        const string someTz = "Europe/Volgograd";
        var e = new CalendarEvent
        {
            Start = _nowTime.ToTimeZone(someTz),
            End = _nowTime.AddHours(1).ToTimeZone(someTz),
            RecurrenceRules = new List<RecurrencePattern> { rrule },
        };
        var c = new Calendar
        {
            Events = { e },
        };
        var serialized = new CalendarSerializer().SerializeToString(c);
        var serializedUntilNotContainsZSuffix = serialized
            .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Single(line => line.StartsWith("RRULE:", StringComparison.Ordinal));
        var untilIndex = serializedUntilNotContainsZSuffix.IndexOf("UNTIL", StringComparison.Ordinal);
        var until = serializedUntilNotContainsZSuffix.Substring(untilIndex);

        Assert.That(!until.EndsWith("Z"), Is.True);
    }

    [Test]
    public void ProductId_and_Version_CanBeChanged()
    {
        var c = new Calendar
        {
            ProductId = "FOO",
            Version = "BAR",
        };

        var serialized = new CalendarSerializer().SerializeToString(c);
        
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain($"PRODID:{c.ProductId}"));
            Assert.That(serialized, Does.Contain($"VERSION:{c.Version}"));
        });
    }

    [Test]
    public void ProductId_and_Version_HaveDefaultValues()
    {
        var c = new Calendar();
        Assert.Multiple(() =>
        {
            Assert.That(c.ProductId, Is.EqualTo(LibraryMetadata.ProdId));
            Assert.That(c.Version, Is.EqualTo(LibraryMetadata.Version));
        });
    }

    [Test]
    public void AttachmentFormatType()
    {
        var cal1 = new Calendar
        {
            Events =
            {
                new CalendarEvent
                {
                    Attachments =
                    {
                        new Attachment(Encoding.UTF8.GetBytes("{}"))
                        {
                            FormatType = "application/json",
                        },
                    },
                },
            },
        };
        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(cal1);
        var cal2 = Calendar.Load(serializedCalendar);
        Assert.That(cal2.Events.Single().Attachments.Single().FormatType, Is.EqualTo("application/json"));
    }

    [Test(Description = "It should be possible to serialize a calendar component instead of a whole calendar")]
    public void SerializeSubcomponent()
    {
        const string expectedString = "This is an expected string";
        var e = new CalendarEvent
        {
            Start = new CalDateTime(_nowTime),
            End = new CalDateTime(_later),
            Summary = expectedString,
        };

        var serialized = new CalendarSerializer().SerializeToString(e);
        Assert.Multiple(() =>
        {
            Assert.That(serialized.Contains(expectedString, StringComparison.Ordinal), Is.True);
            Assert.That(!serialized.Contains("VCALENDAR", StringComparison.Ordinal), Is.True);
        });
    }
}
