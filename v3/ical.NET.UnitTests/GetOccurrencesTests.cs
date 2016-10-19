using System;
using System.Linq;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    internal class GetOccurrenceTests
    {
        [Test]
        public void WrongDurationTest()
        {
            var firstStart = new CalDateTime(DateTime.Parse("2016-01-01"));
            var firstEnd = new CalDateTime(DateTime.Parse("2016-01-05"));
            var vEvent = new CalendarEvent()
            {
                DtStart = firstStart,
                DtEnd = firstEnd,
            };

            var secondStart = new CalDateTime(DateTime.Parse("2016-03-01"));
            var secondEnd = new CalDateTime(DateTime.Parse("2016-03-05"));
            var vEvent2 = new CalendarEvent
            {
                DtStart = secondStart,
                DtEnd = secondEnd,
            };

            var calendar = new Calendar();
            calendar.Events.Add(vEvent);
            calendar.Events.Add(vEvent2);

            var searchStart = DateTime.Parse("2015-12-29");
            var searchEnd = DateTime.Parse("2017-02-10");
            var occurrences = calendar.GetOccurrences(searchStart, searchEnd).OrderBy(o => o.Period.StartTime).ToList();

            var firstOccurrence = occurrences.First();
            var firstStartCopy = firstStart.Copy<CalDateTime>();
            var firstEndCopy = firstEnd.Copy<CalDateTime>();
            Assert.AreEqual(firstStartCopy, firstOccurrence.Period.StartTime);
            Assert.AreEqual(firstEndCopy, firstOccurrence.Period.EndTime);

            var secondOccurrence = occurrences.Last();
            var secondStartCopy = secondStart.Copy<CalDateTime>();
            var secondEndCopy = secondEnd.Copy<CalDateTime>();
            Assert.AreEqual(secondStartCopy, secondOccurrence.Period.StartTime);
            Assert.AreEqual(secondEndCopy, secondOccurrence.Period.EndTime);
        }
    }
}
