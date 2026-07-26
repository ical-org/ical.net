//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.TimeZones;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class VTimeZoneTest
{
    [Test, Category("VTimeZone")]
    public void CustomIdShouldNotThrow()
    {
        Assert.DoesNotThrow(() => new VTimeZone("shouldFail"));
    }

    [Test, Category("VTimeZone")]
    public void InvalidTzIdFromDateTimeZoneShouldThrow()
    {
        Assert.Throws<DateTimeZoneNotFoundException>(() => CreateTestCalendar("shouldFail"));
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneAmericaPhoenixShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Phoenix");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:America/Phoenix"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("DTSTART:19670430T020000"), "Daylight savings for Phoenix was not serialized properly.");
        }
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneAmericaPhoenixShouldSerializeProperly2()
    {
        var iCal = CreateTestCalendar("America/Phoenix", DateTime.Now, false);
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:America/Phoenix"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Not.Contain("BEGIN:DAYLIGHT"), "Daylight savings should not exist for Phoenix.");
        }
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneUsMountainStandardTimeShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("US Mountain Standard Time");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:US Mountain Standard Time"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"));
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"));
            Assert.That(serialized, Does.Contain("X-LIC-LOCATION"), "X-LIC-LOCATION was not serialized");
        }
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

        Assert.That(serialized, Does.Contain("TZID:Central America Standard Time"), "Time zone not found in serialization");
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneEasternStandardTimeShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Eastern Standard Time");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.That(serialized, Does.Contain("TZID:Eastern Standard Time"), "Time zone not found in serialization");
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneEuropeMoscowShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Europe/Moscow");
        var serializer = new CalendarSerializer();
        // Unwrap the lines to make it easier to search for specific values
        var serialized = TextUtil.UnwrapLines(serializer.SerializeToString(iCal)!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:Europe/Moscow"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:MSD"), "MSD was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:MSK"), "MSK info was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:MSD"), "MSD was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:MST"), "MST was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:MMT"), "MMT was not serialized");
            Assert.That(serialized, Does.Contain("TZOFFSETFROM:+023017"), "TZOFFSETFROM:+023017 was not serialized");
            Assert.That(serialized, Does.Contain("TZOFFSETTO:+023017"), "TZOFFSETTO:+023017 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19180916T010000"), "DTSTART:19180916T010000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19171228T000000"), "DTSTART:19171228T000000 was not serialized");
            // RDATE may contain multiple dates, separated by a comma
            Assert.That(Regex.IsMatch(serialized, $@"RDATE:.*\b19991031T030000\b", RegexOptions.Compiled, RegexDefaults.Timeout), Is.True, "RDATE:19731028T020000 was not serialized");
        }
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneAmericaChicagoShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Chicago");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:America/Chicago"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:CDT"), "CDT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:CST"), "CST was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:EST"), "EST was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:CWT"), "CWT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:CPT"), "CPT was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19181027T020000"), "DTSTART:19181027T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19450814T180000"), "DTSTART:19450814T180000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19420209T020000"), "DTSTART:19420209T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19360301T020000"), "DTSTART:19360301T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20070311T020000"), "DTSTART:20070311T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20071104T020000"), "DTSTART:20071104T020000 was not serialized");
        }
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneAmericaLosAngelesShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Los_Angeles");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:America/Los_Angeles"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BYDAY=2SU"), "BYDAY=2SU was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:PDT"), "PDT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:PST"), "PST was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:PPT"), "PPT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:PWT"), "PWT was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19180331T020000"), "DTSTART:19180331T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20071104T020000"), "DTSTART:20071104T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20070311T020000"), "DTSTART:20070311T020000 was not serialized");
        }
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneEuropeOsloShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Europe/Oslo");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:Europe/Oslo"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BYDAY=-1SU;BYMONTH=3"), "BYDAY=-1SU;BYMONTH=3 was not serialized");
            Assert.That(serialized, Does.Contain("BYDAY=-1SU;BYMONTH=10"), "BYDAY=-1SU;BYMONTH=10 was not serialized");
        }

    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneAmericaAnchorageShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Anchorage");
        var serializer = new CalendarSerializer();
        // Unwrap the lines to make it easier to search for specific values
        var serialized = TextUtil.UnwrapLines(serializer.SerializeToString(iCal)!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:America/Anchorage"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:AHST"), "AHST was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:AHDT"), "AHDT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:AKST"), "AKST was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:YST"), "YST was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:AHDT"), "AHDT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:LMT"), "LMT was not serialized");
            // RDATE may contain multiple dates, separated by a comma
            Assert.That(Regex.IsMatch(serialized, $@"RDATE:.*\b19731028T020000\b", RegexOptions.Compiled, RegexDefaults.Timeout), Is.True, "RDATE:19731028T020000 was not serialized");
            Assert.That(Regex.IsMatch(serialized, $@"RDATE:.*\b19801026T020000\b", RegexOptions.Compiled, RegexDefaults.Timeout), Is.True, "RDATE:19731028T020000 was not serialized");
            Assert.That(serialized, Does.Not.Contain("RDATE:19670401/P1D"), "RDate was not properly serialized for vtimezone, should be RDATE:19670401T000000");
            Assert.That(serialized, Does.Contain("DTSTART:19420209T020000"), "DTSTART:19420209T020000 was not serialized");
        }
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneAmericaEirunepeShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Eirunepe");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:America/Eirunepe"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:-04"), "-04 was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:-05"), "-05 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19311003T110000"), "DTSTART:19311003T110000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19320401T000000"), "DTSTART:19320401T000000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20080624T000000"), "DTSTART:20080624T000000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:19501201T000000"), "DTSTART:19501201T000000 was not serialized");
            // Should not contain the following
            Assert.That(serialized, Does.Not.Contain("RDATE:19501201T000000/P1D"), "The RDATE was not serialized correctly, should be RDATE:19501201T000000");
        }
    }

    [Test, Category("VTimeZone")]
    public void VTimeZoneAmericaDetroitShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Detroit");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Does.Contain("TZID:America/Detroit"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:EDT"), "EDT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:EPT"), "EPT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:EST"), "EST was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20070311T020000"), "DTSTART:20070311T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20071104T020000"), "DTSTART:20071104T020000 was not serialized");
        }
    }

    [Test]
    public void CalendarWithCustomTimeZonesLoads()
    {
        var data = """
            BEGIN:VCALENDAR
            PRODID:ical.net
            BEGIN:VTIMEZONE
            TZID:Custom Time Zone
            BEGIN:STANDARD
            DTSTART:16010101T030000
            TZOFFSETFROM:+0200
            TZOFFSETTO:+0100
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=4SU;BYMONTH=10
            END:STANDARD
            BEGIN:DAYLIGHT
            DTSTART:16010101T020000
            TZOFFSETFROM:+0100
            TZOFFSETTO:+0200
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=3
            END:DAYLIGHT
            END:VTIMEZONE
            BEGIN:VEVENT
            UID:446677788899995465
            SUMMARY:My Event
            DTSTART;TZID=Custom Time Zone:20240423T114500
            DTEND;TZID=Custom Time Zone:20240423T120000
            DTSTAMP:20250306T143151Z
            END:VEVENT
            END:VCALENDAR
            """;

        var cal = Calendar.Load(data)!;

        Assert.That(cal.TimeZones, Is.Not.Empty);

        var customTimeZone = cal.TimeZoneProvider["Custom Time Zone"];
        var start = Instant.FromUtc(2024, 1, 1, 0, 0).InZone(customTimeZone);
        var results = cal.GetOccurrences(start).ToList();

        Assert.That(results, Has.Count.EqualTo(1));

        Assert.That(results[0].Start, Is.EqualTo(new LocalDateTime(2024, 4, 23, 11, 45).InZoneStrictly(customTimeZone)));
    }

    [Test]
    public void CalendarWithCustomTimeZonesFailsWithTzdbOnlyProvider()
    {
        var data = """
            BEGIN:VCALENDAR
            PRODID:ical.net
            BEGIN:VTIMEZONE
            TZID:Custom Time Zone
            BEGIN:STANDARD
            DTSTART:16010101T030000
            TZOFFSETFROM:+0200
            TZOFFSETTO:+0100
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=4SU;BYMONTH=10
            END:STANDARD
            BEGIN:DAYLIGHT
            DTSTART:16010101T020000
            TZOFFSETFROM:+0100
            TZOFFSETTO:+0200
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=3
            END:DAYLIGHT
            END:VTIMEZONE
            BEGIN:VEVENT
            UID:446677788899995465
            SUMMARY:My Event
            DTSTART;TZID=Custom Time Zone:20240423T114500
            DTEND;TZID=Custom Time Zone:20240423T120000
            DTSTAMP:20250306T143151Z
            END:VEVENT
            END:VCALENDAR
            """;

        var cal = Calendar.Load(data)!;

        // Only allow strict Tzdb with no extra aliases
        cal.TimeZoneProvider = DateTimeZoneProviders.Tzdb;

        Assert.That(cal.TimeZones, Is.Not.Empty);

        Assert.Throws<DateTimeZoneNotFoundException>(() =>
        {
            // Time zone does not exist in Tzdb
            var customTimeZone = cal.TimeZoneProvider["Custom Time Zone"];
        });

        Assert.Throws<DateTimeZoneNotFoundException>(() =>
        {
            // Time zone does not exist in Tzdb
            var customTimeZone = cal.TimeZoneProvider["Eastern Standard Time"];
        });

        var tz = cal.TimeZoneProvider["America/New_York"];
        var start = Instant.FromUtc(2024, 1, 1, 0, 0).InZone(tz);

        Assert.Throws<DateTimeZoneNotFoundException>(() =>
        {
            // Should attempt to evaluate event time zone using Tzdb and fail
            var _ = cal.GetOccurrences(start).ToList();
        });
    }

    [Test]
    public void CalendarWithVTimeZoneOnlyFailsToEvaluateUnknownEventTimeZone()
    {
        var data = """
            BEGIN:VCALENDAR
            PRODID:ical.net
            BEGIN:VTIMEZONE
            TZID:Custom Time Zone
            BEGIN:STANDARD
            DTSTART:16010101T030000
            TZOFFSETFROM:+0200
            TZOFFSETTO:+0100
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=4SU;BYMONTH=10
            END:STANDARD
            BEGIN:DAYLIGHT
            DTSTART:16010101T020000
            TZOFFSETFROM:+0100
            TZOFFSETTO:+0200
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=3
            END:DAYLIGHT
            END:VTIMEZONE
            BEGIN:VEVENT
            UID:446677788899995465
            SUMMARY:My Event
            DTSTART;TZID=America/New_York:20240423T114500
            DTEND;TZID=America/New_York:20240423T120000
            DTSTAMP:20250306T143151Z
            END:VEVENT
            END:VCALENDAR
            """;

        var cal = Calendar.Load(data)!;

        // Only allow VTIMEZONE events
        cal.TimeZoneProvider = cal.CreateTimeZoneProvider();

        var tz = cal.TimeZoneProvider["Custom Time Zone"];
        var start = Instant.FromUtc(2024, 1, 1, 0, 0).InZone(tz);

        Assert.Throws<DateTimeZoneNotFoundException>(() =>
        {
            // Fails to evaluate because America/New_York is not defined
            var _ = cal.GetOccurrences(start).ToList();
        });
    }


    [Test]
    public void CalendarWithVTimeZoneOnlyCanEvaluateUsingUnknownTimeZone()
    {
        var data = """
            BEGIN:VCALENDAR
            PRODID:ical.net
            BEGIN:VTIMEZONE
            TZID:Custom Time Zone
            BEGIN:STANDARD
            DTSTART:16010101T030000
            TZOFFSETFROM:+0200
            TZOFFSETTO:+0100
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=4SU;BYMONTH=10
            END:STANDARD
            BEGIN:DAYLIGHT
            DTSTART:16010101T020000
            TZOFFSETFROM:+0100
            TZOFFSETTO:+0200
            RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=3
            END:DAYLIGHT
            END:VTIMEZONE
            BEGIN:VEVENT
            UID:446677788899995465
            SUMMARY:My Event
            DTSTART;TZID=Custom Time Zone:20240423T114500
            DTEND;TZID=Custom Time Zone:20240423T120000
            DTSTAMP:20250306T143151Z
            END:VEVENT
            END:VCALENDAR
            """;

        var cal = Calendar.Load(data)!;

        // Only allow VTIMEZONE events
        cal.TimeZoneProvider = cal.CreateTimeZoneProvider();

        var tz = DateTimeZoneProviders.Tzdb["America/New_York"];
        var start = Instant.FromUtc(2024, 1, 1, 0, 0).InZone(tz);

        List<Occurrence> results = null!;

        Assert.DoesNotThrow(() =>
        {
            // America/New_York is not defined in the calendar, but it
            // should still be able to evaluate.
            results = cal.GetOccurrences(start).ToList();
        });

        Assert.That(results, Has.Count.EqualTo(1));
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
            Start = CalDateTime.FromDateTime(DateTime.Now, tzId),
            End = CalDateTime.FromDateTime(DateTime.Now.AddHours(1), tzId),
            RecurrenceRule = new(FrequencyType.Daily)
        };
        iCal.Events.Add(calEvent);

        var calEvent2 = new CalendarEvent
        {
            Description = "Test Recurring Event 2",
            Start = CalDateTime.FromDateTime(DateTime.Now.AddHours(2), tzId),
            End = CalDateTime.FromDateTime(DateTime.Now.AddHours(3), tzId),
            RecurrenceRule = new(FrequencyType.Daily)
        };
        iCal.Events.Add(calEvent2);
        return iCal;
    }

    [Test]
    public void VTimeZone_ConvertsToAndFromDateTimeZone()
    {
        var calendar = Calendar.Load(IcsFiles.Google1)!;

        // Only use calendar time zones
        calendar.TimeZoneProvider = calendar.CreateTimeZoneProvider();
        var vtz = calendar.TimeZoneProvider["Europe/Berlin"];

        var converted = VTimeZone.FromDateTimeZone(vtz).ToDateTimeZone();

        var start = Instant.FromUtc(2025, 1, 1, 0, 0);
        var end = Instant.FromUtc(2026, 1, 1, 0, 0);

        var convertedIntervals = converted.GetZoneIntervals(start, end);
        var originalIntervals = vtz.GetZoneIntervals(start, end);

        Assert.That(convertedIntervals, Is.EqualTo(originalIntervals));
    }

    [Test]
    public void VTimeZone_CalendarDateTimeZone_ZoneIntervalMatchesNodaTime()
    {
        var calendar = Calendar.Load(IcsFiles.Google1)!;

        // Only use calendar time zones
        calendar.TimeZoneProvider = calendar.CreateTimeZoneProvider();

        var start = Instant.FromUtc(2025, 1, 1, 0, 0);
        var end = Instant.FromUtc(2026, 1, 1, 0, 0);

        var vtz = calendar.TimeZoneProvider["Europe/Berlin"];
        var calIntervals = vtz.GetZoneIntervals(start, end);

        var tz = DateTimeZoneProviders.Tzdb["Europe/Berlin"];
        var expectedIntervals = tz.GetZoneIntervals(start, end);

        Assert.That(calIntervals, Is.EqualTo(expectedIntervals));
    }

    [Test, Category("VTimeZone")]
    public void VTimeZone_CalendarDateTimeZone_GetZoneInterval_Should_Calculate_Savings_Correctly()
    {
        // Arrange: Create a calendar with a custom VTIMEZONE that has daylight saving time
        var icalString = """
            BEGIN:VCALENDAR
            VERSION:2.0
            PRODID:ical.net
            BEGIN:VTIMEZONE
            TZID:Test/DST
            BEGIN:STANDARD
            DTSTART:19701101T020000
            RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU
            TZOFFSETFROM:-0400
            TZOFFSETTO:-0500
            TZNAME:EST
            END:STANDARD
            BEGIN:DAYLIGHT
            DTSTART:19700308T020000
            RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU
            TZOFFSETFROM:-0500
            TZOFFSETTO:-0400
            TZNAME:EDT
            END:DAYLIGHT
            END:VTIMEZONE
            BEGIN:VEVENT
            DTSTART;TZID=Test/DST:20250615T120000
            DTEND;TZID=Test/DST:20250615T130000
            SUMMARY:Test Event
            UID:test@example.com
            END:VEVENT
            END:VCALENDAR
            """;

        var calendar = Calendar.Load(icalString)!;
        var vtz = calendar.TimeZones.First();

        // Act: Get the DateTimeZone from the VTIMEZONE
        var dateTimeZone = vtz.ToDateTimeZone();

        // Get a zone interval during daylight saving time (June 2025)
        var instantInDst = Instant.FromUtc(2025, 6, 15, 16, 0); // 12:00 EDT = 16:00 UTC
        var dstInterval = dateTimeZone.GetZoneInterval(instantInDst);

        // Get a zone interval during standard time (December 2025)
        var instantInStd = Instant.FromUtc(2025, 12, 15, 17, 0); // 12:00 EST = 17:00 UTC
        var stdInterval = dateTimeZone.GetZoneInterval(instantInStd);

        // Assert: Daylight saving time should have non-zero savings
        using (Assert.EnterMultipleScope())
        {
            // During daylight time (EDT), wall offset is -04:00
            Assert.That(dstInterval.WallOffset.ToTimeSpan(), Is.EqualTo(TimeSpan.FromHours(-4)),
                "Wall offset during DST should be -04:00");

            // Savings should be +01:00 (the difference from standard time)
            Assert.That(dstInterval.Savings.ToTimeSpan(), Is.EqualTo(TimeSpan.FromHours(1)),
                "Savings during DST should be +01:00");

            // Standard offset should be -05:00 (wall offset - savings)
            Assert.That(dstInterval.StandardOffset.ToTimeSpan(), Is.EqualTo(TimeSpan.FromHours(-5)),
                "Standard offset during DST should be -05:00");

            // During standard time (EST), wall offset is -05:00
            Assert.That(stdInterval.WallOffset.ToTimeSpan(), Is.EqualTo(TimeSpan.FromHours(-5)),
                "Wall offset during standard time should be -05:00");

            // Savings should be zero during standard time
            Assert.That(stdInterval.Savings.ToTimeSpan(), Is.EqualTo(TimeSpan.Zero),
                "Savings during standard time should be zero");

            // Standard offset equals wall offset during standard time
            Assert.That(stdInterval.StandardOffset.ToTimeSpan(), Is.EqualTo(TimeSpan.FromHours(-5)),
                "Standard offset during standard time should be -05:00");
        }
    }
}
