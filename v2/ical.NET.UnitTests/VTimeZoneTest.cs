using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.DataTypes;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers;
using NodaTime;
using NodaTime.TimeZones;
using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    class VTimeZoneTest
    {
        [Test, Category("VTimeZone")]
        public void InvalidTzIdShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => { new VTimeZone("shouldFail"); });
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaAnchorageShouldSerializeProperly()
        {
            var tzId = "America/Anchorage";
            var iCal = CreateTestICal(tzId);

            CalendarSerializer serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Anchorage"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:CAT"), "CAT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:CAWT"), "CAWT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AHST"), "AHST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AHDT"), "AHDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AKST"), "AKST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:YST"), "YST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AHDT"), "AHDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:LMT"), "LMT was not serialized");
            Assert.IsTrue(serialized.Contains("RDATE:19731028T020000"), "RDATE:19731028T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("RDATE:19801026T020000"), "RDATE:19801026T020000 was not serialized");
            Assert.IsTrue(serialized.Contains("DTSTART:19420209T020000"), "DTSTART:19420209T020000 was not serialized");
            Assert.IsFalse(serialized.Contains("RDATE:19670401/P1D"), "RDate was not properly serialized, should be RDATE:19670401T000000");
        }

        private static Calendar CreateTestICal(string tzId)
        {
            Calendar iCal = new Calendar();
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId) ?? DateTimeZoneProviders.Bcl.GetZoneOrNull(tzId);

            VTimeZone tz = VTimeZone.FromDateTimeZone(zone, new DateTime(1900, 1, 1), true);
            Assert.IsNotNull(tz);
            iCal.AddChild(tz);

            Event calEvent = new Event();
            calEvent.Description = "Test Recurring Event";
            calEvent.Start = new CalDateTime(DateTime.Now, tzId);
            calEvent.End = new CalDateTime(DateTime.Now.AddHours(1), tzId);
            calEvent.RecurrenceRules = new List<IRecurrencePattern>();
            calEvent.RecurrenceRules.Add(new RecurrencePattern(FrequencyType.Daily));
            iCal.Events.Add(calEvent);

            Event calEvent2 = new Event();
            calEvent2.Description = "Test Recurring Event 2";
            calEvent2.Start = new CalDateTime(DateTime.Now.AddHours(2), tzId);
            calEvent2.End = new CalDateTime(DateTime.Now.AddHours(3), tzId);
            calEvent2.RecurrenceRules = new List<IRecurrencePattern>();
            calEvent2.RecurrenceRules.Add(new RecurrencePattern(FrequencyType.Daily));
            iCal.Events.Add(calEvent2);
            return iCal;
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaEirunepeShouldSerializeProperly()
        {
            var tzId = "America/Eirunepe";
            var iCal = CreateTestICal(tzId);

            CalendarSerializer serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Eirunepe"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:ACST"), "ACST was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:ACT"), "ACT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:AMT"), "AMT was not serialized");

            // Should not contain the following
            Assert.IsFalse(serialized.Contains("RDATE:19501201T000000/P1D"), "The RDATE was not serialized correctly, should be RDATE:19501201T000000");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaDetroitShouldSerializeProperly()
        {
            var tzId = "America/Detroit";
            var iCal = CreateTestICal(tzId);

            CalendarSerializer serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.IsTrue(serialized.Contains("TZID:America/Detroit"), "Time zone not found in serialization");
            Assert.IsTrue(serialized.Contains("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:EDT"), "EDT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:EPT"), "EPT was not serialized");
            Assert.IsTrue(serialized.Contains("TZNAME:EST"), "EST was not serialized");
        }
    }
}
