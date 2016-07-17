using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace ical.Net.UnitTests
{
    public class SymmetricSerializationTests
    {
        private static readonly DateTime _nowTime = DateTime.Now;
        private static readonly DateTime _later = _nowTime.AddHours(1);
        private static CalendarSerializer GetNewSerializer() => new CalendarSerializer(new SerializationContext());
        private static string SerializeToString(Calendar c) => GetNewSerializer().SerializeToString(c);
        private static Event GetSimpleEvent() => new Event {DtStart = new CalDateTime(_nowTime), DtEnd = new CalDateTime(_later),Duration = _later - _nowTime};
        private static ICalendar UnserializeCalendar(string s) => Calendar.LoadFromStream(new StringReader(s)).Single();

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

        [Test, TestCaseSource(nameof(AttendeeSerialization_TestCases))]
        public void AttendeeSerialization_Test(Attendee attendee)
        {
            var calendar = new Calendar();
            calendar.AddTimeZone(new VTimeZone("America/Los_Angeles"));
            var someEvent = GetSimpleEvent();
            someEvent.Attendees = new List<IAttendee> {attendee};
            calendar.Events.Add(someEvent);

            var serialized = SerializeToString(calendar);
            var unserialized = UnserializeCalendar(serialized);

            Assert.AreEqual(calendar.GetHashCode(), unserialized.GetHashCode());
            Assert.IsTrue(calendar.Events.SequenceEqual(unserialized.Events));
            Assert.AreEqual(calendar, unserialized);
        }

        public static IEnumerable<ITestCaseData> AttendeeSerialization_TestCases()
        {
            //TODO: Fix this. It appears to be non-deterministic. E.g. you can have one of the Delegated* properties uncommented, and it works, but not both
            var complex1 = new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Accepted",
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Accepted,
                //SentBy = new Uri("mailto:someone@example.com"), //Broken
                //DirectoryEntry = new Uri("ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)"), //Broken
                //Type = "CuType",
                //Members = new List<string> { "Group A", "Group B" },
                //Role = ParticipationRole.Chair,
                //DelegatedTo = new List<string> { "Peon A", "Peon B" },
                //DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B" }
            };
            yield return new TestCaseData(complex1).SetName("Complex attendee");

            var simple = new Attendee("MAILTO:james@example.com")
            {
                CommonName = "James James",
                Role = ParticipationRole.RequiredParticipant,
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Tentative
            };
            yield return new TestCaseData(simple).SetName("Simple attendee");
        }
    }
}
