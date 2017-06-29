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
        public void VTimeZoneAmericaDetroitShouldSerializeProperly()
        {
            var tzId = "America/Detroit";
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

            CalendarSerializer serializer = new CalendarSerializer();
            var ms = new MemoryStream();
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
