using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.UnitTests
{
    public class SymmetricSerializationTests
    {
        private static readonly DateTime _nowTime = DateTime.Now;
        private static readonly DateTime _later = _nowTime.AddHours(1);
        private static CalendarSerializer GetNewSerializer() => new CalendarSerializer(new SerializationContext());
        private static string SerializeToString(Calendar c) => GetNewSerializer().SerializeToString(c);
        private static Event GetSimpleEvent() => new Event {DtStart = new CalDateTime(_nowTime), DtEnd = new CalDateTime(_later), Duration = _later - _nowTime};
        private static ICalendar UnserializeCalendar(string s) => Calendar.LoadFromStream(new StringReader(s)).Single();

        [Test, TestCaseSource(nameof(Event_TestCases))]
        public void Event_Tests(Calendar iCalendar)
        {
            var originalEvent = iCalendar.Events.Single();

            var serializedCalendar = SerializeToString(iCalendar);
            var unserializedCalendar = UnserializeCalendar(serializedCalendar);

            var onlyEvent = unserializedCalendar.Events.Single();

            Assert.AreEqual(originalEvent.GetHashCode(), onlyEvent.GetHashCode());
            Assert.AreEqual(originalEvent, onlyEvent);
            Assert.AreEqual(iCalendar, unserializedCalendar);
        }

        public static IEnumerable<ITestCaseData> Event_TestCases()
        {
            var rrule = new RecurrencePattern(FrequencyType.Daily, 1) { Count = 5};
            var e = new Event
            {
                DtStart = new CalDateTime(_nowTime),
                DtEnd = new CalDateTime(_later),
                Duration = TimeSpan.FromHours(1),
                RecurrenceRules = new List<IRecurrencePattern> { rrule },
            };

            var calendar = new Calendar();
            calendar.Events.Add(e);

            yield return new TestCaseData(calendar).SetName("readme.md example");
            e = GetSimpleEvent();
            e.Description = "This is an event description that is really rather long. Hopefully the line breaks work now, and it's serialized properly.";

            calendar = new Calendar();
            calendar.Events.Add(e);
            yield return new TestCaseData(calendar).SetName("Description serialization isn't working properly. Issue #60");
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

        [Test, TestCaseSource(nameof(BinaryAttachment_TestCases))]
        public void BinaryAttachment_Tests(string theString)
        {
            var asBytes = Encoding.UTF8.GetBytes(theString);
            var attachment = new Attachment
            {
                Data = asBytes,
                ValueEncoding = Encoding.UTF8,
            };

            var calendar = new Calendar();
            var vEvent = GetSimpleEvent();
            vEvent.Attachments = new List<IAttachment> { attachment };
            calendar.Events.Add(vEvent);

            var serialized = SerializeToString(calendar);
            var unserialized = UnserializeCalendar(serialized);

            Assert.AreEqual(calendar.GetHashCode(), unserialized.GetHashCode());
            Assert.AreEqual(calendar, unserialized);
        }

        public static IEnumerable<ITestCaseData> BinaryAttachment_TestCases()
        {
            yield return new TestCaseData("This is a string.")
                .SetName("Short string");
            yield return new TestCaseData("This is a stringThisThis is a")
                .SetName("Moderate string fails");
            yield return new TestCaseData("This is a song that never ends. It just goes on and on my friends. Some people started singing it not...")
                .SetName("Much longer string");

            const string jsonSerialized =
                "{\"TheList\":[\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\",\"Foo\",\"Bar\",\"Baz\"],\"TheNumber\":42,\"TheSet\":[\"Foo\",\"Bar\",\"Baz\"]}";
            yield return new TestCaseData(jsonSerialized).SetName("JSON-serialized text");
        }
    }
}
