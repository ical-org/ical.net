//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class ConcurrentDeserializationTests
{
    [Test]
    public void ConcurrentDeserialization_Test()
    {
        // https://github.com/rianjs/ical.net/issues/40
        var calendars = new List<string>
        {
            IcsFiles.DailyCount2,
            IcsFiles.DailyInterval2,
            IcsFiles.DailyByDay1,
            IcsFiles.RecurrenceDates1,
            IcsFiles.DailyByHourMinute1,
        };

        var deserializedCalendars = calendars.AsParallel().SelectMany(CalendarCollection.Load);
        var materialized = deserializedCalendars.ToList();
        Assert.That(materialized, Has.Count.EqualTo(5));
    }
}