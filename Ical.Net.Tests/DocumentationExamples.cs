//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NodaTime.Extensions;
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

            //Recur daily through the end of the day, July 31, 2016
            RecurrenceRule = new(FrequencyType.Daily, 1)
            {
                Until = new CalDateTime("20160731T235959")
            }
        };

        var calendar = new Calendar();
        calendar.Events.Add(vEvent);

        // Count the occurrences between July 20, and Aug 5 -- there should be 12:
        // July 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        var searchStart = new CalDateTime(2016, 07, 20).ToZonedDateTime("America/New_York");
        var searchEnd = new CalDateTime(2016, 08, 05).ToZonedDateTime("America/New_York").ToInstant();
        var occurrences = calendar.GetOccurrences(searchStart).TakeWhileBefore(searchEnd).ToList();
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

            // Recurring every other Tuesday until Dec 31
            RecurrenceRule = new(FrequencyType.Weekly, 2)
            {
                Until = new CalDateTime("20161231T115959")
            }
        };

        // Count every other Tuesday between July 1 and Dec 31.
        // The first Tuesday is July 5. There should be 13 in total
        var searchStart = new CalDateTime(2010, 01, 01).ToZonedDateTime("America/New_York");
        var searchEnd = new CalDateTime(2016, 12, 31).ToZonedDateTime("America/New_York").ToInstant();
        var tuesdays = vEvent.GetOccurrences(searchStart).TakeWhileBefore(searchEnd).ToList();

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

            // Recurring every other Tuesday until Dec 31
            RecurrenceRule = new(FrequencyType.Yearly, 1)
            {
                Frequency = FrequencyType.Yearly,
                Interval = 1,
                ByMonth = [11],
                ByDay = [new WeekDay { DayOfWeek = DayOfWeek.Thursday, Offset = 4 }],
            }
        };

        var searchStart = new CalDateTime(2000, 01, 01).ToZonedDateTime("America/New_York");
        var searchEnd = new CalDateTime(2017, 01, 01).ToZonedDateTime("America/New_York").ToInstant();
        var usThanksgivings = vEvent.GetOccurrences(searchStart).TakeWhileBefore(searchEnd).ToList();

        Assert.That(usThanksgivings, Has.Count.EqualTo(17));
        foreach (var thanksgiving in usThanksgivings)
        {
            Assert.That(thanksgiving.Start.DayOfWeek == DayOfWeek.Thursday.ToIsoDayOfWeek(), Is.True);
        }
    }
}
