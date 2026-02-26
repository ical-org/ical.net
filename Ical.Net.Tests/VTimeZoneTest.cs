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

    [Test, Category("VTimeZone")]
    public void RecurrenceId_IsCompatibleWith_RecurrenceInstance()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var dt = new CalDateTime("20250930");

        var iCal = CreateTestCalendar("America/Detroit");

        var tzInfo1 = iCal.TimeZones.First().TimeZoneInfos.First();
        tzInfo1.RecurrenceIdentifier = new RecurrenceIdentifier(dt);

        iCal = CreateTestCalendar("America/Detroit");
        var tzInfo2 = iCal.TimeZones.First().TimeZoneInfos.First();
        tzInfo2.RecurrenceId = dt.Date.PlusDays(1).ToCalDateTime();

        iCal = CreateTestCalendar("America/Detroit");
        var tzInfo3 = iCal.TimeZones.First().TimeZoneInfos.First();
        tzInfo3.RecurrenceIdentifier = new RecurrenceIdentifier(dt, RecurrenceRange.ThisAndFuture);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tzInfo1.RecurrenceId, Is.EqualTo(tzInfo1.RecurrenceIdentifier.StartTime));
            Assert.That(tzInfo1.RecurrenceIdentifier.Range, Is.EqualTo(RecurrenceRange.ThisInstance));

            Assert.That(tzInfo1.TzId, Is.EqualTo("America/Detroit"));

            Assert.That(tzInfo2.RecurrenceIdentifier!.StartTime, Is.EqualTo(dt.Date.PlusDays(1).ToCalDateTime()));
            Assert.That(tzInfo2.RecurrenceId, Is.EqualTo(dt.Date.PlusDays(1).ToCalDateTime()));
            Assert.That(tzInfo2.RecurrenceIdentifier.Range, Is.EqualTo(RecurrenceRange.ThisInstance));

            // RecurrenceId only supports ThisInstance implicitly,
            // so RecurrenceInstance with ThisAndFuture returns null
            Assert.That(tzInfo3.RecurrenceIdentifier.Range, Is.EqualTo(RecurrenceRange.ThisAndFuture));
            Assert.That(tzInfo3.RecurrenceId, Is.Null);
        }
#pragma warning restore CS0618 // Type or member is obsolete
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
