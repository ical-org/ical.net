using System;
using System.Collections.Generic;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.UnitTests
{
    public class EqualityAndHashingTests
    {
        private const string _someTz = "America/Los_Angeles";
        private static readonly DateTime _nowTime = DateTime.Parse("2016-07-16T16:47:02.9310521-04:00");
        private static readonly DateTime _later = _nowTime.AddHours(1);

        [Test, TestCaseSource(nameof(CalDateTime_TestCases))]
        public void CalDateTime_Tests(CalDateTime incomingDt, CalDateTime expectedDt)
        {
            Assert.AreEqual(incomingDt.Value, expectedDt.Value);
            Assert.AreEqual(incomingDt.GetHashCode(), expectedDt.GetHashCode());
            Assert.AreEqual(incomingDt.TzId, expectedDt.TzId);
            Assert.IsTrue(incomingDt.Equals(expectedDt));
        }

        public static IEnumerable<ITestCaseData> CalDateTime_TestCases()
        {
            var nowCalDt = new CalDateTime(_nowTime);
            yield return new TestCaseData(nowCalDt, new CalDateTime(_nowTime)).SetName("Now, no time zone");

            var nowCalDtWithTz = new CalDateTime(_nowTime, _someTz);
            yield return new TestCaseData(nowCalDtWithTz, new CalDateTime(_nowTime, _someTz)).SetName("Now, with time zone");
        }

        [Test, TestCaseSource(nameof(Event_TestCases))]
        public void Event_Tests(Event incoming, Event expected)
        {
            Assert.AreEqual(incoming.DtStart, expected.DtStart);
            Assert.AreEqual(incoming.DtEnd, expected.DtEnd);
            Assert.AreEqual(incoming.Location, expected.Location);
            Assert.AreEqual(incoming.Status, expected.Status);
            Assert.AreEqual(incoming.IsActive(), expected.IsActive());
            Assert.AreEqual(incoming.Duration, expected.Duration);
            Assert.AreEqual(incoming.Transparency, expected.Transparency);
            Assert.AreEqual(incoming.GetHashCode(), expected.GetHashCode());
            Assert.IsTrue(incoming.Equals(expected));
        }

        public static IEnumerable<ITestCaseData> Event_TestCases()
        {
            var outgoing = new Event
            {
                DtStart = new CalDateTime(_nowTime),
                DtEnd = new CalDateTime(_later),
                Duration = TimeSpan.FromHours(1),
            };

            var expected = new Event
            {
                DtStart = new CalDateTime(_nowTime),
                DtEnd = new CalDateTime(_later),
                Duration = TimeSpan.FromHours(1),
            };
            yield return new TestCaseData(outgoing, expected).SetName("Events with start, end, and duration");

            var fiveA = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };

            var fiveB = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };
            outgoing.RecurrenceRules = new List<IRecurrencePattern> {fiveA};
            expected.RecurrenceRules = new List<IRecurrencePattern> {fiveB};
            yield return new TestCaseData(outgoing, expected).SetName("Events with start, end, duration, and one recurrence rule");
        }

        [Test]
        public void Calendar_Tests()
        {
            var rruleA = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };

            var e = new Event
            {
                DtStart = new CalDateTime(_nowTime),
                DtEnd = new CalDateTime(_later),
                Duration = TimeSpan.FromHours(1),
                RecurrenceRules = new List<IRecurrencePattern> { rruleA },
            };

            var actualCalendar = new Calendar();
            actualCalendar.Events.Add(e);

            //Work around referential equality...
            var rruleB = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5
            };

            var expectedCalendar = new Calendar();
            expectedCalendar.Events.Add(new Event
            {
                DtStart = new CalDateTime(_nowTime),
                DtEnd = new CalDateTime(_later),
                Duration = TimeSpan.FromHours(1),
                RecurrenceRules = new List<IRecurrencePattern> { rruleB },
            });

            Assert.AreEqual(actualCalendar.GetHashCode(), expectedCalendar.GetHashCode());
            Assert.IsTrue(actualCalendar.Equals(expectedCalendar));
        }

        [Test, TestCaseSource(nameof(VTimeZone_TestCases))]
        public void VTimeZone_Tests(VTimeZone actual, VTimeZone expected)
        {
            Assert.AreEqual(actual.Url, expected.Url);
            Assert.AreEqual(actual.TzId, expected.TzId);
            Assert.AreEqual(actual, expected);
            Assert.AreEqual(actual.GetHashCode(), expected.GetHashCode());
        }

        public static IEnumerable<ITestCaseData> VTimeZone_TestCases()
        {
            var first = new VTimeZone
            {
                TzId = "New Zealand Standard Time"
            };

            var second = new VTimeZone("New Zealand Standard Time");
            yield return new TestCaseData(first, second);

            first.Url = new Uri("http://example.com/");
            second.Url = new Uri("http://example.com");
            yield return new TestCaseData(first, second);
        }

        [Test, TestCaseSource(nameof(Attendees_TestCases))]
        public void Attendees_Tests(Attendee actual, Attendee expected)
        {
            Assert.AreEqual(expected.GetHashCode(), actual.GetHashCode());
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<ITestCaseData> Attendees_TestCases()
        {
            var tentative1 = new Attendee("MAILTO:james@example.com")
            {
                CommonName = "James Tentative",
                Role = ParticipationRole.RequiredParticipant,
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Tentative
            };
            var tentative2 = new Attendee("MAILTO:james@example.com")
            {
                CommonName = "James Tentative",
                Role = ParticipationRole.RequiredParticipant,
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Tentative
            };
            yield return new TestCaseData(tentative1, tentative2).SetName("Simple attendee test case");

            var complex1 = new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Accepted",
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Accepted,
                SentBy = new Uri("mailto:someone@example.com"),
                DirectoryEntry = new Uri("ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)"),
                Type = "CuType",
                Members = new List<string> { "Group A", "Group B"},
                Role = ParticipationRole.Chair,
                DelegatedTo = new List<string> { "Peon A", "Peon B"},
                DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B"}
            };
            var complex2 = new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Accepted",
                Rsvp = true,
                ParticipationStatus = EventParticipationStatus.Accepted,
                SentBy = new Uri("mailto:someone@example.com"),
                DirectoryEntry = new Uri("ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)"),
                Type = "CuType",
                Members = new List<string> { "Group A", "Group B" },
                Role = ParticipationRole.Chair,
                DelegatedTo = new List<string> { "Peon A", "Peon B" },
                DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B" }
            };
            yield return new TestCaseData(complex1, complex2).SetName("Complex attendee test");
        }

        [Test, TestCaseSource(nameof(CalendarCollection_TestCases))]
        public void CalendarCollection_Tests(string rawCalendar)
        {
            var a = Calendar.LoadFromStream(new StringReader(IcsFiles.USHolidays)) as CalendarCollection;
            var b = Calendar.LoadFromStream(new StringReader(IcsFiles.USHolidays)) as CalendarCollection;
            
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreEqual(a, b);
        }

        public static IEnumerable<ITestCaseData> CalendarCollection_TestCases()
        {
            yield return new TestCaseData(IcsFiles.Google1).SetName("Google calendar test case");
            yield return new TestCaseData(IcsFiles.Parse1).SetName("Weird file parse test case");
            yield return new TestCaseData(IcsFiles.USHolidays).SetName("US Holidays (quite large)");
        }
    }
}
