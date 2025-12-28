//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Utility;
using NodaTime;
using NUnit.Framework;

namespace Ical.Net.Tests.TestHelpers;

internal static class OccurrenceTester
{
    private const string _tzid = "US-Eastern";

    public static void AssertOccurrences(
        Calendar cal,
        CalDateTime? fromDate,
        CalDateTime? toDate,
        DataTypes.Period[] expectedPeriods,
        int eventIndex,
        string? timeZone = null)
    {
        var evt = cal.Events.Skip(eventIndex).First();

        var tz = DateUtil.GetZone(timeZone ?? _tzid);
        var start = fromDate?.ToZonedDateTime(tz).ToInstant();

        var occurrences = toDate == null
            ? evt.GetOccurrences(tz, start).ToList()
            : evt.GetOccurrences(tz, start).TakeWhileBefore(toDate.ToZonedDateTime(tz).ToInstant()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(
                occurrences,
                Has.Count.EqualTo(expectedPeriods.Length),
                $"There should have been {expectedPeriods.Length} occurrences; there were {occurrences.Count}");

            if (evt.RecurrenceRules.Count > 0)
            {
                Assert.That(evt.RecurrenceRules, Has.Count.EqualTo(1));
            }

            for (var i = 0; i < expectedPeriods.Length; i++)
            {
                var start = expectedPeriods[i].StartTime.ToZonedDateTime(tz);

                ZonedDateTime end;

                if (expectedPeriods[i].Duration is { } d)
                {
                    end = start.LocalDateTime
                        .Plus(d.GetNominalPart())
                        .InZone(start.Zone, Evaluator.ResolveFrom(start))
                        .Plus(d.GetTimePart());
                }
                else if (expectedPeriods[i].EndTime is { } periodEnd)
                {
                    end = periodEnd.ToZonedDateTime(tz);
                }
                else
                {
                    throw new Exception("Expected test period must have a duration or an end");
                }

                Assert.That(occurrences[i].Start, Is.EqualTo(start), "Event should start on " + start);
                Assert.That(occurrences[i].End, Is.EqualTo(end), "Event should end on " + end);
            }
        });
    }
}
