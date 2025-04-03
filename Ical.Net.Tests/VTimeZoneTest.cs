//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NodaTime;
using NUnit.Framework;

namespace Ical.Net.Tests;

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
        // Unwrap the lines to make it easier to search for specific values
        var serialized = TextUtil.UnwrapLines(serializer.SerializeToString(iCal));

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
            // RDATE may contain multiple dates, separated by a comma
            Assert.That(Regex.IsMatch(serialized, $@"RDATE:.*\b19991031T030000\b", RegexOptions.Compiled, RegexDefaults.Timeout), Is.True, "RDATE:19731028T020000 was not serialized");
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
        // Unwrap the lines to make it easier to search for specific values
        var serialized = TextUtil.UnwrapLines(serializer.SerializeToString(iCal));

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
            // RDATE may contain multiple dates, separated by a comma
            Assert.That(Regex.IsMatch(serialized, $@"RDATE:.*\b19731028T020000\b", RegexOptions.Compiled, RegexDefaults.Timeout), Is.True, "RDATE:19731028T020000 was not serialized");
            Assert.That(Regex.IsMatch(serialized, $@"RDATE:.*\b19801026T020000\b", RegexOptions.Compiled, RegexDefaults.Timeout), Is.True, "RDATE:19731028T020000 was not serialized");
            Assert.That(serialized.Contains("RDATE:19670401/P1D"), Is.False, "RDate was not properly serialized for vtimezone, should be RDATE:19670401T000000");
            Assert.That(serialized.Contains("DTSTART:19420209T020000"), Is.True, "DTSTART:19420209T020000 was not serialized");
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

    [Test, Category("VTimeZoneProvider")]
    public void VTimeZoneProvider_ShouldResolve_CustomVTimeZone()
    {
        var ics = IcsFiles.VTimeZone1;

        var calendar = Calendar.Load(ics);

        var timeZone = VTimeZoneProvider.FromCalendar(calendar).GetZoneOrNull("Custom/Timezone");

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

    [Test, Category("VTimeZoneProvider")]
    public void GetZoneOrNull_ShouldReturnUtcZone_WhenTzIdIsUtc()
    {
        var ics = IcsFiles.VTimeZone1;
        var calendar = Calendar.Load(ics);
        var provider = VTimeZoneProvider.FromCalendar(calendar);

        var result = provider.GetZoneOrNull("UTC");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo("UTC"));
            Assert.That(result.Id, Is.EqualTo(provider["UTC"].Id));
        });
    }

    [Test, Category("VTimeZoneProvider")]
    public void ProviderTzIds_ShouldEqualToCalendar()
    {
        var ics = IcsFiles.VTimeZone1;
        var calendar = Calendar.Load(ics);
        var provider = VTimeZoneProvider.FromCalendar(calendar);

        Assert.Multiple(() =>
        {
            Assert.That(provider.Ids.Count, Is.EqualTo(1));
            Assert.That(provider.Ids, Is.EquivalentTo(calendar.TimeZones.Select(tz => tz.TzId)));
            Assert.That(provider.GetSystemDefault(), Is.EqualTo(DateTimeZoneProviders.Tzdb.GetSystemDefault()));
            Assert.That(provider.VersionId, Is.EqualTo("1.0.0"));
        });
    }

    [Test, Category("VTimeZoneProvider")]
    [TestCase("UTC+01:00", 1)]
    [TestCase("UTC-05:00", -5)]
    public void GetZoneOrNull_ShouldReturnOffsetZone_WhenTzIdIsUtcWithOffset(string tzId, int expectedOffsetHours)
    {
        var ics = IcsFiles.VTimeZone1;
        var calendar = Calendar.Load(ics);
        var provider = VTimeZoneProvider.FromCalendar(calendar);

        var result = provider.GetZoneOrNull(tzId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.MaxOffset, Is.EqualTo(Offset.FromHours(expectedOffsetHours)));
        });
    }

    [Test, Category("CustomTimeZone")]
    public void Occurrences_WithinUndefined_TzIntervals()
    {
        // Timezone intervals are defined for July and August 2025 only
        var ics = IcsFiles.VTimeZone3;
        var calendar = Calendar.Load(ics);
        var provider = VTimeZoneProvider.FromCalendar(calendar);

        TimeZoneResolvers.TimeZoneResolver = tzId =>
            TimeZoneResolvers.Default(tzId)
            // use custom timezones from the calendar if not found in the default resolver
            ?? provider.GetZoneOrNull(tzId);

        var timeZone = provider.GetZoneOrNull("July August Timezone");
        var occurrences = calendar.GetOccurrences<CalendarEvent>(
            new CalDateTime(2024, 6, 1),
            new CalDateTime(2025, 10, 1)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(timeZone, Is.Not.Null);
            Assert.That(timeZone?.Id, Is.EqualTo("July August Timezone"));
            Assert.That(occurrences, Is.Not.Null);

            // Check occurrence in June - no intervals defined for 2024
            var juneOccurrence = occurrences.First();
            /*
            Assert.That(juneOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2024, 6, 1, 9, 0, 0, "July August Timezone")));
            Assert.That(juneOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2024, 6, 1, 9, 0, 0, DateTimeKind.Utc))); // No offset
            */
            // Check an occurrence in July
            var julyOccurrence = occurrences.First(o => o.Period.StartTime.Date == new DateOnly(2025, 7, 1));
            Assert.That(julyOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2025, 7, 1, 9, 0, 0, "July August Timezone")));
            Assert.That(julyOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2025, 7, 1, 8, 0, 0, DateTimeKind.Utc))); // +1 hour offset

            // Check an occurrence in August
            var augustOccurrence = occurrences.First(o => o.Period.StartTime.Date == new DateOnly(2025, 8, 1));
            Assert.That(augustOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2025, 8, 1, 9, 0, 0, "July August Timezone")));
            Assert.That(augustOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2025, 8, 1, 10, 0, 0, DateTimeKind.Utc))); // -1 hour offset

            // Check occurrence in September - no intervals defined after August 2025
            var septOccurrence = occurrences.Last();
            /*
            Assert.That(septOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2025, 9, 30, 9, 0, 0, "July August Timezone")));
            Assert.That(septOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2025, 9, 30, 9, 0, 0, DateTimeKind.Utc))); // No offset
            */
        });
    }

    [Test, Category("VTimeZoneProvider")]
    public void Occurrences_WithCustomTimeZone()
    {
        var ics = IcsFiles.VTimeZone1;
        var calendar = Calendar.Load(ics);

        TimeZoneResolvers.TimeZoneResolver = tzId =>
            TimeZoneResolvers.Default(tzId)
            // use custom timezones from the calendar if not found in the default resolver
            ?? VTimeZoneProvider.FromCalendar(calendar).GetZoneOrNull(tzId);

        var occ = calendar.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occ, Has.Count.EqualTo(1));
            Assert.That(occ[0].Period.StartTime, Is.EqualTo(new CalDateTime(2025, 11, 1, 9, 0, 0, "Custom/Timezone")));
            // Offset is -4 hours for daylight saving time from 2025-03-08 
            Assert.That(occ[0].Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2025, 11, 1, 13, 0, 0, DateTimeKind.Utc)));
        });
    }

    [Test, Category("CustomTimeZone")]
    public void CustomTimeZone_ShouldHandleStandardAndDST()
    {
        // Explicit timezone rules for each year are provided for clarity
        // The timezone definitions cover the range for all expected occurrences
        // Recurrence without a timezone is undefined
        var ics = IcsFiles.VTimeZone2;

        var calendar = Calendar.Load(ics);
        var provider = VTimeZoneProvider.FromCalendar(calendar);

        TimeZoneResolvers.TimeZoneResolver = tzId =>
            TimeZoneResolvers.Default(tzId)
            // use custom timezones from the calendar if not found in the default resolver
            ?? provider.GetZoneOrNull(tzId);

        var timeZone = provider.GetZoneOrNull("Special Timezone");
        var occurrences = calendar.GetOccurrences<CalendarEvent>(
            new CalDateTime(2024, 1, 1),
            new CalDateTime(2025, 12, 31)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(timeZone, Is.Not.Null);
            Assert.That(timeZone?.Id, Is.EqualTo("Special Timezone"));

            Assert.That(occurrences, Is.Not.Null);
            // 24 months from 2024-01-01 to 2025-12-31
            Assert.That(occurrences.Count, Is.EqualTo(24));

            // Check the first occurrence - no intervals defined for 2024
            var firstOccurrence = occurrences.First();
            Assert.That(firstOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2024, 1, 1, 9, 0, 0, "Special Timezone")));
            // CST offset -5 hours
            Assert.That(firstOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2024, 1, 1, 14, 0, 0, DateTimeKind.Utc)));

            // Check 8th occurrence
            var eighthOccurrence = occurrences[7];
            Assert.That(eighthOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2024, 8, 1, 9, 0, 0, "Special Timezone")));
            // CDT offset -4 hours
            Assert.That(eighthOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2024, 8, 1, 13, 0, 0, DateTimeKind.Utc)));

            // Check the last occurrence - no intervals defined for 2025-09
            var lastOccurrence = occurrences.Last();
            Assert.That(lastOccurrence.Period.StartTime, Is.EqualTo(new CalDateTime(2025, 12, 1, 9, 0, 0, "Special Timezone")));
            // CST offset -5 hours
            Assert.That(lastOccurrence.Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2025, 12, 1, 14, 0, 0, DateTimeKind.Utc)));
        });
    }
}

