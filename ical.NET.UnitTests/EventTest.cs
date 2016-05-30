using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces;
using NUnit.Framework;

namespace ical.NET.UnitTests
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
    }
}
