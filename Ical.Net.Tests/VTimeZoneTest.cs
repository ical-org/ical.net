//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.TimeZone;
using Ical.Net.Utility;
using NodaTime;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class VTimeZoneTest
{
    [Test, Category(nameof(VTimeZone))]
    public void InvalidTzIdShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new VTimeZone("shouldFail"));
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneFromDateTimeZoneNullZoneShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => CreateTestCalendar("shouldFail"));
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneAmericaPhoenixShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Phoenix");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("TZID:America/Phoenix"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("DTSTART:19670430T020000"), "Daylight savings for Phoenix was not serialized properly.");
        });
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneAmericaPhoenixShouldSerializeProperly2()
    {
        var iCal = CreateTestCalendar("America/Phoenix", DateTime.Now, false);
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("TZID:America/Phoenix"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Not.Contain("BEGIN:DAYLIGHT"), "Daylight savings should not exist for Phoenix.");
        });
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneUsMountainStandardTimeShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("US Mountain Standard Time");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("TZID:US Mountain Standard Time"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"));
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"));
            Assert.That(serialized, Does.Contain("X-LIC-LOCATION"), "X-LIC-LOCATION was not serialized");
        });
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZonePacificKiritimatiShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Pacific/Kiritimati");
        var serializer = new CalendarSerializer();
        Assert.DoesNotThrow(() => serializer.SerializeToString(iCal));
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneCentralAmericaStandardTimeShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Central America Standard Time");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.That(serialized, Does.Contain("TZID:Central America Standard Time"), "Time zone not found in serialization");
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneEasternStandardTimeShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Eastern Standard Time");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.That(serialized, Does.Contain("TZID:Eastern Standard Time"), "Time zone not found in serialization");
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneEuropeMoscowShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Europe/Moscow");
        var serializer = new CalendarSerializer();
        // Unwrap the lines to make it easier to search for specific values
        var serialized = TextUtil.UnwrapLines(serializer.SerializeToString(iCal));

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("TZID:Europe/Moscow"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
        });
        Assert.Multiple(() =>
        {
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
        });
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneAmericaChicagoShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Chicago");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
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
        });
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneAmericaLosAngelesShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Los_Angeles");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
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
        });

        //Assert.IsTrue(serialized.Contains("TZURL:http://tzurl.org/zoneinfo/America/Los_Angeles"), "TZURL:http://tzurl.org/zoneinfo/America/Los_Angeles was not serialized");
        //Assert.IsTrue(serialized.Contains("RDATE:19600424T010000"), "RDATE:19600424T010000 was not serialized");  // NodaTime doesn't match with what tzurl has
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneEuropeOsloShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("Europe/Oslo");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("TZID:Europe/Oslo"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BYDAY=-1SU;BYMONTH=3"), "BYDAY=-1SU;BYMONTH=3 was not serialized");
            Assert.That(serialized, Does.Contain("BYDAY=-1SU;BYMONTH=10"), "BYDAY=-1SU;BYMONTH=10 was not serialized");
        });

    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneAmericaAnchorageShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Anchorage");
        var serializer = new CalendarSerializer();
        // Unwrap the lines to make it easier to search for specific values
        var serialized = TextUtil.UnwrapLines(serializer.SerializeToString(iCal));

        Assert.Multiple(() =>
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
        });
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneAmericaEirunepeShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Eirunepe");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
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
        });

        // Should not contain the following
        Assert.That(serialized, Does.Not.Contain("RDATE:19501201T000000/P1D"), "The RDATE was not serialized correctly, should be RDATE:19501201T000000");
    }

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneAmericaDetroitShouldSerializeProperly()
    {
        var iCal = CreateTestCalendar("America/Detroit");
        var serializer = new CalendarSerializer();
        var serialized = serializer.SerializeToString(iCal);

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Does.Contain("TZID:America/Detroit"), "Time zone not found in serialization");
            Assert.That(serialized, Does.Contain("BEGIN:STANDARD"), "The standard timezone info was not serialized");
            Assert.That(serialized, Does.Contain("BEGIN:DAYLIGHT"), "The daylight timezone info was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:EDT"), "EDT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:EPT"), "EPT was not serialized");
            Assert.That(serialized, Does.Contain("TZNAME:EST"), "EST was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20070311T020000"), "DTSTART:20070311T020000 was not serialized");
            Assert.That(serialized, Does.Contain("DTSTART:20071104T020000"), "DTSTART:20071104T020000 was not serialized");
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

    [Test, Category(nameof(VTimeZone))]
    public void VTimeZoneProvider_ShouldResolve_CustomVTimeZone()
    {
        var ics = IcsFiles.EventWithCustomTz;

        var calendar = Calendar.Load(ics)!;
        var provider = new CalTimeZoneProvider();
        provider.AddRangeFrom(calendar.TimeZones);

        var timeZone = provider.GetZoneOrNull("Custom/Timezone");

        var intervals = timeZone?
            .GetZoneIntervals(Instant
                    .FromUtc(1970, 1, 1, 0, 0, 0),
                Instant.FromUtc(1970, 3, 8, 8, 0, 0))
            .ToList();

        var standardInterval = intervals?.FirstOrDefault(i => i.Name == "CST");
        var daylightInterval = intervals?.FirstOrDefault(i => i.Name == "CDT");

        Assert.Multiple(() =>
        {
            Assert.That(timeZone, Is.Not.Null);
            Assert.That(timeZone?.Id, Is.EqualTo("Custom/Timezone"));

            Assert.That(intervals, Is.Not.Null);
            Assert.That(intervals, Has.Count.EqualTo(2));

            Assert.That(standardInterval, Is.Not.Null);
            Assert.That(standardInterval?.WallOffset, Is.EqualTo(Offset.FromHours(-5)));

            Assert.That(daylightInterval, Is.Not.Null);
            Assert.That(daylightInterval?.WallOffset, Is.EqualTo(Offset.FromHours(-4)));
        });
    }

    [Test, Category(nameof(VTimeZone))]
    public void CustomTimeZone_ShouldHandleStandardAndDST()
    {
        // Explicit timezone rules for each year are provided for clarity
        // The timezone definitions cover the range for all expected occurrences
        // Recurrence without a timezone is undefined
        var ics = IcsFiles.RecEventWithCustomTz;

        var calendar = Calendar.Load(ics)!;
        var provider = new CalTimeZoneProvider();
        provider.AddRangeFrom(calendar.TimeZones);

        TimeZoneResolvers.TimeZoneResolver = tzId =>
            // use custom timezones from the calendar, hand over to default resolver if tzId not found
            provider.GetZoneOrNull(tzId) ??
            TimeZoneResolvers.Default(tzId);

        var timeZone = provider.GetZoneOrNull("Special Timezone");
        var occurrences = calendar.GetOccurrences<CalendarEvent>(
            new CalDateTime(2024, 1, 1)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(timeZone, Is.Not.Null);
            Assert.That(timeZone?.Id, Is.EqualTo("Special Timezone"));

            Assert.That(occurrences, Is.Not.Null);
            // 24 months from 2024-01-01 to 2025-12-31
            Assert.That(occurrences.Count, Is.EqualTo(24));

            // Check the first occurrence - no intervals defined for 2024
            var firstOccurrence = occurrences[0];
            Assert.That(firstOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2024, 1, 1, 9, 0, 0, "Special Timezone")));
            // CST offset -5 hours
            Assert.That(firstOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2024, 1, 1, 14, 0, 0, DateTimeKind.Utc)));

            // Check 8th occurrence
            var eighthOccurrence = occurrences[7];
            Assert.That(eighthOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2024, 8, 1, 9, 0, 0, "Special Timezone")));
            // CDT offset -4 hours
            Assert.That(eighthOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2024, 8, 1, 13, 0, 0, DateTimeKind.Utc)));

            // Check the last occurrence - no intervals defined for 2025-09
            var lastOccurrence = occurrences[occurrences.Count - 1];
            Assert.That(lastOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2025, 12, 1, 9, 0, 0, "Special Timezone")));
            // CST offset -5 hours
            Assert.That(lastOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2025, 12, 1, 14, 0, 0, DateTimeKind.Utc)));
        });
    }

    [Test, Category(nameof(VTimeZone))]
    [TestCase("Custom/America/New_York")] // using custom timezone
    [TestCase("America/New_York")] // using NodaTime
    public void NewYorkTimezone_ShouldHandle_DstTransitions(string tzName)
    {
        // America/New_York definition from the ICS file
        // and the IANA timezone database of NodaTime
        // should give the same results

        // Load the calendar with the custom timezone
        var calendar = Calendar.Load(IcsFiles.AmericaNewYork);
        foreach (var tz in calendar.TimeZones)
        {
            // Change the name for the custom timezone
            tz.TzId = tzName;
        }
        var provider = new CalTimeZoneProvider();
        provider.AddRangeFrom(calendar.TimeZones);

        // Modify the timezone resolver
        TimeZoneResolvers.TimeZoneResolver = tzId =>
            TimeZoneResolvers.Default(tzId)
            // use custom timezone provider for "Custom/America/New_York"
            ?? provider.GetZoneOrNull(tzId);

        // Arrange
        var springForward = new CalDateTime(2023, 3, 12, 2, 30, 0, tzName); // Invalid time (skipped)
        var fallBack = new CalDateTime(2023, 11, 5, 1, 30, 0, tzName);      // Ambiguous time (occurs twice)

        // Act

        // Spring Forward (2:30AM doesn't exist - should map to 3:30AM EDT)
        var springResult = springForward.ToTimeZone(tzName);
        var springOffset = LocalDateTime.FromDateTime(springResult.Value)
            .InZoneLeniently(TimeZoneResolvers.TimeZoneResolver(tzName)).Offset;

        // Fall Back (1:30AM occurs twice - should use daylight instance)
        var fallResult = fallBack.ToTimeZone(tzName);
        var fallOffset = LocalDateTime.FromDateTime(fallResult.Value)
            .InZoneLeniently(TimeZoneResolvers.TimeZoneResolver(tzName)).Offset;

        Assert.Multiple(() =>
        {
            // 3:30AM / EDT offset -4 hours
            Assert.That(springResult.Value.ToString("s"), Is.EqualTo("2023-03-12T03:30:00"));
            Assert.That(springOffset, Is.EqualTo(Offset.FromHours(-4)));
            // 1:30AM / prefers EDT offset -4 hours
            Assert.That(fallResult.Value.ToString("s"), Is.EqualTo("2023-11-05T01:30:00"));
            Assert.That(fallOffset, Is.EqualTo(Offset.FromHours(-4)));
        });
    }

    [Test, Category(nameof(VTimeZone))]
    [TestCase("Custom/Asia/Jerusalem")] // using custom timezone
    [TestCase("Asia/Jerusalem")] // using NodaTime
    public void JerusalemTimezone_ShouldHandle_DstTransitions(string tzName)
    {
        // Load the calendar with the custom timezone
        var calendar = Calendar.Load(IcsFiles.AsiaJerusalem);
        foreach (var tz in calendar.TimeZones)
        {
            // Change the name for the custom timezone
            tz.TzId = tzName;
        }
        var provider = new CalTimeZoneProvider();
        provider.AddRangeFrom(calendar.TimeZones);

        // Modify the timezone resolver
        TimeZoneResolvers.TimeZoneResolver = tzId =>
            TimeZoneResolvers.Default(tzId)
            // use custom timezone provider for "Custom/Asia/Jerusalem"
            ?? provider.GetZoneOrNull(tzId);

        // Arrange
        var springForward = new CalDateTime(2023, 3, 24, 2, 30, 0, tzName); // Invalid time (skipped)
        var fallBack = new CalDateTime(2023, 10, 29, 1, 30, 0, tzName);     // Ambiguous time (occurs twice)

        // Act

        // Spring Forward (2:30AM doesn't exist - should map to 3:30AM IDT)
        var springResult = springForward.ToTimeZone(tzName);
        var springOffset = LocalDateTime.FromDateTime(springResult.Value)
            .InZoneLeniently(TimeZoneResolvers.TimeZoneResolver(tzName)).Offset;

        // Fall Back (1:30AM occurs twice - should use daylight instance)
        var fallResult = fallBack.ToTimeZone(tzName);
        var fallOffset = LocalDateTime.FromDateTime(fallResult.Value)
            .InZoneLeniently(TimeZoneResolvers.TimeZoneResolver(tzName)).Offset;

        Assert.Multiple(() =>
        {
            // 3:30AM / IDT offset +3 hours
            Assert.That(springResult.Value.ToString("s"), Is.EqualTo("2023-03-24T03:30:00"));
            Assert.That(springOffset, Is.EqualTo(Offset.FromHours(3)));
            // 1:30AM / prefers IDT offset +3 hours
            Assert.That(fallResult.Value.ToString("s"), Is.EqualTo("2023-10-29T01:30:00"));
            Assert.That(fallOffset, Is.EqualTo(Offset.FromHours(3)));
        });
    }
}
