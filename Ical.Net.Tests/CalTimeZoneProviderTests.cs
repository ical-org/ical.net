//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.TimeZone;
using NodaTime;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class CalTimeZoneProviderTests
{
    private Func<string, DateTimeZone> _tzResolverBackup = TimeZoneResolvers.TimeZoneResolver;
    private CalTimeZoneProvider _provider;

    [OneTimeSetUp]
    public void Setup()
    {
        _tzResolverBackup = TimeZoneResolvers.TimeZoneResolver;

        // Just needed to create a custom time zone provider
        const string dummyTimeZoneIcs =
            """
            BEGIN:VCALENDAR
            PRODID:-//Dummy//NONSGML ICS//EN
            VERSION:2.0
            BEGIN:VTIMEZONE
            TZID:Custom/TimeZone
            BEGIN:STANDARD
            TZNAME:STD
            DTSTART:20221106T020000
            RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU
            TZOFFSETFROM:-0400
            TZOFFSETTO:-0500
            END:STANDARD
            BEGIN:DAYLIGHT
            TZNAME:DST
            DTSTART:20230312T020000
            RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU
            TZOFFSETFROM:-0500
            TZOFFSETTO:-0400
            END:DAYLIGHT
            END:VTIMEZONE
            END:VCALENDAR
            """;
        var c = Calendar.Load(dummyTimeZoneIcs)!;
        _provider = new CalTimeZoneProvider();
        _provider.AddRangeFrom(c.TimeZones);

        TimeZoneResolvers.TimeZoneResolver = tzId =>
            // use custom timezones from the calendar, hand over to default resolver if tzId not found
            _provider.GetZoneOrNull(tzId) ??
            TimeZoneResolvers.Default(tzId);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        TimeZoneResolvers.TimeZoneResolver = _tzResolverBackup;
    }

    [Test, Category(nameof(CalTimeZoneProvider))]
    public void VTimeZoneProvider_ShouldResolve_CustomVTimeZone()
    {
        var ics = IcsFiles.EventWithCustomTz;
        var calendar = Calendar.Load(ics);
        var firstTimeZone = calendar.TimeZones.First(tz => tz.TzId == "Custom/Timezone");
        var timeZone = firstTimeZone.ToDateTimeZone();

        var intervals = timeZone?
            .GetZoneIntervals(Instant.FromUtc(1970, 1, 1, 0, 0, 0),
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

    [Test, Category(nameof(CalTimeZoneProvider))]
    [TestCase("UTC", 0)]
    [TestCase("UTC+01:00", 1)]
    [TestCase("UTC-05:00", -5)]
    public void VDateTimeZone_ShouldReturnOffsetZone_UtcAndUtcOffset(string tzId, int expectedOffsetHours)
    {
        var ics = IcsFiles.EventWithCustomTz;
        var calendar = Calendar.Load(ics)!;

        var provider = new CalTimeZoneProvider();
        provider.AddRangeFrom(calendar.TimeZones);

        var result = provider.GetZoneOrNull(tzId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.MaxOffset, Is.EqualTo(Offset.FromHours(expectedOffsetHours)));
            Assert.That(result.Id, Is.EqualTo(provider[tzId].Id));
        });
    }

    [Test, Category(nameof(CalTimeZoneProvider))]
    [TestCase("UTC", 0)]
    [TestCase("UTC+01:00", 1)]
    [TestCase("UTC-02:00", -2)]
    public void Provider_ShouldHandle_UtcAndUtcOffset(string utcTzId, int offset)
    {
        var toConvert = new CalDateTime(2023, 6, 1, 14, 0, 0, "UTC");

        // Act
        // The custom provider can delegate to
        // DateTimeZone.Utc and DateTimeZone.Offset
        TimeZoneResolvers.TimeZoneResolver = _provider.GetZoneOrNull;

        var result = toConvert.ToTimeZone(utcTzId);

        Assert.Multiple(() =>
        {
            Assert.That(result.TzId, Is.EqualTo(utcTzId));
            Assert.That(result.Value, Is.EqualTo(toConvert.Value.AddHours(offset)));
        });
    }

    [Test, Category(nameof(CalTimeZoneProvider))]
    public void Occurrences_WithCustomTimeZone()
    {
        var ics = IcsFiles.EventWithCustomTz;
        var calendar = Calendar.Load(ics)!;
        var calTimeZoneProvider = new CalTimeZoneProvider();
        calTimeZoneProvider.AddRangeFrom(calendar.TimeZones);

        TimeZoneResolvers.TimeZoneResolver = tzId =>
            // use custom timezones from the calendar, hand over to default resolver if tzId not found
            calTimeZoneProvider.GetZoneOrNull(tzId) ??
            TimeZoneResolvers.Default(tzId);

        var occ = calendar.GetOccurrences().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(occ, Has.Count.EqualTo(1));
            Assert.That(occ[0].Period.StartTime, Is.EqualTo(new CalDateTime(2025, 11, 1, 9, 0, 0, "Custom/Timezone")));
            // Offset is -4 hours for daylight saving time from 2025-03-08 
            Assert.That(occ[0].Period.StartTime.AsUtc, Is.EqualTo(new DateTime(2025, 11, 1, 13, 0, 0, DateTimeKind.Utc)));
        });
    }
}
