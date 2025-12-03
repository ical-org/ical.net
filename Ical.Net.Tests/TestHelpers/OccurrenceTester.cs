//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System.Linq;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests.TestHelpers;

internal static class OccurrenceTester
{
    public static void AssertOccurrences(
        Calendar cal,
        CalDateTime? fromDate,
        CalDateTime? toDate,
        Period[] expectedPeriods,
        string[]? timeZones,
        int eventIndex
    )
    {
        var evt = cal.Events.Skip(eventIndex).First();

        var occurrences = toDate == null
            ? evt.GetOccurrences(fromDate).ToList()
            : evt.GetOccurrences(fromDate).TakeWhileBefore(toDate).ToList();

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
                var period = new Period(expectedPeriods[i].StartTime, expectedPeriods[i].EffectiveDuration!.Value);

                Assert.That(occurrences[i].Period, Is.EqualTo(period), "Event should occur on " + period);
                if (timeZones != null)
                {
                    Assert.That(period.StartTime.TimeZoneName, Is.EqualTo(timeZones[i]),
                        $"Event {period} should occur in the {timeZones[i]} timezone");
                }
            }
        });
    }
}
