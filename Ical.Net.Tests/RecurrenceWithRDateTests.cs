//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.Collections;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class RecurrenceWithRDateTests
{
    [Test]
    public void RDate_SingleDateTime_IsProcessedCorrectly()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1),
        };
        calendarEvent.RecurrenceDates.Add(new CalDateTime(2023, 10, 2, 10, 0, 0));

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(2));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0)));
            Assert.That(ics, Does.Contain("RDATE:20231002T100000"));
            Assert.That(ics, Does.Contain("DURATION:PT1H"));
        });
    }

    [Test]
    public void RDate_SingleDateOnly_IsProcessedCorrectly()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);

        List<Period> recurrenceDates =
        [
            new Period(new CalDateTime(2023, 10, 2))
        ];

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
        };
        calendarEvent.RecurrenceDates.AddRange(recurrenceDates);

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(2));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2)));
            Assert.That(ics, Does.Contain("RDATE;VALUE=DATE:20231002"));
            Assert.That(ics, Does.Not.Contain("DURATION:"));
        });
    }

    [Test]
    public void RDate_MultipleDates_WithTimeZones_AreProcessedCorrectly()
    {
        const string tzId = "America/New_York";
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0, tzId);
        var recurrenceDates = new List<CalDateTime>
        {
            new CalDateTime(2023, 10, 2, 10, 0, 0, tzId),
            new CalDateTime(2023, 10, 3, 10, 0, 0, tzId)
        };

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 2),
        };
        calendarEvent.RecurrenceDates.AddRange(recurrenceDates);

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
            Assert.That(ics, Does.Contain("RDATE;TZID=America/New_York:20231002T100000,20231003T100000"));
            Assert.That(ics, Does.Contain("DURATION:PT2H"));
        });
    }

    [Test]
    public void RDate_PeriodsWithTimezone_AreProcessedCorrectly()
    {
        const string tzId = "America/New_York";
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0, tzId);
        var recDateCollection = new List<Period>
        {
            new Period(new CalDateTime(2023, 10, 2, 10, 0, 0, tzId), new Duration(hours: 4)),
            new Period(new CalDateTime(2023, 10, 3, 10, 0, 0, tzId), new Duration(hours: 5))
        };

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 2),
        };
        calendarEvent.RecurrenceDates.AddRange(recDateCollection);

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
            // Line folding is used for long lines
            Assert.That(ics, Does.Contain("RDATE;TZID=America/New_York;VALUE=PERIOD:20231002T100000/PT4H,20231003T1000\r\n 00/PT5H"));
        });

        // Deserialization

        cal = Calendar.Load(ics)!;
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

    [Test]
    public void RDate_MixedDatesAndPeriods_AreProcessedCorrectly()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1),
        };
        calendarEvent.RecurrenceDates.Add(new CalDateTime(2023, 10, 2, 10, 0, 0))
            .Add(new Period(new CalDateTime(2023, 10, 3, 10, 0, 0), new Duration(hours: 3)));

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(3));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0)));
            Assert.That(occurrences[2].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 3, 10, 0, 0)));
            Assert.That(occurrences[2].Period.EffectiveDuration, Is.EqualTo(new Duration(hours: 3)));
            Assert.That(ics, Does.Contain("RDATE:20231002T100000"));
            Assert.That(ics, Does.Contain("RDATE;VALUE=PERIOD:20231003T100000/PT3H"));
        });
    }

    [Test]
    public void RDate_DifferentTimeZones_AreProcessedCorrectly()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0, "America/New_York");
        List<CalDateTime> recDates =
        [
            new CalDateTime(2023, 10, 2, 10, 0, 0, "America/Los_Angeles"),
            new CalDateTime(2023, 10, 3, 10, 0, 0, "Europe/London")
        ];
        
        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1),
        };
        calendarEvent.RecurrenceDates.AddRange(recDates);

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(3));
            Assert.That(occurrences[0].Period.StartTime,
                Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0, "America/New_York")));
            Assert.That(occurrences[1].Period.StartTime,
                Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0, "America/Los_Angeles")));
            Assert.That(occurrences[2].Period.StartTime,
                Is.EqualTo(new CalDateTime(2023, 10, 3, 10, 0, 0, "Europe/London")));
            Assert.That(ics, Does.Contain("RDATE;TZID=America/Los_Angeles:20231002T100000"));
            Assert.That(ics, Does.Contain("RDATE;TZID=Europe/London:20231003T100000"));
        });
    }

    [Test]
    public void RDate_DateOnlyWithDurationAndDateTime_AreProcessedCorrectly()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(days: 2),
        };
        calendarEvent.RecurrenceDates.Add(new Period(new CalDateTime(2023, 10, 2), new Duration(days: 1)))
            .Add(new CalDateTime(2023, 10, 3, 10, 0, 0));

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(3));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(eventStart));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2)));
            Assert.That(occurrences[1].Period.EffectiveDuration, Is.EqualTo(new Duration(days: 1)));
            Assert.That(occurrences[2].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 3, 10, 0, 0)));
            Assert.That(ics, Does.Contain("RDATE;VALUE=PERIOD:20231002/P1D"));
            Assert.That(ics, Does.Contain("RDATE:20231003T100000"));
            Assert.That(ics, Does.Contain("DURATION:P2D"));
        });
    }

    [Test]
    public void RDate_OverlappingPeriods_AreProcessedCorrectly()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);
        List<Period> recDates =
        [
            new Period(new CalDateTime(2023, 10, 2, 10, 0, 0), new Duration(hours: 2)),
            new Period(new CalDateTime(2023, 10, 2, 11, 0, 0), new Duration(hours: 2))
        ];

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1),
        };
        calendarEvent.RecurrenceDates.AddRange(recDates);

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(3));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 1, 10, 0, 0)));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 10, 0, 0)));
            Assert.That(occurrences[2].Period.StartTime, Is.EqualTo(new CalDateTime(2023, 10, 2, 11, 0, 0)));
            Assert.That(ics, Does.Contain("RDATE;VALUE=PERIOD:20231002T100000/PT2H,20231002T110000/PT2H"));
        });
    }

    [Test]
    public void RDate_LargeNumberOfDates_ShouldBeLineFolded()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);
        var recurrenceDates = new PeriodList();
        for (var i = 1; i <= 100; i++) // Adjusted to create 100 dates
        {
            recurrenceDates.Add(new Period(eventStart.AddDays(i)));
        }

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1),
        };
        calendarEvent.RecurrenceDates.AddRange(recurrenceDates);

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(101)); // Including the original event
            for (var i = 0; i < 101; i++)
            {
                Assert.That(occurrences[i].Period.StartTime, Is.EqualTo(eventStart.AddDays(i)));
            }
            // First folded line is 75 characters long
            Assert.That(ics, Does.Contain("RDATE:20231002T100000,20231003T100000,20231004T100000,20231005T100000,2023"));
            // Last folded line
            Assert.That(ics, Does.Contain(" 00,20240107T100000,20240108T100000,20240109T100000"));
        });
    }

    [Test]
    public void RDate_DuplicateDates_ShouldBeSerializedJustOnce()
    {
        var cal = new Calendar();
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0);

        var periodDuplicate = new Period(new CalDateTime(2023, 10, 2, 10, 0, 0), new Duration(hours: 2));
        List<Period> recDates = [periodDuplicate, periodDuplicate];

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1),
        };
        calendarEvent.RecurrenceDates.AddRange(recDates);

        cal.Events.Add(calendarEvent);
        var serializer = new CalendarSerializer();
        var ics = serializer.SerializeToString(cal);

        var occurrences = calendarEvent.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occurrences, Has.Count.EqualTo(2));
            Assert.That(occurrences[0].Period.StartTime, Is.EqualTo(eventStart));
            Assert.That(occurrences[1].Period.StartTime, Is.EqualTo(periodDuplicate.StartTime));

            Assert.That(ics, Does.Contain("RDATE;VALUE=PERIOD:20231002T100000/PT2H"));
        });
    }

    [Test]
    public void RDate_DateOnly_WithExactDuration_ShouldThrow()
    {
        var eventStart = new CalDateTime(2023, 10, 1, 10, 0, 0, "America/New_York");
        var recurrenceDates = new List<Period>
        {
            new Period(new CalDateTime(2023, 10, 2)),
        };

        var calendarEvent = new CalendarEvent
        {
            Start = eventStart,
            Duration = new Duration(hours: 1), // Exact duration cannot be added to date-only recurrence
        };
        calendarEvent.RecurrenceDates.AddRange(recurrenceDates);

        Assert.That(() => { _ = calendarEvent.GetOccurrences().ToList(); }, Throws.InvalidOperationException);
    }
}
