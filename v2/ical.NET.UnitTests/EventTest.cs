using ical.net.DataTypes;
using ical.net.ExtensionMethods;
using ical.net.Interfaces;
using NUnit.Framework;
using System;
using System.Linq;

namespace ical.net.unittests
{
    [TestFixture]
    public class EventTest
    {
        /// <summary>
        /// Ensures that events can be properly added to a calendar.
        /// </summary>
        [Test, Category("Event")]
        public void Add1()
        {
            ICalendar cal = new Calendar();

            var evt = new Event
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
            ICalendar cal = new Calendar();

            var evt = new Event
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
            ICalendar cal = new Calendar();

            var evt = new Event
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
            ICalendar cal = new Calendar();

            // Do not set DTSTAMP manually
            var evt = new Event
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
            ICalendar cal = new Calendar();

            var evt = new Event
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
            var ical = new ical.net.Calendar();
            var evt = new ical.net.Event();
            evt.DtStamp = new CalDateTime(new DateTime(2016, 8, 17, 2, 30, 0, DateTimeKind.Utc));
            ical.Events.Add(evt);

            var serializer = new ical.net.Serialization.iCalendar.Serializers.CalendarSerializer();
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
            var ical = new ical.net.Calendar();
            var evt = new ical.net.Event();
            ical.Events.Add(evt);

            var serializer = new ical.net.Serialization.iCalendar.Serializers.CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(ical);

            var lines = serializedCalendar.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var result = lines.First(s => s.StartsWith("DTSTAMP"));
            Assert.AreEqual($"DTSTAMP:{evt.DtStamp.Year}{evt.DtStamp.Month:00}{evt.DtStamp.Day:00}T{evt.DtStamp.Hour:00}{evt.DtStamp.Minute:00}{evt.DtStamp.Second:00}Z", result);
        }
    }
}
