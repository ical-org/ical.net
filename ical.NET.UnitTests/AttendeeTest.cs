using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class AttendeeTest
    {
        internal static Event EvtFactory()
        {
            return new Event
            {
                Summary = "Testing",
                Start = new CalDateTime(2010, 3, 25),
                End = new CalDateTime(2010, 3, 26)
            };
        }

        const string req = "REQ-PARTICIPANT"; //this string may be added to the api in the future
        internal static IList<Attendee> AttendeesFactory()
        {
            
            return new[] {
                new Attendee("MAILTO:james@test.com")
                {
                    CommonName = "James James",
                    Role = req,
                    Rsvp = true,
                    ParticipationStatus = ParticipationStatus.Tentative
                },
                new Attendee("MAILTO:mary@test.com")
                {
                    CommonName = "Mary Mary",
                    Role = req,
                    Rsvp = true,
                    ParticipationStatus = ParticipationStatus.Accepted
                }
            };
        }
        /// <summary>
        /// Ensures that attendees can be properly added to an event.
        /// </summary>
        [Test, Category("Attendee")]
        public void Add1Attendee()
        {
            var evt = EvtFactory();
            Assert.AreEqual(0, evt.Attendees.Count);

            var at = AttendeesFactory();

            evt.Attendees.Add(at[0]);
            Assert.AreEqual(1, evt.Attendees.Count);
            Assert.AreSame(at[0], evt.Attendees[0]);

            //the properties below had been set to null during the Attendees.Add operation in NuGet version 2.1.4
            Assert.AreEqual(req, evt.Attendees[0].Role); 
            Assert.AreEqual(ParticipationStatus.Tentative, evt.Attendees[0].ParticipationStatus);
        }

        [Test, Category("Attendee")]
        public void Add2Attendees()
        {
            var evt = EvtFactory();
            Assert.AreEqual(0, evt.Attendees.Count);

            var at = AttendeesFactory();

            evt.Attendees.Add(at[0]);
            evt.Attendees.Add(at[1]);
            Assert.AreEqual(2, evt.Attendees.Count);
            Assert.AreSame(at[1], evt.Attendees[1]);

            Assert.AreEqual(req, evt.Attendees[1].Role);

            var cal = new Calendar();
            cal.Events.Add(evt);
            var serializer = new CalendarSerializer(new SerializationContext());
            Console.Write(serializer.SerializeToString(cal));
        }

        /// <summary>
        /// Ensures that attendees can be properly removed from an event.
        /// </summary>
        [Test, Category("Attendee")]
        public void Remove1Attendee()
        {
            var evt = EvtFactory();

            Assert.AreEqual(0, evt.Attendees.Count);
            var at = AttendeesFactory()[0];
            evt.Attendees.Add(at);
            Assert.AreEqual(1, evt.Attendees.Count);
            Assert.AreSame(at, evt.Attendees[0]);

            evt.Attendees.Remove(at);
            Assert.AreEqual(0, evt.Attendees.Count);
        }

    }
}
