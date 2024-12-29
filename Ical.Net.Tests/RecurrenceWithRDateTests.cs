//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class RecurrenceWithRDateTests
{
    [Test]
    public void RDate_SingleDate_IsProcessedCorrectly()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);
        PeriodList recurrenceDates =
        [
            new Period(new CalDateTime(2023, 10, 2, 10, 0, 0))
        ];

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1),
            RecurrenceDates = new List<PeriodList> { recurrenceDates }
        };

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(2));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0)));
            Assert.That(ics, Does.Contain("DURATION:PT1H"));
            Assert.That(ics, Does.Contain("RDATE:20231002T100000"));
        });
    }

    [Test]
    public void RDate_MultipleDates_WithTimeZones_AreProcessedCorrectly()
    {
        const string tzId = "America/New_York";
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0, tzId);
        var recurrenceDates = new PeriodList
        {
            new Period(new CalDateTime(2023, 10, 2, 10, 0, 0, tzId)),
            new Period(new CalDateTime(2023, 10, 3, 10, 0, 0, tzId))
        };

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 2),
            RecurrenceDates = new List<PeriodList> { recurrenceDates }
        };

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(3));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0, tzId)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0, tzId)));
            Assert.That(occurrences[2].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 3, 10, 0, 0, tzId)));
            Assert.That(ics, Does.Contain("DURATION:PT2H"));
            Assert.That(ics, Does.Contain("RDATE;TZID=America/New_York:20231002T100000,20231003T100000"));
        });
    }

    [Test]
    public void RDate_Periods_AreProcessedCorrectly()
    {
        const string tzId = "America/New_York";
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0, tzId);
        var recurrenceDates = new PeriodList
        {
            new Period(new CalDateTime(2023, 10, 2, 10, 0, 0, tzId), new Duration(hours: 4)),
            new Period(new CalDateTime(2023, 10, 3, 10, 0, 0, tzId), new Duration(hours: 5))
        };

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 2),
            RecurrenceDates = new List<PeriodList> { recurrenceDates }
        };
        
        cal.Events.Add(calendarEvent);

        // Serialization

        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(3));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0, tzId)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0, tzId)));
            Assert.That(occurrences[2].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 3, 10, 0, 0, tzId)));
            Assert.That(occurrences[1].Period.EffectiveDuration, Is.EqualTo(new Duration(hours: 4)));
            Assert.That(occurrences[2].Period.EffectiveDuration, Is.EqualTo(new Duration(hours: 5)));
            Assert.That(ics, Does.Contain("RDATE;TZID=America/New_York:20231002T100000/PT4H,20231003T100000/PT5H"));
        });

        // Deserialization

        cal = Calendar.Load(ics);
        occurrences = cal.Events.First().GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(3));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0, tzId)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0, tzId)));
            Assert.That(occurrences[2].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 3, 10, 0, 0, tzId)));
            Assert.That(occurrences[1].Period.EffectiveDuration, Is.EqualTo(new Duration(hours: 4)));
            Assert.That(occurrences[2].Period.EffectiveDuration, Is.EqualTo(new Duration(hours: 5)));
        });
    }
}
