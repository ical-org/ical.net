//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.IO;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class SerializationTests2
{
    [Test]
    public void CalendarWritesToStream()
    {
        var stream = new MemoryStream();
        var writer = new CalendarWriter(stream);

        writer.WriteName("LOCATION");
        writer.WriteValue("The Exceptionally Long Named Meeting Room Whose Name Wraps Over Several Lines When Exported From Leading Calendar and Office Software Application Microsoft Office 2007");
        writer.EndLine();

        writer.WriteName("X-ALT-DESC");
        writer.WriteValue("""<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 3.2//EN">\n<HTML>\n<HEAD>\n<META NAME="Generator" CONTENT="MS Exchange Server version 08.00.0681.000">\n<TITLE></TITLE>\n</HEAD>\n<BODY>\n<!-- Converted from text/rtf format -->\n\n<P DIR=LTR><SPAN LANG="en-gb"></SPAN></P>\n\n</BODY>\n</HTML>""");
        writer.EndLine();

        writer.WriteName("X-ABC-MULTIBYTE");
        writer.WriteValue("🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄");
        writer.EndLine();

        writer.WriteName("X-ABC-MULTIBYTE");
        writer.WriteValue("🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄");
        writer.EndLine();

        var reader = new StreamReader(stream);
        stream.Position = 0;
        var result = reader.ReadToEnd();

        var expected = """
            LOCATION:The Exceptionally Long Named Meeting Room Whose Name Wraps Over Se
             veral Lines When Exported From Leading Calendar and Office Software Applic
             ation Microsoft Office 2007
            X-ALT-DESC:<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 3.2//EN">\\n<HTML>\\n<HE
             AD>\\n<META NAME="Generator" CONTENT="MS Exchange Server version 08.00.068
             1.000">\\n<TITLE></TITLE>\\n</HEAD>\\n<BODY>\\n<!-- Converted from text/rt
             f format -->\\n\\n<P DIR=LTR><SPAN LANG="en-gb"></SPAN></P>\\n\\n</BODY>\\
             n</HTML>
            X-ABC-MULTIBYTE:🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍
             😄
            X-ABC-MULTIBYTE:🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍
             😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄

            """.ReplaceLineEndings("\r\n");

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void CalendarSerialize()
    {
        var cal = new Calendar();

        var e = new CalendarEvent
        {
            DtStamp = new(2026, 2, 4, 4, 0, 0, "UTC"),
            Start = new(2026, 2, 4, 4, 0, 0, "UTC")
        };

        var p = new CalendarProperty("X-ABC-ATTACHMENT", new Attachment
        {
            Uri = new System.Uri("https://www.example.com")
        });

        e.AddProperty(p);

        e.Uid = "uuid1153170430406";
        e.Properties["UID"]!.Parameters.Set("ENCODING", "BASE64");

        e.Url = new System.Uri("https://www.example.com");
        e.Properties["URL"]!.Parameters.Set("X-CUSTOM", "Property to shift encoding start");
        e.Properties["URL"]!.Parameters.Set("ENCODING", "BASE64");

        e.AddProperty("X-ABC-MULTIBYTE", "🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄\\🎈👍;😄");
        e.AddProperty("X-ABC-MULTIBYTES", "🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍😄\\🎈👍;😄");

        cal.Events.Add(e);

        //var t = new Serialization.CalendarSerializer();
        //var result = t.SerializeToString(cal);

        var options = new CalendarSerializerOptions();

        // Must define custom properties if value is not a string
        options.Converters.Add("X-ABC-ATTACHMENT", ConverterMap.GetConverter("ATTACH"));

        var result = CalendarSerializer.Serialize(cal, options);

        var calFromString = CalendarSerializer.Deserialize<Calendar>(result, options);

        var expectedIcs = """
            BEGIN:VCALENDAR
            PRODID:-//github.com/ical-org/ical.net//NONSGML ical.net 1.0.0.0//EN
            VERSION:2.0
            BEGIN:VEVENT
            DTSTAMP:20260204T040000Z
            DTSTART:20260204T040000Z
            SEQUENCE:0
            UID;ENCODING=BASE64:dXVpZDExNTMxNzA0MzA0MDY=
            URL;X-CUSTOM=Property to shift encoding start;ENCODING=BASE64:aHR0cHM6Ly93d
             3cuZXhhbXBsZS5jb20=
            X-ABC-ATTACHMENT:https://www.example.com
            X-ABC-MULTIBYTE:🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍
             😄🎈👍😄🎈👍😄🎈👍😄\\🎈👍\;😄
            X-ABC-MULTIBYTES:🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍
             😄🎈👍😄🎈👍😄🎈👍😄\\🎈👍\;😄
            END:VEVENT
            END:VCALENDAR

            """.ReplaceLineEndings("\r\n");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(expectedIcs));
            Assert.That(calFromString.Events[0]!.Uid, Is.EqualTo(e.Uid));
            Assert.That(calFromString.Events[0]!.Url, Is.EqualTo(e.Url));

            Assert.That(
                calFromString.Events[0]!.Properties.Get<string>("X-ABC-MULTIBYTE"),
                Is.EqualTo(e.Properties.Get<string>("X-ABC-MULTIBYTE")));
        }
    }

    [Test]
    public void DeserializesSingleEventCalendar()
    {
        var ics = IcsFiles.Attachment4;

        var options = new CalendarSerializerOptions
        {
            OrderComponentProperties = false
        };

        var cal = CalendarSerializer.Deserialize<Calendar>(ics, options);
        var output = CalendarSerializer.Serialize(cal, options);

        Assert.That(output, Is.EqualTo(ics));
    }

    [Test]
    public void Destar()
    {
        const string iCalString =
            """
            BEGIN:VCALENDAR
            PRODID:-//github.com/ic
             al-org/ical.net//NONSGML ical.net//EN
            VERSION:2.0
            BEGIN:VEVENT
            X-ABC-TEST:This\;is\;a\\value\wow
            DTSTAMP:20250608T164638Z
            DTSTART:20250301T000000
            X-ABC-MULTIBYTE:🎈👍😄🎈👍😄🎈👍😄🎈👍😄🎈👍
             😄
            RRULE:FREQ=DAILY;COUNT=1000
            SEQUENCE:0
            UID:ac22036c-73e6-4020-b54a-80e580462749
            END:VEVENT
            END:VCALENDAR
            """;

        var cal1 = Serialization.SimpleDeserializer.Default.Deserialize(new StringReader(iCalString))
            .First();

        var cal2 = CalendarSerializer.Deserialize<Calendar>(iCalString);

        Assert.That(cal1.Properties["UID"], Is.EqualTo(cal2.Properties["UID"]));
    }

    [Test]
    public void PropertyParametersAreQuotedWhenNeeded()
    {
        var ev = new CalendarEvent();

        var prop = new CalendarProperty("X-TEST", "123");

        prop.AddParameter("X-COLON", "Start:End");
        prop.AddParameter("X-COMMA", "Start,End");
        prop.AddParameter("X-SEMICOLON", "Start;End");

        ev.AddProperty(prop);

        var data = CalendarSerializer.Serialize(ev);
        var result = data
            .Split()
            .Where(x => x.StartsWith("X-TEST"))
            .First();

        var expected = """
            X-TEST;X-COLON="Start:End";X-COMMA="Start,End";X-SEMICOLON="Start;End":123
            """;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void DoubleEncodedBase64_AndBackslashEscapedText()
    {
        var ev = new CalendarEvent
        {
            DtStamp = new(2026, 9, 3, 0, 0, 0, "UTC"),
            Uid = "123"
        };

        var prop = new CalendarProperty("CATEGORIES", "A,Test,Value;👍👍🎈");
        prop.AddValue("A second value:123");
        prop.AddParameter("ENCODING", "BASE64");

        ev.AddProperty(prop);

        var data = CalendarSerializer.Serialize(ev);

        var expected = """
            BEGIN:VEVENT
            CATEGORIES;ENCODING=BASE64:QVwsVGVzdFwsVmFsdWVcO/CfkY3wn5GN8J+OiCxBIHNlY29u
             ZCB2YWx1ZToxMjM=
            DTSTAMP:20260903T000000Z
            SEQUENCE:0
            UID:123
            END:VEVENT

            """.ReplaceLineEndings("\r\n");

        Assert.That(data, Is.EqualTo(expected));
    }

    [Test]
    public void CustomPropertiesWriteMultipleValues()
    {
        var ev = new CalendarEvent();

        var prop = new CalendarProperty("X-TEST", "A,Test,Value;👍👍🎈");
        prop.AddValue("A second value:123");

        ev.AddProperty(prop);

        var data2 = CalendarSerializer.Serialize(ev);

        Assert.That(data2, Does.Contain("A second value:123"));
    }

    [Test]
    public void CustomPropertyWithIntegerType()
    {
        var ev = new CalendarEvent();

        var prop = new CalendarProperty("X-TEST", 123);
        prop.AddParameter("VALUE", "INTEGER");

        ev.AddProperty(prop);

        //var t = new Serialization.EventSerializer();
        //var data = t.SerializeToString(ev)!;
        var data = CalendarSerializer.Serialize(ev);
        var result = data
            .Split()
            .Where(x => x.StartsWith("X-TEST"))
            .First();

        var expected = """
            X-TEST;VALUE=INTEGER:123
            """;

        //var ev2 = (CalendarEvent) Serialization.SimpleDeserializer.Default.Deserialize(new StringReader(data)).First();
        var ev2 = CalendarSerializer.Deserialize<CalendarEvent>(data);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(expected));

            // Value type is string, so it fails to get 123
            Assert.That(ev2.Properties.Get<int>("X-TEST"), Is.Zero);

            // Value as string works
            Assert.That(ev2.Properties.Get<string>("X-TEST"), Is.EqualTo("123"));
        }
    }

    [Test]
    public void CustomPropertyWithBooleanType()
    {
        var ev = new CalendarEvent();

        var prop = new CalendarProperty("X-TEST", false);
        prop.AddParameter("VALUE", "BOOLEAN");

        ev.AddProperty(prop);

        //var t = new Serialization.EventSerializer();
        //var data = t.SerializeToString(ev)!;
        var data = CalendarSerializer.Serialize(ev);
        var result = data
            .Split()
            .Where(x => x.StartsWith("X-TEST"))
            .First();

        var expected = """
            X-TEST;VALUE=BOOLEAN:FALSE
            """;

        Assert.That(result, Is.EqualTo(expected).IgnoreCase);
    }

    [Test]
    public void EventShouldNotSerializeBothDurationAndEnd()
    {
        var ev = new CalendarEvent();

        ev.Start = new(2026, 1, 1, 5, 0, 0);
        ev.Properties.Set("DURATION", Duration.FromHours(1));
        ev.Properties.Set("DTEND", new CalDateTime(2026, 1, 1, 8, 0, 0));

        //var t = new Serialization.EventSerializer();
        //var data = t.SerializeToString(ev)!;
        var data = CalendarSerializer.Serialize(ev);

        Assert.That(data.Contains("DURATION:"), Is.False);
    }

    [Test]
    public void FreeBusyDeserializes()
    {
        // RFC 5545 - Section 3.6.4
        var ics = """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:-//RDU Software//NONSGML HandCal//EN
            BEGIN:VFREEBUSY
            ORGANIZER:mailto:jsmith@example.com
            DTSTART:19980313T141711Z
            DTEND:19980410T141711Z
            FREEBUSY:19980314T233000Z/19980315T003000Z
            FREEBUSY;FBTYPE=FREE:19980316T153000Z/19980316T163000Z
            FREEBUSY:19980318T030000Z/19980318T040000Z
            URL:http://www.example.com/calendar/busytime/jsmith.ifb
            END:VFREEBUSY
            END:VCALENDAR
            """;

        var cal = CalendarSerializer.Deserialize<Calendar>(ics);

        var fb = cal.FreeBusy[0]!;

        Assert.That(fb.Entries, Has.Count.EqualTo(3));
        Assert.That(fb.Entries[0].Status, Is.EqualTo(FreeBusyStatus.Busy));
        Assert.That(fb.Entries[1].Status, Is.EqualTo(FreeBusyStatus.Free));
    }

    [Test]
    public void StatusConfirmedEventDeserializes()
    {
        // RFC 5545 - Section 4
        var ics = """
            BEGIN:VCALENDAR
            PRODID:-//xyz Corp//NONSGML PDA Calendar Version 1.0//EN
            VERSION:2.0
            BEGIN:VEVENT
            DTSTAMP:19960704T120000Z
            UID:uid1@example.com
            ORGANIZER:mailto:jsmith@example.com
            DTSTART:19960918T143000Z
            DTEND:19960920T220000Z
            STATUS:CONFIRMED
            CATEGORIES:CONFERENCE
            SUMMARY:Networld+Interop Conference
            DESCRIPTION:Networld+Interop Conference
              and Exhibit\nAtlanta World Congress Center\n
             Atlanta\, Georgia
            END:VEVENT
            END:VCALENDAR
            """;

        var cal = CalendarSerializer.Deserialize<Calendar>(ics);

        Assert.That(cal.Events[0]!.Status, Is.EqualTo(EventStatus.Confirmed));
    }

    [Test]
    public void TransparencyTypeDeserializes()
    {
        // RFC 5545 - Section 3.6.1
        var ics = """
            BEGIN:VEVENT
            UID:19970901T130000Z-123402@example.com
            DTSTAMP:19970901T130000Z
            DTSTART:19970401T163000Z
            DTEND:19970402T010000Z
            SUMMARY:Laurel is in sensitivity awareness class.
            CLASS:PUBLIC
            CATEGORIES:BUSINESS,HUMAN RESOURCES
            TRANSP:TRANSPARENT
            END:VEVENT
            """;

        var ev = CalendarSerializer.Deserialize<CalendarEvent>(ics);

        Assert.That(ev.Transparency, Is.EqualTo(TransparencyType.Transparent));
    }

    [Test]
    public void CustomPropertyWithDefinedType()
    {
        var ics = """
            BEGIN:VEVENT
            UID:19970901T130000Z-123402@example.com
            DTSTAMP:19970901T130000Z
            DTSTART:19970401T163000Z
            DTEND:19970402T010000Z
            SUMMARY:Laurel is in sensitivity awareness class.
            X-ABC-CUSTOM;VALUE=DATE-TIME:20260901T163000Z
            END:VEVENT
            """;

        var config = new CalendarSerializerOptions();

        // Use the same converter that DTSTART uses
        config.Converters.Add("X-ABC-CUSTOM", config.GetConverter("DTSTART"));

        var ev = CalendarSerializer.Deserialize<CalendarEvent>(ics, config);

        Assert.That(ev.Properties.Get<CalDateTime>("X-ABC-CUSTOM"), Is.EqualTo(CalDateTime.Parse("20260901T163000Z", null)));
    }

    [Test]
    public void CustomBooleanTypeSerialize()
    {
        var ics = """
            BEGIN:VEVENT
            UID:19970901T130000Z-123402@example.com
            DTSTAMP:19970901T130000Z
            DTSTART:19970401T163000Z
            DTEND:19970402T010000Z
            SUMMARY:Laurel is in sensitivity awareness class.
            X-ABC-CUSTOM;VALUE=BOOLEAN:TRUE
            END:VEVENT
            """;

        var config = new CalendarSerializerOptions();
        config.Converters.Add("X-ABC-CUSTOM", new FakeCustomConverter());

        var ev = CalendarSerializer.Deserialize<CalendarEvent>(ics, config);

        Assert.That(ev.Properties.Get<bool>("X-ABC-CUSTOM"), Is.True);
    }

    private class FakeCustomConverter : CalendarPropertyConverter<bool>
    {
        public override bool Read(CalendarReader reader, IParameterCollection parameters)
        {
            if (reader.TryGetBoolean(out var result))
            {
                return result;
            }

            throw new System.Exception("Custom user exception");
        }

        public override void Write(CalendarWriter writer, bool value)
        {
            writer.WriteValue(value ? "TRUE" : "FALSE");
        }
    }

    [Test]
    public void CustomComponentDeserialize()
    {
        var ics = """
            BEGIN:ABC
            SUMMARY:This is a custom component
            END:ABC
            """;

        var abc = CalendarSerializer.Deserialize<CalendarComponent>(ics);

        Assert.That(abc.Properties.Get<string>("SUMMARY"), Is.EqualTo("This is a custom component"));
    }
}
