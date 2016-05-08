using System;
using NodaTime;
using NUnit.Framework;

namespace ical.net.UnitTests
{
    class NodaUtilitiesUnitTests
    {
        [Test]
        public void ToTimeZoneTests()
        {
            var now = DateTime.Parse("2016-04-10 10:37:00");
            var actual = NodaUtilities.ToTimeZone(now, "America/New_York", "America/Los_Angeles", TimeSpan.FromHours(-4));
            var la = DateTime.Parse("2016-04-10 07:37:00");
            var expected = new ZonedDateTime(LocalDateTime.FromDateTime(la), NodaUtilities.GetTimeZone("America/Los_Angeles"), Offset.FromHours(-7));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.ToDateTimeUtc(), actual.ToDateTimeUtc());
        }
    }
}
