//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

/// <summary>
/// The class contains the tests for submitted issues from the GitHub repository,
/// slightly modified to fit the testing environment and the current version of the library.
/// </summary>
[TestFixture]
public class RecurrenceTests_From_Issues
{
    [Test]
    public void GetOccurrence_DtEnd_ShouldBeExcluded()
    {
        // VEVENT duration not handled correctly in case of a DST change between DTSTART and DTEND #660 

        // DST transition occurs on 2024-10-27, 02:00:00,
        // when the clocks go back one hour from BST to GMT.
        var startDate = new CalDateTime("20241001T000000", "Europe/London");
        var endDate = new CalDateTime("20241027T020000", "Europe/London");

        var ical =
            $"""
             BEGIN:VCALENDAR
             VERSION:2.0
             PRODID:Video Wall
             BEGIN:VEVENT
             UID:VW6
             DTSTAMP:20240630T000000Z
             DTSTART;TZID=Europe/London:{startDate}
             DTEND;TZID=Europe/London:{endDate}
             SUMMARY:New home speech.mp4
             COMMENT:New location announcement; may need update before Thanksgiving
             END:VEVENT
             END:VCALENDAR
             """;

        var calendar = Calendar.Load(ical);
        // Event ends on 2024-10-27, at 02:00:00 GMT (when DST ends). The end time is excluded by RFC 5545 definition.
        var occurrences = calendar.GetOccurrences(endDate, new CalDateTime("20250101T000000", "Europe/London")).ToList();

        Assert.That(occurrences, Is.Empty);
    }

