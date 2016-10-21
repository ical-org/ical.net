using Ical.Net.DataTypes;
using Ical.Net.ExtensionMethods;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    public class EventTest
    {
        private static readonly DateTime _now = DateTime.UtcNow;
        private static readonly DateTime _later = _now.AddHours(1);

        /// <summary>
        /// Ensures that events can be properly added to a calendar.
        /// </summary>
        [Test, Category("Event")]
        public void Add1()
        {
            Calendar cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.AreEqual(1, cal.Children.Count);
            Assert.AreSame(evt, cal.Children[0]);            
        }

        /// <summary>
        /// Ensures that events can be properly removed from a calendar.
        /// </summary>
        [Test, Category("Event")]
        public void Remove1()
        {
            Calendar cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.AreEqual(1, cal.Children.Count);
            Assert.AreSame(evt, cal.Children[0]);

            cal.RemoveChild(evt);
            Assert.AreEqual(0, cal.Children.Count);
            Assert.AreEqual(0, cal.Events.Count);
        }

        /// <summary>
        /// Ensures that events can be properly removed from a calendar.
        /// </summary>
        [Test, Category("Event")]
        public void Remove2()
        {
            Calendar cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.AreEqual(1, cal.Children.Count);
            Assert.AreSame(evt, cal.Children[0]);

            cal.Events.Remove(evt);
            Assert.AreEqual(0, cal.Children.Count);
            Assert.AreEqual(0, cal.Events.Count);
        }

        /// <summary>
        /// Ensures that event DTSTAMP is set.
        /// </summary>
        [Test, Category("Event")]
        public void EnsureDTSTAMPisNotNull()
        {
            Calendar cal = new Calendar();

            // Do not set DTSTAMP manually
            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.IsNotNull(evt.DtStamp);
        }

        /// <summary>
        /// Ensures that automatically set DTSTAMP property is of kind UTC.
        /// </summary>
        [Test, Category("Event")]
        public void EnsureDTSTAMPisOfTypeUTC()
        {
            Calendar cal = new Calendar();

            var evt = new CalendarEvent
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };

            cal.Events.Add(evt);
            Assert.IsTrue(evt.DtStamp.IsUniversalTime, "DTSTAMP should always be of type UTC.");
        }

        /// <summary>
        /// Ensures that correct set DTSTAMP property is being serialized with kind UTC.
        /// </summary>
        [Test, Category("Deserialization")]
        public void EnsureCorrectSetDTSTAMPisSerializedAsKindUTC()
        {
            var ical = new Ical.Net.Calendar();
            var evt = new Ical.Net.CalendarEvent();
            evt.DtStamp = new CalDateTime(new DateTime(2016, 8, 17, 2, 30, 0, DateTimeKind.Utc));
            ical.Events.Add(evt);

            var serializer = new Ical.Net.Serialization.iCalendar.Serializers.CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(ical);

            var lines = serializedCalendar.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var result = lines.First(s => s.StartsWith("DTSTAMP"));
            Assert.AreEqual("DTSTAMP:20160817T023000Z", result);
        }

        /// <summary>
        /// Ensures that automatically set DTSTAMP property is being serialized with kind UTC.
        /// </summary>
        [Test, Category("Deserialization")]
        public void EnsureAutomaticallySetDTSTAMPisSerializedAsKindUTC()
        {
            var ical = new Ical.Net.Calendar();
            var evt = new Ical.Net.CalendarEvent();
            ical.Events.Add(evt);

            var serializer = new Ical.Net.Serialization.iCalendar.Serializers.CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(ical);

            var lines = serializedCalendar.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var result = lines.First(s => s.StartsWith("DTSTAMP"));
            Assert.AreEqual($"DTSTAMP:{evt.DtStamp.Year}{evt.DtStamp.Month:00}{evt.DtStamp.Day:00}T{evt.DtStamp.Hour:00}{evt.DtStamp.Minute:00}{evt.DtStamp.Second:00}Z", result);
        }

        [Test]
        public void EventWithExDateShouldNotBeEqualToSameEventWithoutExDate()
        {
            const string icalNoException = @"BEGIN:VCALENDAR
PRODID:-//Telerik Inc.//NONSGML RadScheduler//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
BEGIN:VTIMEZONE
TZID:UTC
BEGIN:STANDARD
TZNAME:UTC
TZOFFSETTO:+0000
TZOFFSETFROM:+0000
DTSTART:16010101T000000
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
DTSTART;TZID=UTC:20161020T170000
DTEND;TZID=UTC:20161020T230000
UID:694f818f-6d67-4307-9c4d-0b5211686ff0
IMPORTANCE:None
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR";

            const string icalWithException = @"BEGIN:VCALENDAR
PRODID:-//Telerik Inc.//NONSGML RadScheduler//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
BEGIN:VTIMEZONE
TZID:UTC
BEGIN:STANDARD
TZNAME:UTC
TZOFFSETTO:+0000
TZOFFSETFROM:+0000
DTSTART:16010101T000000
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
DTSTART;TZID=UTC:20161020T170000
DTEND;TZID=UTC:20161020T230000
UID:694f818f-6d67-4307-9c4d-0b5211686ff0
IMPORTANCE:None
RRULE:FREQ=DAILY
EXDATE;TZID=UTC:20161020T170000
END:VEVENT
END:VCALENDAR";

            var noException = Calendar.LoadFromStream(new StringReader(icalNoException)).First().Events.First();
            var withException = Calendar.LoadFromStream(new StringReader(icalWithException)).First().Events.First();

            Assert.AreNotEqual(noException, withException);
            Assert.AreNotEqual(noException.GetHashCode(), withException.GetHashCode());
        }

        private static CalendarEvent GetSimpleEvent() => new CalendarEvent
        {
            DtStart = new CalDateTime(_now),
            DtEnd = new CalDateTime(_later),
        };

        [Test]
        public void RrulesAreSignificantTests()
        {
            var rrule = new RecurrencePattern(FrequencyType.Daily, 1);
            var testRrule = GetSimpleEvent();
            testRrule.RecurrenceRules = new List<RecurrencePattern> { rrule };

            var simpleEvent = GetSimpleEvent();
            Assert.AreNotEqual(simpleEvent, testRrule);
            Assert.AreNotEqual(simpleEvent.GetHashCode(), testRrule.GetHashCode());

            var testRdate = GetSimpleEvent();
            testRdate.RecurrenceDates = new List<PeriodList> { new PeriodList { new Period(new CalDateTime(_now)) } };
            Assert.AreNotEqual(simpleEvent, testRdate);
            Assert.AreNotEqual(simpleEvent.GetHashCode(), testRdate.GetHashCode());
        }
    }
}
