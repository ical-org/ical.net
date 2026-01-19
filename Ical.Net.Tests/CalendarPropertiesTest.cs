//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class CalendarPropertiesTest
{
    [Test]
    public void AddPropertyShouldNotIncludePropertyNameInValue()
    {
        const string propName = "X-WR-CALNAME";
        const string propValue = "Testname";

        var iCal = new Calendar();
        iCal.AddProperty(propName, propValue);

        var result = new CalendarSerializer().SerializeToString(iCal)!;

        var lines = result.Split(new[] { SerializationConstants.LineBreak }, StringSplitOptions.None);
        var propLine = lines.FirstOrDefault(x => x.StartsWith("X-WR-CALNAME:"));
        Assert.That(propLine, Is.EqualTo($"{propName}:{propValue}"));
    }

    [Test]
    public void PropertySerialization()
    {
        const string propValue =
            """<html><body>BodyText</body></html>""";
        var start = DateTime.UtcNow;
        var end = start.AddHours(1);
        var @event = new CalendarEvent
        {
            Start = new CalDateTime(start),
            End = new CalDateTime(end),
            Description = "This is a description",
        };
        var property = new CalendarProperty("X-ALT-DESC", propValue);
        // Parameters most not be part of the value
        property.Parameters.Add("FMTTYPE", "text/html");
        var property2 = new CalendarProperty("ATTENDEE", "mailto:janedoe@example.com");
        property2.Parameters.Add("MEMBER", "mailto:projectA@example.com,mailto:projectB@example.com");
        @event.Properties.Add(property2);
        @event.AddProperty(property);
        var calendar = new Calendar();
        calendar.Events.Add(@event);

        var serialized = new CalendarSerializer().SerializeToString(calendar);
        Assert.That(serialized, Does.Contain($"X-ALT-DESC;FMTTYPE=text/html:{propValue}"),
            "Parameter and value should get serialized standard compliant");
    }

    [Test]
    public void PropertyDeserialization()
    {
        var ics = """
                  BEGIN:VCALENDAR
                  PRODID:-//github.com/ical-org/ical.net//NONSGML ical.net 5.0.0.0//EN
                  VERSION:2.0
                  BEGIN:VEVENT
                  DESCRIPTION:This is a description
                  DTEND:20250518T145922Z
                  DTSTAMP:20250518T135922Z
                  DTSTART:20250518T135922Z
                  SEQUENCE:0
                  UID:8f7aa9fc-e9d7-4276-8bf6-915dfe168f0d
                  X-ALT-DESC;FMTTYPE=text/html:<html><body>BodyText</body></html>
                  X-PROJECTS;PROP=name;PRIO=high:ProjectA,ProjectB
                  END:VEVENT
                  END:VCALENDAR
                  """;
        var calendar = Calendar.Load(ics)!;
        var calEvent = calendar.Events.First();
        var propDescription = calEvent.Properties.FirstOrDefault(x => x.Name == "X-ALT-DESC")!;
        var propProjects = calEvent.Properties.FirstOrDefault(x => x.Name == "X-PROJECTS")!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(propDescription.Parameters, Has.Count.EqualTo(1));
            Assert.That(propDescription.Value, Is.EqualTo("<html><body>BodyText</body></html>"));
            Assert.That(propDescription.Parameters.FirstOrDefault(p => p.Name == "FMTTYPE")!.Value, Is.EqualTo("text/html"));

            Assert.That(propProjects.Parameters, Has.Count.EqualTo(2), "Parameter list should have 2 elements");
            Assert.That(propProjects.Value, Is.EqualTo("ProjectA,ProjectB"));
            Assert.That(propProjects.Parameters.FirstOrDefault(p => p.Name == "PROP")!.Value!.ToString(), Is.EqualTo("name"));
            Assert.That(propProjects.Parameters.FirstOrDefault(p => p.Name == "PRIO")!.Value!.ToString(), Is.EqualTo("high"));
        }
    }

    [Test]
    public void PropertySetValueMustAllowNull()
    {
        var property = new CalendarProperty();
        Assert.DoesNotThrow(() => property.SetValue(null));
    }
}
