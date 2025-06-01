//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class CalendarEventTest
{
    private static readonly DateTime _now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    private static readonly DateTime _later = _now.AddHours(1);
    private static readonly string _uid = Guid.NewGuid().ToString();

    /// <summary>
    /// Ensures that events can be properly added to a calendar.
    /// </summary>
    [Test, Category("CalendarEvent")]
    public void Add1()
    {
        var cal = new Calendar();

        var evt = new CalendarEvent
        {
            Summary = "Testing",
            Start = new CalDateTime(2010, 3, 25),
            End = new CalDateTime(2010, 3, 26)
        };

        cal.Events.Add(evt);
        Assert.That(cal.Children, Has.Count.EqualTo(1));
        Assert.That(cal.Children[0], Is.SameAs(evt));
    }

    /// <summary>
    /// Ensures that events can be properly removed from a calendar.
    /// </summary>
    [Test, Category("CalendarEvent")]
    public void Remove1()
    {
        var cal = new Calendar();

        var evt = new CalendarEvent
        {
            Summary = "Testing",
            Start = new CalDateTime(2010, 3, 25),
            End = new CalDateTime(2010, 3, 26)
        };

        cal.Events.Add(evt);
        Assert.Multiple(() =>
        {
            Assert.That(cal.Children, Has.Count.EqualTo(1));
            Assert.That(cal.Children[0], Is.SameAs(evt));
        });
        cal.RemoveChild(evt);
        Assert.Multiple(() =>
        {
            Assert.That(cal.Children.Count, Is.EqualTo(0));
            Assert.That(cal.Events.Count, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Ensures that events can be properly removed from a calendar.
    /// </summary>
    [Test, Category("CalendarEvent")]
    public void Remove2()
    {
        var cal = new Calendar();

        var evt = new CalendarEvent
        {
            Summary = "Testing",
            Start = new CalDateTime(2010, 3, 25),
            End = new CalDateTime(2010, 3, 26)
        };

        cal.Events.Add(evt);
        Assert.Multiple(() =>
        {
            Assert.That(cal.Children, Has.Count.EqualTo(1));
            Assert.That(cal.Children[0], Is.SameAs(evt));
        });

        cal.Events.Remove(evt);
        Assert.Multiple(() =>
        {
            Assert.That(cal.Children.Count, Is.EqualTo(0));
            Assert.That(cal.Events.Count, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Ensures that event DTSTAMP is set.
    /// </summary>
    [Test, Category("CalendarEvent")]
    public void EnsureDTSTAMPisNotNull()
    {
        var cal = new Calendar();

        // Do not set DTSTAMP manually
        var evt = new CalendarEvent
        {
            Summary = "Testing",
            Start = new CalDateTime(2010, 3, 25),
            End = new CalDateTime(2010, 3, 26)
        };

        cal.Events.Add(evt);
        Assert.That(evt.DtStamp, Is.Not.Null);
    }

    /// <summary>
    /// Ensures that automatically set DTSTAMP property is of kind UTC.
    /// </summary>
    [Test, Category("CalendarEvent")]
    public void EnsureDTSTAMPisOfTypeUTC()
    {
        var cal = new Calendar();

        var evt = new CalendarEvent
        {
            Summary = "Testing",
            Start = new CalDateTime(2010, 3, 25),
            End = new CalDateTime(2010, 3, 26)
        };

        cal.Events.Add(evt);
        Assert.That(evt.DtStamp.IsUtc, Is.True, "DTSTAMP should always be of type UTC.");
    }

    /// <summary>
    /// Ensures that automatically set DTSTAMP property is being serialized with kind UTC.
    /// </summary>
    [Test, Category("Deserialization"), TestCaseSource(nameof(EnsureAutomaticallySetDtStampIsSerializedAsUtcKind_TestCases))]
    public bool EnsureAutomaticallySetDTSTAMPisSerializedAsKindUTC(string serialized)
    {
        var lines = serialized.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        var result = lines.First(s => s.StartsWith("DTSTAMP"));

        return !result.Contains("TZID=") && result.EndsWith("Z");
    }

    public static IEnumerable EnsureAutomaticallySetDtStampIsSerializedAsUtcKind_TestCases()
    {
        var emptyCalendar = new Calendar();
        var evt = new CalendarEvent();
        emptyCalendar.Events.Add(evt);

        var serializer = new CalendarSerializer();
        yield return new TestCaseData(serializer.SerializeToString(emptyCalendar))
            .SetName("Empty calendar with empty event returns true")
            .Returns(true);

        var explicitDtStampCalendar = new Calendar();
        var explicitDtStampEvent = new CalendarEvent
        {
            DtStamp = new CalDateTime(new DateTime(2016, 8, 17, 2, 30, 0, DateTimeKind.Utc))
        };
        explicitDtStampCalendar.Events.Add(explicitDtStampEvent);
        yield return new TestCaseData(serializer.SerializeToString(explicitDtStampCalendar))
            .SetName("CalendarEvent with explicitly-set DTSTAMP property returns true")
            .Returns(true);
    }

    private static CalendarEvent GetSimpleEvent() => new CalendarEvent
    {
        DtStart = new CalDateTime(_now),
        DtEnd = new CalDateTime(_later),
        Uid = _uid,
    };

    private static List<RecurrencePattern> GetSimpleRecurrenceList()
        => new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily, 1) { Count = 5 } };
    private static List<CalDateTime> GetExceptionDates()
        => new List<CalDateTime> { new CalDateTime(_now.AddDays(1).Date) };
    
    [Test]
    public void EventResourcesCanBeZeroedOut()
    {
        var e = GetSimpleEvent();
        var resources = new[] { "Foo", "Bar", "Baz" };

        e.Resources = new List<string>(resources);
        Assert.That(resources, Is.EquivalentTo(e.Resources));

        var newResources = new[] { "Hello", "Goodbye" };
        e.Resources = new List<string>(newResources);
        Assert.Multiple(() =>
        {
            Assert.That(newResources, Is.EquivalentTo(e.Resources));
            Assert.That(e.Resources.Any(r => resources.Contains(r)), Is.False);
        });

        //See https://github.com/rianjs/ical.net/issues/208
        e.Resources = Array.Empty<string>();
        Assert.That(e.Resources?.Count, Is.EqualTo(0));
    }

    [Test]
    public void HourMinuteSecondOffsetParsingTest()
    {
        const string ical =
            """
            BEGIN:VCALENDAR
            PRODID:-//1&1 Mail & Media GmbH/GMX Kalender Server 3.10.0//NONSGML//DE
            VERSION:2.0
            CALSCALE:GREGORIAN
            METHOD:REQUEST
            BEGIN:VTIMEZONE
            TZID:Europe/Brussels
            TZURL:http://tzurl.org/zoneinfo/Europe/Brussels
            X-LIC-LOCATION:Europe/Brussels
            BEGIN:DAYLIGHT
            TZOFFSETFROM:-001730
            TZOFFSETTO:-001730
            TZNAME:CEST
            DTSTART:19810329T020000
            RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=-1SU
            END:DAYLIGHT
            BEGIN:STANDARD
            TZOFFSETFROM:+001730
            TZOFFSETTO:+001730
            TZNAME:BMT
            DTSTART:18800101T000000
            RDATE:18800101T000000
            END:STANDARD
            END:VTIMEZONE
            END:VCALENDAR
            """;
        var timezones = Calendar.Load(ical)
            .TimeZones.First()
            .Children.Cast<CalendarComponent>()
            .ToArray();

        var positiveOffset = timezones
            .Skip(1).Take(1).First()
            .Properties.First().Value as UtcOffset;
        var expectedPositive = TimeSpan.FromMinutes(17.5);
        Assert.That(positiveOffset?.Offset, Is.EqualTo(expectedPositive));

        var negativeOffset = timezones
            .First()
            .Properties.First().Value as UtcOffset;

        var expectedNegative = TimeSpan.FromMinutes(-17.5);
        Assert.That(negativeOffset?.Offset, Is.EqualTo(expectedNegative));
    }


    [Test, Category("CalendarEvent")]
    public void GetEffectiveDurationTests()
    {
        var dt = new DateTime(2025, 3, 1, 14, 30, 0);
        const string tzIdStart = "America/New_York";
        const string tzIdEnd = "Europe/London";

        var evt = new CalendarEvent
        {
            DtStart = new CalDateTime(DateOnly.FromDateTime(dt), TimeOnly.FromDateTime(dt), tzIdStart),
            DtEnd = new CalDateTime(DateOnly.FromDateTime(dt.AddHours(1)), TimeOnly.FromDateTime(dt.AddHours(1)), tzIdEnd)
        };

        var ed = evt.EffectiveDuration;
        Assert.Multiple(() =>
        {
            Assert.That(evt.DtStart.Value, Is.EqualTo(dt));
            Assert.That(evt.DtEnd.Value, Is.EqualTo(dt.AddHours(1)));
            Assert.That(evt.EffectiveDuration, Is.EqualTo(Duration.FromHours(-4)));
        });

        evt = new CalendarEvent
        {
            DtStart = new CalDateTime(DateOnly.FromDateTime(dt.Date)),
            DtEnd = new CalDateTime(DateOnly.FromDateTime(dt.Date))
        };

        Assert.Multiple(() =>
        {
            Assert.That(evt.DtStart.Value, Is.EqualTo(dt.Date));
            Assert.That(evt.EffectiveDuration.IsZero, Is.True);
        });

        evt = new CalendarEvent
        {
            DtStart = new CalDateTime(DateOnly.FromDateTime(dt)),
        };

        Assert.Multiple(() =>
        {
            Assert.That(evt.DtStart.Value, Is.EqualTo(dt.Date));
            Assert.That(evt.Duration, Is.Null);
            Assert.That(evt.EffectiveDuration, Is.EqualTo(DataTypes.Duration.FromDays(1)));
        });

        evt = new CalendarEvent
        {
            DtStart = new CalDateTime(DateOnly.FromDateTime(dt), TimeOnly.FromDateTime(dt)),
            Duration = Duration.FromHours(2),
        };

        Assert.Multiple(() => {
            Assert.That(evt.DtStart.Value, Is.EqualTo(dt));
            Assert.That(evt.DtEnd, Is.Null);
            Assert.That(evt.EffectiveDuration, Is.EqualTo(Duration.FromHours(2)));
        });

        evt = new CalendarEvent()
        {
            DtStart = new CalDateTime(DateOnly.FromDateTime(dt.Date), TimeOnly.FromDateTime(dt.Date)),
            Duration = Duration.FromHours(2),
        };

        Assert.Multiple(() => {
            Assert.That(evt.DtStart.Value, Is.EqualTo(dt.Date));
            Assert.That(evt.EffectiveDuration, Is.EqualTo(Duration.FromHours(2)));
        });

        evt = new CalendarEvent()
        {
            DtStart = new CalDateTime(DateOnly.FromDateTime(dt)),
            Duration = Duration.FromDays(1),
        };

        Assert.Multiple(() => {
            Assert.That(evt.DtStart.Value, Is.EqualTo(dt.Date));
            Assert.That(evt.EffectiveDuration, Is.EqualTo(Duration.FromDays(1)));
        });
    }

    [Test]
    public void EitherEndTime_OrDuraction_CanBeSet()
    {
        var evt = new CalendarEvent()
        {
            DtStart = new CalDateTime(2025, 10, 11, 12, 13, 14, CalDateTime.UtcTzId)
        };

        Assert.Multiple(() =>
        {
            Assert.That(() => evt.DtEnd = new CalDateTime(2025, 12, 11), Throws.Nothing);
            Assert.That(() => evt.Duration = Duration.FromDays(1), Throws.InvalidOperationException);
            Assert.That(() => evt.DtEnd = null, Throws.Nothing);
            Assert.That(() => evt.Duration = Duration.FromDays(1), Throws.Nothing);
            Assert.That(() => evt.DtEnd = new CalDateTime(2025, 12, 11), Throws.InvalidOperationException);
        });
    }
}
