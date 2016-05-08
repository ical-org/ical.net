using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodaTime;
using NUnit.Framework;

namespace ical.net.UnitTests
{
    public class RecurrenceCalculatorTests
    {
        private static readonly DateTimeZone _nyTz = NodaUtilities.GetTimeZone("America/New_York");

        [Test]
        public void GetRecurrencesTest()
        {
            const int limit = 5;
            var sunday = DateTimeOffset.Parse("5/1/2016 4:30:32 PM -04:00");
            var today = ZonedDateTime.FromDateTimeOffset(sunday).WithZone(_nyTz);

            //every sunday for 5 occurrences
            var repeatRules = new RecurrenceRuleSet(limit, Duration.FromStandardWeeks(1));
            var foo = new RecurrenceCalculator(today, repeatRules, null);
            var actual = foo.GetRecurrences();

            var expected = new List<ZonedDateTime>(limit);
            var temp = today;
            expected.Add(temp);
            for (var i = 0; expected.Count < limit; i++)
            {
                temp = temp.Plus(Duration.FromStandardWeeks(1));
                expected.Add(temp);
            }
            CollectionAssert.AreEqual(expected.OrderBy(z => z, ZonedDateTime.Comparer.Instant).ToImmutableSortedSet(), actual);
        }
    }
}
