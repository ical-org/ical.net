//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class DocumentationExamples
{
    [Test]
    public void Daily_Test()
    {
        // The first instance of an event taking place on July 1, 2016 between 07:00 and 08:00.
        // We want it to recur through the end of July.
        var vEvent = new CalendarEvent
        {
            DtStart = new CalDateTime("20160701T070000"),
            DtEnd = new CalDateTime("20160701T080000"),
        };

        //Recur daily through the end of the day, July 31, 2016
        var recurrenceRule = new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Until = new CalDateTime("20160731T235959")
        };

        vEvent.RecurrenceRules = new List<RecurrencePattern> { recurrenceRule };
        var calendar = new Calendar();
        calendar.Events.Add(vEvent);


        // Count the occurrences between July 20, and Aug 5 -- there should be 12:
        // July 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        var searchStart = new CalDateTime(2016, 07, 20);
        var searchEnd = new CalDateTime(2016, 08, 05);
        var occurrences = calendar.GetOccurrences(searchStart).TakeBefore(searchEnd).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(12));
    }

    [Test]
    public void EveryOtherTuesdayUntilTheEndOfTheYear_Test()
    {
        // An event taking place between 07:00 and 08:00, beginning July 5 (a Tuesday)
        var vEvent = new CalendarEvent
        {
            DtStart = new CalDateTime(DateTime.Parse("2016-07-05T07:00", CultureInfo.InvariantCulture)),
            DtEnd = new CalDateTime(DateTime.Parse("2016-07-05T08:00",CultureInfo.InvariantCulture)),
        };

        // Recurring every other Tuesday until Dec 31
        var rrule = new RecurrencePattern(FrequencyType.Weekly, 2)
        {
            Until = new CalDateTime("20161231T115959")
        };
        vEvent.RecurrenceRules = new List<RecurrencePattern> { rrule };

        // Count every other Tuesday between July 1 and Dec 31.
        // The first Tuesday is July 5. There should be 13 in total
        var searchStart = new CalDateTime(2010, 01, 01);
        var searchEnd = new CalDateTime(2016, 12, 31);
        var tuesdays = vEvent.GetOccurrences(searchStart).TakeBefore(searchEnd).ToList();

        Assert.That(tuesdays, Has.Count.EqualTo(13));
    }

    [Test]
    public void FourthThursdayOfNovember_Tests()
    {
        // (The number of US thanksgivings between 2000 and 2016)
        // An event taking place between 07:00 and 19:00, beginning July 5 (a Tuesday)
        var vEvent = new CalendarEvent
        {
            DtStart = new CalDateTime(DateTime.Parse("2000-11-23T07:00", CultureInfo.InvariantCulture)),
            DtEnd = new CalDateTime(DateTime.Parse("2000-11-23T19:00", CultureInfo.InvariantCulture)),
        };

        // Recurring every other Tuesday until Dec 31
        var rrule = new RecurrencePattern(FrequencyType.Yearly, 1)
        {
            Frequency = FrequencyType.Yearly,
            Interval = 1,
            ByMonth = new List<int> { 11 },
            ByDay = new List<WeekDay> { new WeekDay { DayOfWeek = DayOfWeek.Thursday, Offset = 4 } },
        };
        vEvent.RecurrenceRules = new List<RecurrencePattern> { rrule };

        var searchStart = new CalDateTime(2000, 01, 01);
        var searchEnd = new CalDateTime(2017, 01, 01);
        var usThanksgivings = vEvent.GetOccurrences(searchStart).TakeBefore(searchEnd).ToList();

        Assert.That(usThanksgivings, Has.Count.EqualTo(17));
        foreach (var thanksgiving in usThanksgivings)
        {
            Assert.That(thanksgiving.Period.StartTime.DayOfWeek == DayOfWeek.Thursday, Is.True);
        }
    }

    [Test]
    public void DailyExceptSunday_Test()
    {
        //An event that happens daily through 2016, except for Sundays
        var vEvent = new CalendarEvent
        {
            DtStart = new CalDateTime(DateTime.Parse("2016-01-01T07:00", CultureInfo.InvariantCulture)),
            DtEnd = new CalDateTime(DateTime.Parse("2016-12-31T08:00", CultureInfo.InvariantCulture)),
            RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily, 1) },
        };

        //Define the exceptions: Sunday
        var exceptionRule = new RecurrencePattern(FrequencyType.Weekly, 1)
        {
            ByDay = new List<WeekDay> { new WeekDay(DayOfWeek.Sunday) }
        };
        vEvent.ExceptionRules = new List<RecurrencePattern> { exceptionRule };

        var calendar = new Calendar();
        calendar.Events.Add(vEvent);

        // We are essentially counting all the days that aren't Sunday in 2016, so there should be 314
        var searchStart = new CalDateTime(2015, 12, 31);
        var searchEnd = new CalDateTime(2017, 01, 01);
        var occurrences = calendar.GetOccurrences(searchStart).TakeBefore(searchEnd).ToList();
        Assert.That(occurrences, Has.Count.EqualTo(314));
    }
}
