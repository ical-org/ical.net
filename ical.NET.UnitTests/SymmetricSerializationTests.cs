using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NUnit.Framework;

namespace ical.Net.UnitTests
{
    public class SymmetricSerializationTests
    {
        [Test]
        public void SimpleEvent_Test()
        {
            //This is example from the readme
            var now = DateTime.Now;
            var later = now.AddHours(1);

            var rrule = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };

            var e = new Event
            {
                DtStart = new CalDateTime(now),
                DtEnd = new CalDateTime(later),
                Duration = TimeSpan.FromHours(1),
                RecurrenceRules = new List<IRecurrencePattern> { rrule },
            };

            var calendar = new Calendar();
            calendar.Events.Add(e);

            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(calendar);

            var unserializedCalendarCollection = Calendar.LoadFromStream(new StringReader(serializedCalendar));

            var onlyCalendar = unserializedCalendarCollection.Single();
            var onlyEvent = onlyCalendar.Events.Single();

            Assert.AreEqual(e.GetHashCode(), onlyEvent.GetHashCode());
            Assert.AreEqual(e, onlyEvent);
            Assert.AreEqual(calendar, onlyCalendar);
        }

        [Test]
        public void VTimeZoneSerialization_Test()
        {
            var originalCalendar = new Calendar();
            var tz = new VTimeZone
            {
                TzId = "New Zealand Standard Time"
            };
            originalCalendar.AddTimeZone(tz);
            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(originalCalendar);
            var unserializedCalendar = Calendar.LoadFromStream(new StringReader(serializedCalendar)).Single();

            CollectionAssert.AreEqual(originalCalendar.TimeZones, unserializedCalendar.TimeZones);
            Assert.AreEqual(originalCalendar, unserializedCalendar);
            Assert.AreEqual(originalCalendar.GetHashCode(), unserializedCalendar.GetHashCode());
        }
    }
}