    [Test]
    public void ClockGoingForwardTest()
    {
        // Daylight saving transition bugs #623

        // Arrange
        var tzId = "Europe/London";

        var myEvent = new CalendarEvent
        {
            Start = new CalDateTime(2025, 3, 30, 0, 0, 0, tzId),
            End = new CalDateTime(2025, 3, 30, 2, 0, 0, tzId)
            // Either the Duration OR the Ende time can be used to calculate the period.
        };
        
        var calendar = new Calendar();
        calendar.Events.Add(myEvent);

        // Act
        IDateTime start = new CalDateTime(2025, 3, 30, 0, 0, 0, tzId);
        IDateTime end = new CalDateTime(2025, 3, 31, 0, 0, 0, tzId);

        var occurrences = calendar.GetOccurrences<CalendarEvent>(start, end).ToList();
        var occurrence = occurrences.Single();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count(), Is.EqualTo(1));
            Assert.That(occurrence.Source, Is.SameAs(myEvent));
            Assert.That(occurrence.Period.StartTime, Is.EqualTo(myEvent.Start));
            Assert.That(occurrence.Period.EndTime, Is.EqualTo(myEvent.End));
        });
    }

    [Test]
    public void ClockGoingBackTest()
    {
        // Daylight saving transition bugs #623 

        // Arrange
        var tzId = "Europe/London";

        var myEvent = new CalendarEvent
        {
            Start = new CalDateTime(2024, 10, 27, 1, 0, 0, tzId),
            End = new CalDateTime(2024, 10, 27, 2, 0, 0, tzId)
        };

        var calendar = new Calendar();
        calendar.Events.Add(myEvent);

        // Act
        IDateTime start = new CalDateTime(2024, 10, 27, 0, 0, 0, tzId);
        IDateTime end = new CalDateTime(2024, 10, 28, 0, 0, 0, tzId);

        var occurrences = calendar.GetOccurrences<CalendarEvent>(start, end).ToList();
        var occurrence = occurrences.Single();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count(), Is.EqualTo(1));
            Assert.That(occurrence.Source, Is.SameAs(myEvent));
            Assert.That(occurrence.Period.StartTime, Is.EqualTo(myEvent.Start));
            Assert.That(occurrence.Period.EndTime, Is.EqualTo(myEvent.End));
        });
    }

    [Test]
    public void ClockGoingForwardAllDayTest()
    {
        // Daylight saving transition bugs #623 

        // Arrange
        var timeZoneId = "Europe/London";

        var myEvent = new CalendarEvent
        {
            Start = new CalDateTime(2025, 3, 30),
            End = new CalDateTime(2025, 3, 31)
        };

        Assert.That(myEvent.IsAllDay, Is.True);

        var calendar = new Calendar();
        calendar.Events.Add(myEvent);

        // Act
        IDateTime start = new CalDateTime(2025, 3, 30, 0, 0, 0, timeZoneId);
        IDateTime end = new CalDateTime(2025, 3, 31, 0, 0, 0, timeZoneId);

        var occurrences = calendar.GetOccurrences<CalendarEvent>(start, end).ToList();
        var occurrence = occurrences.Single();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count, Is.EqualTo(1));
            Assert.That(occurrence.Source, Is.SameAs(myEvent));
            Assert.That(occurrence.Period.StartTime.HasTime, Is.False);
            Assert.That(occurrence.Period.StartTime, Is.EqualTo(myEvent.Start));
            Assert.That(occurrence.Period.EndTime.HasTime, Is.False);
            Assert.That(occurrence.Period.EndTime, Is.EqualTo(myEvent.End));
        });
    }

    [Test]
    public void ClockGoingBackAllDayTest()
    {
        // Daylight saving transition bugs #623 

        // Arrange
        var timeZoneId = "GMT Standard Time";

        var myEvent = new CalendarEvent
        {
            Start = new CalDateTime(2024, 10, 27),
            End = new CalDateTime(2024, 10, 28)
        };

        Assert.That(myEvent.IsAllDay, Is.True);

        var calendar = new Calendar();
        calendar.Events.Add(myEvent);

        // Act
        IDateTime start = new CalDateTime(2024, 10, 27, 0, 0, 0, timeZoneId);
        IDateTime end = new CalDateTime(2024, 10, 28, 0, 0, 0, timeZoneId);
        // Duration can't be used at the same time as End.

        var occurrences = calendar.GetOccurrences<CalendarEvent>(start, end).ToList();
        var occurrence = occurrences.Single();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count, Is.EqualTo(1));
            Assert.That(occurrence.Source, Is.SameAs(myEvent));
            Assert.That(occurrence.Period.StartTime.HasTime, Is.False);
            Assert.That(occurrence.Period.StartTime, Is.EqualTo(myEvent.Start));
            Assert.That(occurrence.Period.EndTime.HasTime, Is.False);
            Assert.That(occurrence.Period.EndTime, Is.EqualTo(myEvent.End));
        });
    }

    [Test]
    public void ClockGoingBackAllDayNonLocalTest()
    {
        // Daylight saving transition bugs #623 

        // Arrange
        var timeZoneId = "Pacific Standard Time";

        var myEvent = new CalendarEvent
        {
            Start = new CalDateTime(2024, 10, 27),
            End = new CalDateTime(2024, 10, 28)
            // Duration can't be used at the same time as End.
        };

        var calendar = new Calendar();
        calendar.Events.Add(myEvent);

        // Act
        IDateTime start = new CalDateTime(2024, 10, 27, 0, 0, 0, timeZoneId);
        IDateTime end = new CalDateTime(2024, 10, 28, 0, 0, 0, timeZoneId);

        var occurrences = calendar.GetOccurrences<CalendarEvent>(start, end).ToList();
        var occurrence = occurrences.Single();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(myEvent.IsAllDay, Is.True);
            Assert.That(occurrences.Count, Is.EqualTo(1));
            Assert.That(occurrence.Source, Is.SameAs(myEvent));
            Assert.That(occurrence.Period.StartTime.HasTime, Is.False);
            Assert.That(occurrence.Period.StartTime, Is.EqualTo(myEvent.Start));
            Assert.That(occurrence.Period.EndTime.HasTime, Is.False);
            Assert.That(occurrence.Period.EndTime, Is.EqualTo(myEvent.End));
        });
    }

    [Test]
    public void ClockGoingForwardAllDayNonLocalTest()
    {
        // Daylight saving transition bugs #623 

        // Arrange
        var timeZoneId = "Pacific Standard Time";

        var myEvent = new CalendarEvent
        {
            Start = new CalDateTime(2025, 3, 30),
            End = new CalDateTime(2025, 3, 31)
            // Duration can't be used at the same time as End.
        };

        var calendar = new Calendar();
        calendar.Events.Add(myEvent);

        // Act
        IDateTime start = new CalDateTime(2025, 3, 30, 0, 0, 0, timeZoneId);
        IDateTime end = new CalDateTime(2025, 3, 31, 0, 0, 0, timeZoneId);

        var occurrences = calendar.GetOccurrences<CalendarEvent>(start, end).ToList();
        var occurrence = occurrences.Single();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count, Is.EqualTo(1));
            Assert.That(occurrence.Source, Is.SameAs(myEvent));
            Assert.That(myEvent.IsAllDay, Is.True);
            Assert.That(occurrence.Period.StartTime.HasTime, Is.False);
            Assert.That(occurrence.Period.StartTime, Is.EqualTo(myEvent.Start));
            Assert.That(occurrence.Period.EndTime.HasTime, Is.False);
            Assert.That(occurrence.Period.EndTime, Is.EqualTo(myEvent.End));
        });
    }

    [Test]
    public void CheckCalendarZone()
    {
        // GetOccurrences does not work properly when used with TimeZones #671 

        var zone = "America/Los_Angeles";
        var calendar = new Ical.Net.Calendar();
        var blockDate = new DateTime(2024, 12, 2, 8, 0, 0, DateTimeKind.Utc);
        calendar.Events.Add(new CalendarEvent
        {
            Summary = "Unavailable Before Work",
            DtStart = new CalDateTime(blockDate, zone), //.ToTimeZone(zone),
            DtEnd = new CalDateTime(blockDate.Add(new TimeSpan(8, 0, 0)), zone), //.ToTimeZone(zone),
            RecurrenceRules = new List<RecurrencePattern>
            {
                new RecurrencePattern
                {
                    Frequency = Ical.Net.FrequencyType.Weekly,
                    Interval = 1,
                    ByDay = new List<WeekDay>
                    {
                        new WeekDay(DayOfWeek.Monday)
                    }
                }
            }
        });

        var start = new DateTime(2024, 12, 9, 8, 5, 0, DateTimeKind.Utc);
        var end = new DateTime(2024, 12, 10, 7, 59, 59, DateTimeKind.Utc);
        var occurrence = calendar.GetOccurrences(start, end);

        Assert.That(occurrence.Count, Is.EqualTo(1));
    }

    [Test]
    public void Daylight_Savings_Changes_567()
    {
        //  GetOccurrences Creates event with invalid time due to Daylight Savings changes #567 

        var calStart = new CalDateTime(DateTimeOffset.Parse("2023-01-14T19:21:03.700Z").UtcDateTime, "UTC");
        var calFinish = new CalDateTime(DateTimeOffset.Parse("2023-03-14T18:21:03.700Z").UtcDateTime, "UTC");
        var tz = "Pacific Standard Time";
        var pattern = new RecurrencePattern(
            "FREQ=WEEKLY;BYDAY=SU,MO,TU,WE"); //Adjust the date to today so that the times remain constant
        var localTzStartAdjust = (DateTime.Now.Add(TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(35))));
        var localTzFinishAdjust = DateTime.Now.Add(TimeSpan.FromHours(17));
        var ev = new Ical.Net.CalendarComponents.CalendarEvent
        {
            Class = "PUBLIC",
            Start = new CalDateTime(localTzStartAdjust, tz),
            End = new CalDateTime(localTzFinishAdjust, tz),
            Sequence = 0,
            RecurrenceRules = new List<RecurrencePattern> { pattern }
        };
        var col = ev.GetOccurrences(calStart, calFinish);

        Assert.That(col, Is.Empty);
    }

    #region Bug - Invalid recurring event occurrences after New Year #342 

    private static void CheckDates(DateTime startDate, DateTime endDate, CalDateTime[] expectedDates)
    {
        var rule = new RecurrencePattern(FrequencyType.Weekly, 2)
        {
            ByDay = new List<WeekDay>
            {
                new WeekDay(DayOfWeek.Monday),
                new WeekDay(DayOfWeek.Tuesday),
                new WeekDay(DayOfWeek.Wednesday),
                new WeekDay(DayOfWeek.Thursday),
                new WeekDay(DayOfWeek.Friday),
                new WeekDay(DayOfWeek.Saturday),
                new WeekDay(DayOfWeek.Sunday),
            }
        };

        var calendarEvent = new CalendarEvent
        {
            DtStart = new CalDateTime(startDate),
            DtEnd = new CalDateTime(endDate),
            RecurrenceRules = new List<RecurrencePattern> { rule }
        };

        var occurrences = calendarEvent.GetOccurrences(startDate, endDate);
        var occurrencesDates = occurrences.Select(o => new CalDateTime(o.Period.StartTime.Date)).ToList();

        // Sort both collections to ensure they are in the same order
        occurrencesDates.Sort();
        var sortedExpectedDates = expectedDates.OrderBy(d => d).ToList();

        Assert.That(occurrencesDates, Is.EquivalentTo(sortedExpectedDates));
    }

    [Test]
    public void NewYear_EachOtherWeek()
    {
        var startDate = new DateTime(2017, 12, 30);
        var endDate = new DateTime(2018, 1, 29);
        var expectedDates = new[]
        {
                new CalDateTime(2017, 12, 30),
                new CalDateTime(2017, 12, 31),

                // 1-7 no events
                
                new CalDateTime(2018, 1, 8),
                new CalDateTime(2018, 1, 9),
                new CalDateTime(2018, 1, 10),
                new CalDateTime(2018, 1, 11),
                new CalDateTime(2018, 1, 12),
                new CalDateTime(2018, 1, 13),
                new CalDateTime(2018, 1, 14),

                // 15 - 21 no events

                new CalDateTime(2018, 1, 22),
                new CalDateTime(2018, 1, 23),
                new CalDateTime(2018, 1, 24),
                new CalDateTime(2018, 1, 25),
                new CalDateTime(2018, 1, 26),
                new CalDateTime(2018, 1, 27),
                new CalDateTime(2018, 1, 28),
            };

        //PASS
        CheckDates(startDate, endDate, expectedDates);
    }

    [Test]
    public void December_EachOtherWeek()
    {
        var startDate = new DateTime(2017, 12, 1);
        var endDate = new DateTime(2017, 12, 31);
        var expectedDates = new[]
        {
                new CalDateTime(2017, 12, 1),
                new CalDateTime(2017, 12, 2),
                new CalDateTime(2017, 12, 3),
                // 4-10 no events
                
                new CalDateTime(2017, 12, 11),
                new CalDateTime(2017, 12, 12),
                new CalDateTime(2017, 12, 13),
                new CalDateTime(2017, 12, 14),
                new CalDateTime(2017, 12, 15),
                new CalDateTime(2017, 12, 16),
                new CalDateTime(2017, 12, 17),

                // 18 - 24 no events

                new CalDateTime(2017, 12, 25),
                new CalDateTime(2017, 12, 26),
                new CalDateTime(2017, 12, 27),
                new CalDateTime(2017, 12, 28),
                new CalDateTime(2017, 12, 29),
                new CalDateTime(2017, 12, 30),
               // new CalDateTime(2017, 12, 31), 
            };

        //PASS
        CheckDates(startDate, endDate, expectedDates);
    }

    [Test]
    public void NovemberEnd_December_EachOtherWeek()
    {
        var startDate = new DateTime(2017, 11, 30);
        var endDate = new DateTime(2017, 12, 31);
        var expectedDates = new[]
        {
                new CalDateTime(2017, 11, 30),
                new CalDateTime(2017, 12, 1),
                new CalDateTime(2017, 12, 2),
                new CalDateTime(2017, 12, 3),
                // 4-10 no events
                
                new CalDateTime(2017, 12, 11),
                new CalDateTime(2017, 12, 12),
                new CalDateTime(2017, 12, 13),
                new CalDateTime(2017, 12, 14),
                new CalDateTime(2017, 12, 15),
                new CalDateTime(2017, 12, 16),
                new CalDateTime(2017, 12, 17),

                // 18 - 24 no events

                new CalDateTime(2017, 12, 25),
                new CalDateTime(2017, 12, 26),
                new CalDateTime(2017, 12, 27),
                new CalDateTime(2017, 12, 28),
                new CalDateTime(2017, 12, 29),
                new CalDateTime(2017, 12, 30),
            };

        // PASS
        CheckDates(startDate, endDate, expectedDates);
    }
    #endregion

    [Test]
    public void Except_Tuesday_Thursday_Saturday_Sunday()
    {
        // Every day for all of time, except Tuesday,Thursday,Saturday,and Sunday" Not working #298 

        var vEvent = new CalendarEvent
        {
            Summary = "BIO CLASS",//subject
            Description = "Details at CLASS",//description of meeting
            Location = "Building 101",//location
            DtStart = new CalDateTime(DateTime.Parse("2017-06-01T08:00")),
            DtEnd = new CalDateTime(DateTime.Parse("2017-06-01T09:30")),
            RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily, 1) },
        };

        //Define the exceptions: Sunday
        var exceptionRule = new RecurrencePattern(FrequencyType.Weekly, 1)
        {
            ByDay = new List<WeekDay> { new WeekDay(DayOfWeek.Sunday), new WeekDay(DayOfWeek.Saturday),
                new WeekDay(DayOfWeek.Tuesday),new WeekDay(DayOfWeek.Thursday)}
        };
        vEvent.ExceptionRules = new List<RecurrencePattern> { exceptionRule };

        var calendar = new Calendar();
        calendar.Events.Add(vEvent);

        var occurrences = vEvent.GetOccurrences(DateTime.Parse("2017-06-01T00:00"), DateTime.Parse("2017-06-30T23:59")).ToList();

        var excludedDays = new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday, DayOfWeek.Tuesday, DayOfWeek.Thursday };

        Assert.Multiple(() =>
        {
            Assert.That(occurrences.Count, Is.EqualTo(13));
            // Assert that none of the occurrences contain a weekday from the ByDay list
            foreach (var occurrence in occurrences)
            {
                Assert.That(excludedDays, Does.Not.Contain(occurrence.Period.StartTime.DayOfWeek));
            }
        });
    }
}
