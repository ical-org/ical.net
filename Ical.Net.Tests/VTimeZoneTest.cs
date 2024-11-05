//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests
{
    public class VTimeZoneTest
    {
        [Test, Category("VTimeZone")]
        public void InvalidTzIdShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => new VTimeZone("shouldFail"));
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneFromDateTimeZoneNullZoneShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => CreateTestCalendar("shouldFail"));
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaPhoenixShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Phoenix");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:America/Phoenix"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("DTSTART:19670430T020000"), Is.True, "Daylight savings for Phoenix was not serialized properly.");
            });
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaPhoenixShouldSerializeProperly2()
        {
            var iCal = CreateTestCalendar("America/Phoenix", DateTime.Now, false);
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:America/Phoenix"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.False, "Daylight savings should not exist for Phoenix.");
            });
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneUsMountainStandardTimeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("US Mountain Standard Time");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:US Mountain Standard Time"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True);
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True);
                Assert.That(serialized.Contains("X-LIC-LOCATION"), Is.True, "X-LIC-LOCATION was not serialized");
            });
        }

        [Test, Category("VTimeZone")]
        public void VTimeZonePacificKiritimatiShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Pacific/Kiritimati");
            var serializer = new CalendarSerializer();
            Assert.DoesNotThrow(() => serializer.SerializeToString(iCal));
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneCentralAmericaStandardTimeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Central America Standard Time");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.That(serialized.Contains("TZID:Central America Standard Time"), Is.True, "Time zone not found in serialization");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneEasternStandardTimeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Eastern Standard Time");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.That(serialized.Contains("TZID:Eastern Standard Time"), Is.True, "Time zone not found in serialization");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneEuropeMoscowShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Europe/Moscow");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:Europe/Moscow"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True, "The standard timezone info was not serialized");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True, "The daylight timezone info was not serialized");
            });
            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZNAME:MSD"), Is.True, "MSD was not serialized");
                Assert.That(serialized.Contains("TZNAME:MSK"), Is.True, "MSK info was not serialized");
                Assert.That(serialized.Contains("TZNAME:MSD"), Is.True, "MSD was not serialized");
                Assert.That(serialized.Contains("TZNAME:MST"), Is.True, "MST was not serialized");
                Assert.That(serialized.Contains("TZNAME:MMT"), Is.True, "MMT was not serialized");
                Assert.That(serialized.Contains("TZOFFSETFROM:+023017"), Is.True, "TZOFFSETFROM:+023017 was not serialized");
                Assert.That(serialized.Contains("TZOFFSETTO:+023017"), Is.True, "TZOFFSETTO:+023017 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19180916T010000"), Is.True, "DTSTART:19180916T010000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19171228T000000"), Is.True, "DTSTART:19171228T000000 was not serialized");
                Assert.That(serialized.Contains("RDATE:19991031T030000"), Is.True, "RDATE:19991031T030000 was not serialized");
            });
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaChicagoShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Chicago");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:America/Chicago"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True, "The standard timezone info was not serialized");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True, "The daylight timezone info was not serialized");
                Assert.That(serialized.Contains("TZNAME:CDT"), Is.True, "CDT was not serialized");
                Assert.That(serialized.Contains("TZNAME:CST"), Is.True, "CST was not serialized");
                Assert.That(serialized.Contains("TZNAME:EST"), Is.True, "EST was not serialized");
                Assert.That(serialized.Contains("TZNAME:CWT"), Is.True, "CWT was not serialized");
                Assert.That(serialized.Contains("TZNAME:CPT"), Is.True, "CPT was not serialized");
                Assert.That(serialized.Contains("DTSTART:19181027T020000"), Is.True, "DTSTART:19181027T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19450814T180000"), Is.True, "DTSTART:19450814T180000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19420209T020000"), Is.True, "DTSTART:19420209T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19360301T020000"), Is.True, "DTSTART:19360301T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:20070311T020000"), Is.True, "DTSTART:20070311T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:20071104T020000"), Is.True, "DTSTART:20071104T020000 was not serialized");
            });
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaLosAngelesShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Los_Angeles");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:America/Los_Angeles"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True, "The standard timezone info was not serialized");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True, "The daylight timezone info was not serialized");
                Assert.That(serialized.Contains("BYDAY=2SU"), Is.True, "BYDAY=2SU was not serialized");
                Assert.That(serialized.Contains("TZNAME:PDT"), Is.True, "PDT was not serialized");
                Assert.That(serialized.Contains("TZNAME:PST"), Is.True, "PST was not serialized");
                Assert.That(serialized.Contains("TZNAME:PPT"), Is.True, "PPT was not serialized");
                Assert.That(serialized.Contains("TZNAME:PWT"), Is.True, "PWT was not serialized");
                Assert.That(serialized.Contains("DTSTART:19180331T020000"), Is.True, "DTSTART:19180331T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:20071104T020000"), Is.True, "DTSTART:20071104T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:20070311T020000"), Is.True, "DTSTART:20070311T020000 was not serialized");
            });

            //Assert.IsTrue(serialized.Contains("TZURL:http://tzurl.org/zoneinfo/America/Los_Angeles"), "TZURL:http://tzurl.org/zoneinfo/America/Los_Angeles was not serialized");
            //Assert.IsTrue(serialized.Contains("RDATE:19600424T010000"), "RDATE:19600424T010000 was not serialized");  // NodaTime doesn't match with what tzurl has
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneEuropeOsloShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("Europe/Oslo");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:Europe/Oslo"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True, "The standard timezone info was not serialized");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True, "The daylight timezone info was not serialized");
                Assert.That(serialized.Contains("BYDAY=-1SU;BYMONTH=3"), Is.True, "BYDAY=-1SU;BYMONTH=3 was not serialized");
                Assert.That(serialized.Contains("BYDAY=-1SU;BYMONTH=10"), Is.True, "BYDAY=-1SU;BYMONTH=10 was not serialized");
            });

        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaAnchorageShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Anchorage");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:America/Anchorage"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True, "The standard timezone info was not serialized");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True, "The daylight timezone info was not serialized");
                Assert.That(serialized.Contains("TZNAME:AHST"), Is.True, "AHST was not serialized");
                Assert.That(serialized.Contains("TZNAME:AHDT"), Is.True, "AHDT was not serialized");
                Assert.That(serialized.Contains("TZNAME:AKST"), Is.True, "AKST was not serialized");
                Assert.That(serialized.Contains("TZNAME:YST"), Is.True, "YST was not serialized");
                Assert.That(serialized.Contains("TZNAME:AHDT"), Is.True, "AHDT was not serialized");
                Assert.That(serialized.Contains("TZNAME:LMT"), Is.True, "LMT was not serialized");
                Assert.That(serialized.Contains("RDATE:19731028T020000"), Is.True, "RDATE:19731028T020000 was not serialized");
                Assert.That(serialized.Contains("RDATE:19801026T020000"), Is.True, "RDATE:19801026T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19420209T020000"), Is.True, "DTSTART:19420209T020000 was not serialized");
                Assert.That(serialized.Contains("RDATE:19670401/P1D"), Is.False, "RDate was not properly serialized for vtimezone, should be RDATE:19670401T000000");
            });
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaEirunepeShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Eirunepe");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:America/Eirunepe"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True, "The standard timezone info was not serialized");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True, "The daylight timezone info was not serialized");
                Assert.That(serialized.Contains("TZNAME:-04"), Is.True, "-04 was not serialized");
                Assert.That(serialized.Contains("TZNAME:-05"), Is.True, "-05 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19311003T110000"), Is.True, "DTSTART:19311003T110000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19320401T000000"), Is.True, "DTSTART:19320401T000000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:20080624T000000"), Is.True, "DTSTART:20080624T000000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:19501201T000000"), Is.True, "DTSTART:19501201T000000 was not serialized");
            });

            // Should not contain the following
            Assert.That(serialized.Contains("RDATE:19501201T000000/P1D"), Is.False, "The RDATE was not serialized correctly, should be RDATE:19501201T000000");
        }

        [Test, Category("VTimeZone")]
        public void VTimeZoneAmericaDetroitShouldSerializeProperly()
        {
            var iCal = CreateTestCalendar("America/Detroit");
            var serializer = new CalendarSerializer();
            var serialized = serializer.SerializeToString(iCal);

            Assert.Multiple(() =>
            {
                Assert.That(serialized.Contains("TZID:America/Detroit"), Is.True, "Time zone not found in serialization");
                Assert.That(serialized.Contains("BEGIN:STANDARD"), Is.True, "The standard timezone info was not serialized");
                Assert.That(serialized.Contains("BEGIN:DAYLIGHT"), Is.True, "The daylight timezone info was not serialized");
                Assert.That(serialized.Contains("TZNAME:EDT"), Is.True, "EDT was not serialized");
                Assert.That(serialized.Contains("TZNAME:EPT"), Is.True, "EPT was not serialized");
                Assert.That(serialized.Contains("TZNAME:EST"), Is.True, "EST was not serialized");
                Assert.That(serialized.Contains("DTSTART:20070311T020000"), Is.True, "DTSTART:20070311T020000 was not serialized");
                Assert.That(serialized.Contains("DTSTART:20071104T020000"), Is.True, "DTSTART:20071104T020000 was not serialized");
            });
        }

        private static Calendar CreateTestCalendar(string tzId, DateTime? earliestTime = null, bool includeHistoricalData = true)
        {
            var iCal = new Calendar();

            if (earliestTime == null)
            {
                earliestTime = new DateTime(1900, 1, 1);
            }
            iCal.AddTimeZone(tzId, earliestTime.Value, includeHistoricalData);

            var calEvent = new CalendarEvent
            {
                Description = "Test Recurring Event",
                Start = new CalDateTime(DateTime.Now, tzId),
                End = new CalDateTime(DateTime.Now.AddHours(1), tzId),
                RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily) }
            };
            iCal.Events.Add(calEvent);

            var calEvent2 = new CalendarEvent
            {
                Description = "Test Recurring Event 2",
                Start = new CalDateTime(DateTime.Now.AddHours(2), tzId),
                End = new CalDateTime(DateTime.Now.AddHours(3), tzId),
                RecurrenceRules = new List<RecurrencePattern> { new RecurrencePattern(FrequencyType.Daily) }
            };
            iCal.Events.Add(calEvent2);
            return iCal;
        }
    }
}
