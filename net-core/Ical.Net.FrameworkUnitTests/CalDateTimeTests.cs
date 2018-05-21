using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.FrameworkUnitTests
{
    public class CalDateTimeTests
    {
        private static readonly DateTime _now = DateTime.Now;
        private static readonly DateTime _later = _now.AddHours(1);
        private static CalendarEvent GetEventWithRecurrenceRules(string tzId)
        {
            var dailyForFiveDays = new RecurrencePattern(FrequencyType.Daily, 1)
            {
                Count = 5,
            };

            var calendarEvent = new CalendarEvent
            {
                Start = new CalDateTime(_now, tzId),
                End = new CalDateTime(_later, tzId),
                RecurrenceRules = new List<RecurrencePattern> { dailyForFiveDays },
                Resources = new List<string>(new[] { "Foo", "Bar", "Baz" }),
            };
            return calendarEvent;
        }

        [Test, TestCaseSource(nameof(ToTimeZoneTestCases))]
        public void ToTimeZoneTests(CalendarEvent calendarEvent, string targetTimeZone)
        {
            var startAsUtc = calendarEvent.Start.AsUtc;
            
            var convertedStart = calendarEvent.Start.ToTimeZone(targetTimeZone);
            var convertedAsUtc = convertedStart.AsUtc;

            Assert.AreEqual(startAsUtc, convertedAsUtc);
        }

        public static IEnumerable<ITestCaseData> ToTimeZoneTestCases()
        {
            const string bclCst = "Central Standard Time";
            const string bclEastern = "Eastern Standard Time";
            var bclEvent = GetEventWithRecurrenceRules(bclCst);
            yield return new TestCaseData(bclEvent, bclEastern)
                .SetName($"BCL to BCL: {bclCst} to {bclEastern}");

            const string ianaNy = "America/New_York";
            const string ianaRome = "Europe/Rome";
            var ianaEvent = GetEventWithRecurrenceRules(ianaNy);

            yield return new TestCaseData(ianaEvent, ianaRome)
                .SetName($"IANA to IANA: {ianaNy} to {ianaRome}");

            const string utc = "UTC";
            var utcEvent = GetEventWithRecurrenceRules(utc);
            yield return new TestCaseData(utcEvent, utc)
                .SetName("UTC to UTC");

            yield return new TestCaseData(bclEvent, ianaRome)
                .SetName($"BCL to IANA: {bclCst} to {ianaRome}");

            yield return new TestCaseData(ianaEvent, bclCst)
                .SetName($"IANA to BCL: {ianaNy} to {bclCst}");
        }

        [TestCaseSource(nameof(AsDateTimeOffsetTestCases))]
        public DateTimeOffset AsDateTimeOffsetTests(CalDateTime incoming)
            => incoming.AsDateTimeOffset;

        public static IEnumerable<ITestCaseData> AsDateTimeOffsetTestCases()
        {
            const string nyTzId = "America/New_York";
            var summerDate = DateTime.Parse("2018-05-15T11:00");

            var nySummerOffset = TimeSpan.FromHours(-4);
            var nySummer = new CalDateTime(summerDate, nyTzId);
            yield return new TestCaseData(nySummer)
                .SetName("NY Summer DateTime returns DateTimeOffset with UTC-4")
                .Returns(new DateTimeOffset(summerDate, nySummerOffset));

            var utc = new CalDateTime(summerDate, "UTC");
            yield return new TestCaseData(utc)
                .SetName("UTC summer DateTime returns a DateTimeOffset with UTC-0")
                .Returns(new DateTimeOffset(summerDate, TimeSpan.Zero));

            var convertedToNySummer = new CalDateTime(summerDate, "UTC");
            convertedToNySummer.TzId = nyTzId;
            yield return new TestCaseData(convertedToNySummer)
                .SetName("Summer UTC DateTime converted to NY time zone by setting TzId returns a DateTimeOffset with UTC-4")
                .Returns(new DateTimeOffset(summerDate, nySummerOffset));

            var noTz = new CalDateTime(summerDate);
            var currentSystemOffset = TimeZoneInfo.Local.GetUtcOffset(summerDate);
            yield return new TestCaseData(noTz)
                .SetName($"Summer DateTime with no time zone information returns the system-local's UTC offset ({currentSystemOffset})")
                .Returns(new DateTimeOffset(summerDate, currentSystemOffset));
        }

        [Test(Description = "Calling AsUtc should always return the proper UTC time, even if the TzId has changed")]
        public void TestTzidChanges()
        {
            var someTime = DateTimeOffset.Parse("2018-05-21T11:35:00-04:00");

            var someDt = new CalDateTime(someTime.DateTime) { TzId = "America/New_York" };
            var firstUtc = someDt.AsUtc;
            Assert.AreEqual(someTime.UtcDateTime, firstUtc);

            someDt.TzId = "Europe/Berlin";
            var berlinUtc = someDt.AsUtc;
            Assert.AreNotEqual(firstUtc, berlinUtc);
        }
    }
}
