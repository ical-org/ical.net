using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.CoreUnitTests
{
    public class DocumentationExamples
    {
        [Test]
        public void Daily_Test()
        {
            // The first instance of an event taking place on July 1, 2016 between 07:00 and 08:00.
            var vEvent = new CalendarEvent
            {
                DtStart = new CalDateTime(new DateTime(2016, 07, 01, 07, 0, 0)),
                DtEnd = new CalDateTime(new DateTime(2016, 07, 01, 08, 0, 0)),
            };

            //Recur daily through the end of the day, July 31, 2016
            var recurrenceRule = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Until = new DateTime(2016, 07, 31, 23, 59, 59)
            };

            vEvent.RecurrenceRules = new List<RecurrencePattern> {recurrenceRule};
            var calendar = new Calendar();
            calendar.Events.Add(vEvent);


            // Count the occurrences between July 20, and Aug 5 -- there should be 12:
            // July 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
            var searchStart = new DateTime(2016,07,20);
            var searchEnd = new DateTime(2016, 08, 05);
            var occurrences = calendar.GetOccurrences(searchStart, searchEnd);
            Assert.AreEqual(12, occurrences.Count);
        }

        [Test]
        public void EveryOtherTuesdayUntilTheEndOfTheYear_Test()
        {
            // An event taking place between 07:00 and 08:00, beginning July 5 (a Tuesday)
            var vEvent = new CalendarEvent
            {
                DtStart = new CalDateTime(new DateTime(2016, 07, 05, 07, 00, 0)),
                DtEnd = new CalDateTime(new DateTime(2016, 07, 05, 08, 00, 0)),
            };

            // Recurring every other Tuesday until Dec 31
            var rRule = new RecurrencePattern(FrequencyType.Weekly, 2)
            {
                Until = new DateTime(2016, 12, 31, 11, 59, 59),
                ByMonth = new List<int> { 10, 12 } //limit the Months, because > Weekly
            };
            vEvent.RecurrenceRules = new List<RecurrencePattern> { rRule };

            // Count every other Tuesday between July 1 and Dec 31.
            // The first Tuesday is July 5. There should be 13 in total
            var searchStart = new DateTime(2010, 01, 01);
            var searchEnd = new DateTime(2016, 12, 31);
            var tuesdays = vEvent.GetOccurrences(searchStart, searchEnd);

            Assert.AreEqual(5, tuesdays.Count);
        }

        [Test]
        public void FourthThursdayOfNovember_Tests()
        {
            // (The number of US thanksgivings between 2000 and 2016)
            // An event taking place between 07:00 and 19:00
            var vEvent = new CalendarEvent
            {
                DtStart = new CalDateTime(new DateTime(2000, 1, 23, 07, 0, 0)),
                DtEnd = new CalDateTime(new DateTime(2000, 1, 23, 19, 0, 0)),
                IncludeReferenceDate = false, //don't know the exact date 
            };

            // Recurring every 4. Thursday in November
            var rRule = new RecurrencePattern(FrequencyType.Yearly, 1)
            {
                Frequency = FrequencyType.Yearly,
                Interval = 1,
                ByMonth = new List<int> { 11 }, // < Yearly => only in November
                ByDay = new List<WeekDay> { new WeekDay { DayOfWeek = DayOfWeek.Thursday, Offset = 4 } },
                Until = DateTime.MaxValue
            };
            vEvent.RecurrenceRules = new List<RecurrencePattern> { rRule };

            var searchStart = new DateTime(2000, 01, 01);
            var searchEnd = new DateTime(2017, 01, 01);
            var usThanksgivings = vEvent.GetOccurrences(searchStart, searchEnd);

            Assert.AreEqual(17, usThanksgivings.Count);
            foreach (var thanksgiving in usThanksgivings)
            {
                Assert.IsTrue(thanksgiving.Period.StartTime.DayOfWeek == DayOfWeek.Thursday);
            }
        }

        [Test]
        public void DailyExceptSunday_Test()
        {
            //An event that happens daily through 2016, except for Sundays
            var vEvent = new CalendarEvent
            {
                DtStart = new CalDateTime(new DateTime(2016, 01, 01, 07, 0, 0)),
                DtEnd = new CalDateTime(new DateTime(2016, 12, 31, 08, 0, 0)),
                RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily, 1)},
            };

            //Define the exceptions: Sunday
            var exceptionRule = new RecurrencePattern(FrequencyType.Weekly, 1)
            {
                ByDay = new List<WeekDay> { new WeekDay(DayOfWeek.Sunday) }
            };
            vEvent.ExceptionRules = new List<RecurrencePattern> {exceptionRule};

            var calendar = new Calendar();
            calendar.Events.Add(vEvent);

            // We are essentially counting all the days that aren't Sunday in 2016, so there should be 314
            var searchStart = new DateTime(2015, 12, 31);
            var searchEnd = new DateTime(2017, 01, 01);
            var occurrences = calendar.GetOccurrences(searchStart, searchEnd);
            Assert.AreEqual(314, occurrences.Count);
        }
    }
}
